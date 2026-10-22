using System;
using System.Collections.Generic;
using Godot;

namespace TakobiAI.HSM;

/// <summary>
/// A hierarchical state machine driven by an enum. States have enter, update and exit callbacks,
/// a minimum duration and a cooldown. Transitions between them are polled or event-driven,
/// evaluated by priority, and can be global. Any state can host a sub-machine
/// (see <see cref="TakobiState{T}.WithSubMachine"/>).
/// </summary>
/// <remarks>
/// <para><b>Update order.</b> Each <see cref="Update"/> call advances the timers, runs the current state's
/// update callback, updates the state's sub-machines, then processes pending events (offered to sub-machines
/// first), then evaluates polled transitions. At most one transition fires per update on this machine.</para>
/// <para><b>Transition rules.</b> A transition fires only if the machine is not <see cref="Locked"/>, the current
/// state's minimum duration (or the transition's override) has passed, and the target state is off cooldown.</para>
/// <para><b>Self-transitions.</b> Transitions never fire into the current state. Use
/// <see cref="ChangeState"/> or <see cref="Restart"/> to deliberately restart a state.</para>
/// </remarks>
/// <example>
/// <code>
/// var sm = new TakobiHSM&lt;PlayerState&gt;();
///
/// sm.AddState(PlayerState.Idle).OnEnter(PlayIdle);
/// sm.AddState(PlayerState.Run).OnUpdate(dt => Move(dt));
/// sm.AddState(PlayerState.Dead).OnEnter(Die);
///
/// sm.AddTransition(PlayerState.Idle, PlayerState.Run).When(() => moveInput != Vector2.Zero);
/// sm.AddTransition(PlayerState.Run, PlayerState.Idle).When(() => moveInput == Vector2.Zero);
/// sm.AddGlobalTransition(PlayerState.Dead).When(() => hp &lt;= 0).SetPriority(10);
///
/// sm.SetInitialState(PlayerState.Idle);
///
/// // _Process or _PhysicsProcess:
/// sm.Update(delta);
/// </code>
/// </example>
/// <typeparam name="T">The enum type that identifies states.</typeparam>
public partial class TakobiHSM<T> where T : Enum
{
    /// <summary>
    /// Represents the method that handles <see cref="StateChanged"/>.
    /// </summary>
    /// <param name="previous">The state that was exited.</param>
    /// <param name="current">The state that was entered.</param>
    public delegate void StateChangedEventHandler(T previous, T current);

    /// <summary>
    /// Raised after every state change, once the new state's Enter callback has run.
    /// It is not raised for the initial state, but it is raised when a state restarts into itself
    /// (for example <see cref="Restart"/> while already in the initial state).
    /// </summary>
    public event StateChangedEventHandler StateChanged;

    /// <summary>The active state, or <see langword="null"/> before <see cref="SetInitialState"/> or after <see cref="Disable"/>.</summary>
    public TakobiState<T> CurrentState { get; private set; }

    /// <summary>The state that was active before the current one, or <see langword="null"/> if there has been no change yet.</summary>
    public TakobiState<T> PreviousState { get; private set; }

    /// <summary>Time in seconds spent in the current state. Reset on every state change.</summary>
    public double StateElapsed { get; private set; }

    /// <summary>Time in seconds this machine has been updated. Reset by <see cref="Disable"/>. Used for cooldowns.</summary>
    public double TotalElapsed { get; private set; }

    private readonly Dictionary<T, TakobiState<T>> states = new(EqualityComparer<T>.Default);
    
    private readonly Queue<StringName> pendingEvents = new();
    private readonly List<TakobiTransition<T>> transitions = [];

    /// <summary>
    /// If <see langword="true"/>, transitions cannot fire, <see cref="TryChangeState"/> returns <see langword="false"/>,
    /// and <see cref="TriggerEvent"/> ignores events. The current state keeps updating.
    /// <see cref="ChangeState"/> and <see cref="Restart"/> still work while locked.
    /// Locking discards any events that are already queued.
    /// </summary>
    public bool Locked
    {
        get => locked;
        set
        {
            locked = value;

            if (value)
                pendingEvents.Clear();
        }
    }

    /// <summary>
    /// If <see langword="true"/>, <see cref="Update"/> does nothing: no timers advance and no callbacks run,
    /// and <see cref="TriggerEvent"/> ignores events. Events queued before the pause are kept.
    /// </summary>
    public bool Paused { get; set; }

    private bool locked;

    private int transitionCounter;
    private int lastTransitionCount;
    private int stateChangeCount;

    private TakobiState<T> initialState;

    #region States

    /// <summary>
    /// Sets the initial state and enters it immediately, which starts the machine.
    /// Can only be called once; later calls are ignored with a warning.
    /// </summary>
    /// <remarks>
    /// Call this after adding states and transitions. <see cref="StateChanged"/> is not raised.
    /// The state is also the target of <see cref="Restart"/>.
    /// </remarks>
    /// <param name="id">The id of a registered state.</param>
    public void SetInitialState(T id)
    {
        if (initialState is not null)
        {
            GD.PushWarning("initial state already set.");
            return;
        }

        if (!states.TryGetValue(id, out var state))
        {
            GD.PushError($"state with id '{id}' doesn't exist.");
            return;
        }

        initialState = state;

        transitions.Sort(TakobiTransition<T>.Compare);
        SetState(id);
    }   

    /// <summary>
    /// Registers a state and returns it for configuration. If the id is already registered,
    /// a warning is pushed and the existing state is returned.
    /// </summary>
    /// <param name="id">The id of the new state.</param>
    /// <returns>The new or existing state.</returns>
    public TakobiState<T> AddState(T id)
    {
        if (states.TryGetValue(id, out var value))
        {
            GD.PushWarning($"state with id '{id}' exists already.");
            return value;
        }

        var newState = new TakobiState<T>(id);
        states[id] = newState;

        return newState;
    }

    /// <summary>
    /// Switches to <paramref name="target"/>, running the old state's Exit and the new state's Enter.
    /// Changing to the current state restarts it.
    /// </summary>
    /// <remarks>
    /// This ignores <see cref="Locked"/>, cooldowns and minimum durations. Use <see cref="TryChangeState"/>
    /// to respect them. Calling this from one of this machine's Enter or Exit callbacks is rejected with an error;
    /// trigger an event or set a flag there and let a transition handle it instead.
    /// An unknown id is reported with an error and nothing changes.
    /// </remarks>
    /// <param name="target">The id of the state to switch to.</param>
    public void ChangeState(T target)
    {
        SetState(target);
    }

    /// <summary>
    /// Switches to <paramref name="target"/> if <see cref="CanChangeState"/> allows it.
    /// </summary>
    /// <param name="target">The id of the state to switch to.</param>
    /// <returns><see langword="true"/> if the state changed; otherwise <see langword="false"/>.</returns>
    public bool TryChangeState(T target)
    {
        if (!CanChangeState(target))
            return false;
        
        SetState(target);
        return true;
    }

    private bool switching;

    private void SetState(T id)
    {
        if (switching)
        {
            GD.PushError($"can't change to state '{id}' from inside an Enter or Exit callback.");
            return;
        }

        if (!states.TryGetValue(id, out var next))
        {
            GD.PushError($"state with id '{id}' doesn't exist.");
            return;
        }

        var previous = CurrentState;

        switching = true;

        try
        {
            if (previous is not null)
            {
                previous.InvokeExit();
                previous.CapturedElapsed = TotalElapsed;
            }

            pendingEvents.Clear();

            PreviousState = previous;

            StateElapsed = 0.0;
            stateChangeCount++;

            CurrentState = next;
            CurrentState.InvokeEnter();
        }
        finally
        {
            switching = false;
        }

        if (previous is not null)
            StateChanged?.Invoke(previous.Id, CurrentState.Id);
    }

    private bool CanEnter(TakobiState<T> target, double minTime)
    {
        return !Locked && StateElapsed >= minTime && !target.IsOnCooldown(TotalElapsed);
    }

    #endregion

    #region Transitions

    /// <summary>
    /// Adds a transition from <paramref name="from"/> to <paramref name="to"/> and returns it for configuration.
    /// </summary>
    /// <remarks>
    /// Configure it with <see cref="TakobiTransition{T}.When"/>, <see cref="TakobiTransition{T}.OnEvent"/> and the
    /// other fluent methods. A transition where <paramref name="from"/> equals <paramref name="to"/> never fires,
    /// so a warning is pushed.
    /// </remarks>
    /// <param name="from">The state the transition leaves.</param>
    /// <param name="to">The state the transition enters.</param>
    /// <returns>The new transition.</returns>
    /// <exception cref="ArgumentException">Either id is not a registered state. Add states before transitions.</exception>
    public TakobiTransition<T> AddTransition(T from, T to)
    {
        if (!states.ContainsKey(from) || !states.ContainsKey(to))
            throw new ArgumentException($"Invalid state id in AddTransition({from}, {to})");
        
        if (EqualityComparer<T>.Default.Equals(from, to))
            GD.PushWarning($"AddTransition({from}, {to}): self-transitions never fire.");

        var transition = new TakobiTransition<T>(from, to, isGlobal: false) { InsertionIndex = transitionCounter++ };
        transitions.Add(transition);

        return transition;
    }

    /// <summary>
    /// Adds a transition into <paramref name="to"/> that can fire from any state except <paramref name="to"/> itself.
    /// </summary>
    /// <remarks>
    /// Useful for universal rules such as dying or being stunned. Narrow it with
    /// <see cref="TakobiTransition{T}.Exclude"/> or <see cref="TakobiTransition{T}.OnlyFrom"/>, and give it a
    /// <see cref="TakobiTransition{T}.SetPriority"/> if it should win over local transitions.
    /// </remarks>
    /// <param name="to">The state the transition enters.</param>
    /// <returns>The new transition.</returns>
    /// <exception cref="ArgumentException"><paramref name="to"/> is not a registered state.</exception>
    public TakobiTransition<T> AddGlobalTransition(T to)
    {
        if (!states.ContainsKey(to))
            throw new ArgumentException($"Invalid state id in AddGlobalTransition({to})");

        var transition = new TakobiTransition<T>(default, to, isGlobal: true) { InsertionIndex = transitionCounter++ };
        transitions.Add(transition);

        return transition;
    }

    /// <summary>
    /// Adds a transition that fires once <paramref name="from"/> has been active for <paramref name="duration"/> seconds.
    /// </summary>
    /// <remarks>
    /// This is an ordinary polled transition, so it respects the lock, minimum duration, cooldown and priority.
    /// Do not call <see cref="TakobiTransition{T}.When"/> on the result, because that replaces the timeout condition.
    /// Use <see cref="TakobiTransition{T}.IfOnly"/> for extra requirements.
    /// </remarks>
    /// <param name="from">The state the transition leaves.</param>
    /// <param name="to">The state the transition enters.</param>
    /// <param name="duration">Time in seconds to spend in <paramref name="from"/> first.</param>
    /// <returns>The new transition.</returns>
    /// <exception cref="ArgumentException">Either id is not a registered state.</exception>
    public TakobiTransition<T> AddTimeoutTransition(T from, T to, double duration)
    {
        return AddTransition(from, to).When(() => StateElapsed >= duration);
    }

    private bool EvaluateTransitions(StringName eventName = null)
    {
        var current = CurrentState;

        foreach (var transition in transitions)
        {
            if (transition.EventName != eventName) 
                continue;

            if (transition.IsBlockedFor(current.Id)) 
                continue;

            double minTime = transition.MinDurationOverride < 0 ? current.MinDuration : transition.MinDurationOverride;

            if (!CanEnter(states[transition.To], minTime))
                continue;

            if (!transition.IsGuardMet())
                continue;

            if (!transition.IsConditionMet())
                continue;

            transition.ActionCallback?.Invoke();

            SetState(transition.To);
            return true;
        }

        return false;
    }

    #endregion

    #region Events

    /// <summary>
    /// Queues an event to be processed on the next <see cref="Update"/>.
    /// </summary>
    /// <remarks>
    /// If the current state hosts sub-machines, the event is offered to them first and consumed if one of them
    /// fires a transition for it; otherwise this machine handles it.
    /// Each event fires at most one transition: the first applicable one listening for it.
    /// An event nobody can act on is discarded. The queue is cleared on every state change, and events
    /// triggered while <see cref="Locked"/> or <see cref="Paused"/>, or before the machine has started, are
    /// ignored, and locking discards queued events, so events never leak into a later state or lock period.
    /// </remarks>
    /// <param name="eventName">The name of the event.</param>
    public void TriggerEvent(StringName eventName)
    {
        if (Locked || Paused || CurrentState is null)
            return;

        pendingEvents.Enqueue(eventName);
    }

    #endregion

    #region Life Cycle

    /// <summary>
    /// Advances the machine. Call this every frame from <c>_Process</c> or <c>_PhysicsProcess</c>.
    /// Does nothing if the machine is paused or has no initial state.
    /// </summary>
    /// <remarks>
    /// Order: advance timers, run the current state's update callback, update its sub-machines, process pending
    /// events (offered to sub-machines first), evaluate polled transitions. If a callback changes the state,
    /// disables the machine, or locks it, the rest of the update is skipped.
    /// At most one transition fires per update on this machine.
    /// </remarks>
    /// <param name="delta">Time in seconds since the last update.</param>
    public void Update(double delta)
    {
        if (CurrentState is null || Paused)
            return;

        if (lastTransitionCount != transitions.Count)
            transitions.Sort(TakobiTransition<T>.Compare);

        lastTransitionCount = transitions.Count;
        
        TotalElapsed += delta;
        StateElapsed += delta;

        int startCount = stateChangeCount;

        CurrentState.UpdateCallback?.Invoke(delta);

        if (CurrentState is null || stateChangeCount != startCount)
            return;

        CurrentState.InvokeSubMachineUpdate(delta);

        if (CurrentState is null || stateChangeCount != startCount || Locked)
            return;

        while (pendingEvents.Count > 0)
        {
            var eventName = pendingEvents.Dequeue();

            // Innermost first: a sub-machine that fires a transition for the event consumes it.
            if (CurrentState.OfferEventToSubMachines(eventName))
            {
                if (CurrentState is null || stateChangeCount != startCount)
                    return;

                continue;
            }

            if (EvaluateTransitions(eventName))
                return;
        }

        EvaluateTransitions();
    }

    /// <summary>
    /// Lets a parent machine offer an event to this one. Handles it in the innermost active sub-machine first,
    /// then in this machine. Returns <see langword="true"/> if a transition fired.
    /// </summary>
    internal bool HandleEventFromParent(StringName eventName)
    {
        if (CurrentState is null || Paused || Locked)
            return false;

        if (CurrentState.OfferEventToSubMachines(eventName))
            return true;

        return EvaluateTransitions(eventName);
    }

    /// <summary>
    /// Shuts the machine down: exits the current state, clears timers, cooldowns, pending events,
    /// and the <see cref="Locked"/> and <see cref="Paused"/> flags.
    /// </summary>
    /// <remarks>
    /// States and transitions are kept, and so is the initial state, so <see cref="Restart"/> starts the machine again.
    /// Use <see cref="Restart"/> alone to return to the initial state while keeping cooldowns and timers.
    /// </remarks>
    public void Disable()
    {
        CurrentState?.InvokeExit();

        PreviousState = CurrentState = null;

        StateElapsed = TotalElapsed = 0.0;

        Locked = Paused = false;

        pendingEvents.Clear();

        foreach (var (_, state) in states)
            state.CapturedElapsed = double.NegativeInfinity;
    }

    /// <summary>
    /// Returns to the initial state, running Exit and Enter. Ignores <see cref="Locked"/>.
    /// </summary>
    /// <remarks>
    /// Cooldowns, <see cref="TotalElapsed"/> and the lock and pause flags are kept.
    /// On a disabled machine this starts it again without raising <see cref="StateChanged"/>.
    /// Does nothing but push an error if <see cref="SetInitialState"/> was never called.
    /// </remarks>
    public void Restart()
    {
        if (!HasInitialState())
        {
            GD.PushError("call SetInitialState() first.");
            return;
        }
        ChangeState(initialState.Id);
    }

    #endregion

    #region Queries
    
    /// <summary>
    /// Returns <see langword="true"/> if <see cref="TryChangeState"/> would succeed right now.
    /// </summary>
    /// <remarks>
    /// Requires a started machine, <see cref="Locked"/> to be off, the current state's minimum duration to have passed,
    /// the target to be registered and off cooldown, and the target to differ from the current state.
    /// </remarks>
    /// <param name="target">The id of the target state.</param>
    public bool CanChangeState(T target)
    {
        return        
            CurrentState is not null &&
            states.TryGetValue(target, out var next) &&
            CanEnter(next, CurrentState.MinDuration) &&
            !IsInState(target);
    }

    /// <summary>Gets the id of the current state, or <c>default</c> if the machine has not started.</summary>
    public T GetCurrentId() => CurrentState is not null ? CurrentState.Id : default;

    /// <summary>Gets the id of the previous state, or <c>default</c> if there has been no change yet.</summary>
    public T GetPreviousId() => PreviousState is not null ? PreviousState.Id : default;

    /// <summary>Tries to get a registered state by id.</summary>
    /// <returns><see langword="true"/> if the state exists.</returns>
    public bool TryGetState(T id, out TakobiState<T> state) => states.TryGetValue(id, out state);

    /// <summary>Gets a registered state by id, or <see langword="null"/> if none is registered.</summary>
    public TakobiState<T> GetStateOrNull(T id) => states.TryGetValue(id, out var state) ? state : null;

    /// <summary>Returns <see langword="true"/> if <paramref name="id"/> is the current state.</summary>
    public bool IsInState(T id) => CurrentState is not null && EqualityComparer<T>.Default.Equals(CurrentState.Id, id);
   
    /// <summary>Returns <see langword="true"/> if a state with <paramref name="id"/> is registered.</summary>
    public bool HasState(T id) => states.ContainsKey(id);

    /// <summary>Returns <see langword="true"/> if an initial state has been set.</summary>
    public bool HasInitialState() => initialState is not null;

    #endregion
}
