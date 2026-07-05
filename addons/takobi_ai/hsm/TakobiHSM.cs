using System;
using System.Collections.Generic;

namespace TakobiAI.HSM;

/// <summary>
/// Engine-specific logging adapter. Replace the method bodies here to port
/// the FSM to a different engine or logging system.
/// </summary>
static class HSMLogger
{
    public static void LogError(string msg) => Godot.GD.PushError(msg);
    public static void LogWarning(string msg) => Godot.GD.PushWarning(msg);
}

/// <summary>
/// Runs a set of discrete states identified by <typeparamref name="T"/>, transitioning between them
/// based on conditions or events evaluated every <see cref="Tick"/>.
/// </summary>
/// <remarks>
/// <code>
/// var sm = new TakobiHSM&lt;PlayerState&gt;();
///
/// sm.AddState(PlayerState.Idle).OnEnter(...).OnExit(...).OnUpdate(...);
/// sm.AddState(PlayerState.Run).OnEnter(...).OnExit(...).OnUpdate(...);
///
/// sm.AddTransition(PlayerState.Idle, PlayerState.Run).When(() => moveDirection != Vector2.Zero);
/// sm.AddTransition(PlayerState.Run, PlayerState.Idle).When(() => moveDirection == Vector2.Zero);
///
/// sm.SetInitialState(PlayerState.Idle);
/// sm.Start();
///
/// // _Process or _PhysicsProcess:
/// sm.Tick(delta);
/// </code>
/// </remarks>
/// <typeparam name="T">An <see cref="Enum"/> type whose values identify the machine's states.</typeparam>
public partial class TakobiHSM<T> where T : Enum
{
    /// <summary>Fired when the current state times out. Passes the state that timed out.</summary>
    public event Action<T> StateTimeout;

    /// <summary>Fired on every state transition. Passes the previous state then the new state.</summary>
    public event Action<T, T> StateChanged;

    /// <summary>Time in seconds elapsed since the current state was entered.</summary>
    public double TimeInState => elapsed;

    /// <summary>The id of the currently active state. Returns <see langword="default"/> if no state is active.</summary>
    public TakobiState<T> CurrentState => currentState;

    /// <summary>The id of the state that was active before the current one.</summary>
    public T PreviousId => previousId;

    public bool HasStarted => currentState != null;

    private readonly Dictionary<T, TakobiState<T>> states = new(EqualityComparer<T>.Default);

    private TakobiState<T> currentState;

    private T initialId;
    private T previousId;

    private double elapsed;
    private double totalTime;

    private bool initialized;
    private bool cacheDirty;
    private bool locked;
    private bool paused;

    private bool ValidateId(T id)
    {
        if (!states.ContainsKey(id))
        {
            HSMLogger.LogError($"Invalid State Id: '{id}' ");
            return false;
        }
        return true;
    }

    #region Initialization

    /// <summary>
    /// Starts the machine by entering the initial state set via <see cref="SetInitialState"/>.<br/>
    /// The initial state's <see cref="TakobiState{T}.Exit"/> is bypassed since there is no previous state.
    /// </summary>
    public void Start()
    {
        if (!initialized)
        {
            HSMLogger.LogError("invalid initial id, call SetInitialState() first");
            return;
        }

        TransitionTo(initialId, bypassExit: true);
    }

    /// <summary>
    /// Designates <paramref name="id"/> as the state the machine enters on <see cref="Start"/> and <see cref="Reset"/>.
    /// Must be called before <see cref="Start"/>.
    /// </summary>
    /// <param name="id">The id of an already-added state.</param>
    public void SetInitialState(T id)
    {
        if (!states.ContainsKey(id))
        {
            HSMLogger.LogError($"invalid state id: {id}");
            return;
        }

        initialId = id;
        initialized = true;
    }

    /// <summary>
    /// Registers a new state with the given <paramref name="id"/> and returns it for fluent configuration.<br/>
    /// If a state with that id already exists, a warning is pushed and the existing instance is returned.
    /// </summary>
    /// <param name="id">The unique enum value identifying this state.</param>
    /// <returns>The newly created (or existing) <see cref="TakobiState{T}"/>.</returns>
    public TakobiState<T> AddState(T id)
    {
        if (states.TryGetValue(id, out TakobiState<T> value))
        {
            HSMLogger.LogWarning($"State with id: '{id}' exists already");
            return value;
        }

        var state = new TakobiState<T>(id);
        states[id] = state;

        return state;
    }

    #endregion

    #region Tick

    /// <summary>
    /// Advances the machine by <paramref name="dt"/> seconds. Call this every frame from your Node's <c>_Process</c>
    /// or <c>_PhysicsProcess</c>.<br/>
    /// Does nothing if the machine is paused or has no active state.
    /// </summary>
    /// <param name="dt">Delta time in seconds since the last tick.</param>
    public void Tick(double dt)
    {
        if (currentState is null || paused)
            return;

        totalTime += dt; // total time since state machine started

        if (cacheDirty)
            RebuildTransitionCache();

        var startingState = currentState;
        currentState.Update?.Invoke((float)dt);

        elapsed += dt; // total time since current state entered

        // ForceTransition() may have been called during Update
        if (currentState != startingState) return;
        if (locked) return;
        if (IsTimeoutState()) return;

        ProcessEvents();

        if (currentState == startingState)
            EvaluateTransitions();
    }

    #endregion

    #region State Timeout

    private bool IsTimeoutState()
    {
        if (!currentState.Timeout.HasValue || elapsed < currentState.Timeout)
            return false;

        var stateId = currentState.Id;
        var timeoutId = currentState.TimeoutTargetState;

        if (!states.ContainsKey(timeoutId))
        {
            HSMLogger.LogError($"Invalid timeout state id: {timeoutId}");
            return false;
        }

        currentState.OnTimeoutCallback?.Invoke();
        StateTimeout?.Invoke(stateId);
        TransitionTo(timeoutId);

        return true;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Fully shuts down the machine — exits the current state, clears all runtime state,
    /// and marks the machine as uninitialized.<br/>
    /// Call <see cref="SetInitialState"/> and <see cref="Start"/> again to reuse it.
    /// </summary>
    public void Disable()
    {
        currentState?.Exit?.Invoke();
        currentState = null;
        initialized = false;
        pendingEvents.Clear();
        transitionCacheCount = 0;
        elapsed = 0;
        totalTime = 0;
        locked = false;
        paused = false;
    }

    /// <summary>
    /// Transitions immediately back to the initial state, bypassing the <see cref="Lock"/> flag.
    /// </summary>
    public void Reset() => TransitionTo(initialId, respectLocked: false);

    /// <summary>Suspends ticking. The current state is preserved and resumes from where it left off on <see cref="Resume"/>.</summary>
    public void Pause() => paused = true;

    /// <summary>Resumes ticking after a <see cref="Pause"/>.</summary>
    public void Resume() => paused = false;

    /// <summary>
    /// Prevents any automatic or event-driven transitions from firing.
    /// The current state keeps updating. Use <see cref="ForceTransition"/> to move states while locked.
    /// </summary>
    public void Lock() => locked = true;

    /// <summary>Re-enables automatic and event-driven transitions after a <see cref="Lock"/>.</summary>
    public void Unlock() => locked = false;

    /// <summary>Returns <see langword="true"/> if <paramref name="id"/> is the currently active state.</summary>
    public bool IsInState(T id) => currentState != null && EqualityComparer<T>.Default.Equals(id, currentState.Id);

    /// <summary>Returns <see langword="true"/> if a state with <paramref name="id"/> has been added to this machine.</summary>
    public bool HasState(T id) => states.ContainsKey(id);

    /// <summary>
    /// Returns the <see cref="TakobiState{T}"/> for <paramref name="id"/>, or <see langword="default"/> if not found.
    /// Prefer <see cref="TryGetState"/> when the id may not exist.
    /// </summary>
    public TakobiState<T> GetState(T id) => TryGetState(id, out var state) ? state : default;

    /// <summary>
    /// Attempts to retrieve the <see cref="TakobiState{T}"/> for <paramref name="id"/>.
    /// </summary>
    /// <returns><see langword="true"/> if the state was found; <see langword="false"/> otherwise.</returns>
    public bool TryGetState(T id, out TakobiState<T> state) => states.TryGetValue(id, out state);

    /// <summary>
    /// Returns the remaining seconds before the current state times out.
    /// Returns <c>-1</c> if the current state has no timeout configured.
    /// </summary>
    public double RemainingTime()
    {
        if (currentState == null || !currentState.Timeout.HasValue) return -1.0;
        return Math.Max(0.0, currentState.Timeout.Value - elapsed);
    }

    /// <summary>
    /// Returns the timeout progress of the current state as a value between <c>0</c> and <c>1</c>.<br/>
    /// Returns <c>-1</c> if the current state has no timeout configured.
    /// </summary>
    public double StateTimeoutProgress()
    {
        if (currentState == null || !currentState.Timeout.HasValue) return -1.0;
        return Math.Clamp(elapsed / currentState.Timeout.Value, 0.0, 1.0);
    }

    #endregion
}
