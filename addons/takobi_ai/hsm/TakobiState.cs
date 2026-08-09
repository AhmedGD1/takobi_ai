using System.Collections.Generic;
using System;

namespace TakobiAI.HSM;

public sealed class TakobiState<T>(T id) where T : Enum
{
    /// <summary>The unique identifier for this state, based on the enum value.</summary>
    public T Id { get; private set; } = id;

    /// <summary>The state to transition to when this state's <see cref="Timeout"/> elapses.</summary>
    public T TimeoutTargetState { get; private set; }

    /// <summary>Invoked when this state is exited. Subscribe via <see cref="OnExit"/>.</summary>
    public Action Exit { get; private set; }

    /// <summary>Invoked when this state is entered. Subscribe via <see cref="OnEnter"/>.</summary>
    public Action Enter { get; private set; }

    /// <summary>Invoked every tick while this state is active. Subscribe via <see cref="OnUpdate"/>.</summary>
    public Action<float> Update { get; private set; }

    /// <summary>
    /// Invoked when this state times out, before transitioning to <see cref="TimeoutTargetState"/>.
    /// Subscribe via <see cref="OnTimeout"/>.
    /// </summary>
    public Action OnTimeoutCallback { get; private set; }

    /// <summary>Minimum time (in seconds) that must elapse before any transition away from this state is allowed.</summary>
    public double MinDuration { get; private set; }

    /// <summary>
    /// Duration (in seconds) after which the state automatically transitions to <see cref="TimeoutTargetState"/>.
    /// <see langword="null"/> means no timeout is set.
    /// </summary>
    public double? Timeout { get; private set; }

    /// <summary>
    /// Minimum time (in seconds) that must pass after this state was last entered
    /// before it can be entered again.
    /// </summary>
    public double Cooldown { get; private set; }

    /// <summary>The total machine time at which this state was last entered. Used for cooldown evaluation.</summary>
    public double LastFiredTime { get; internal set; } = double.NegativeInfinity;

    /// <summary>
    /// The list of transitions originating from this state, sorted by priority then insertion order.
    /// Managed internally — use <see cref="TakobiHSM{T}.AddTransition"/> to add transitions.
    /// </summary>
    public List<TakobiTransition<T>> Transitions { get; private set; } = [];


    /// <summary>
    /// Registers a callback to be invoked every tick while this state is active.
    /// Multiple callbacks can be registered and will all be called in subscription order.
    /// </summary>
    /// <param name="callback">The delegate receiving the delta time (in seconds) each tick.</param>
    /// <returns>This <see cref="TakobiState{T}"/> instance for fluent chaining.</returns>
    public TakobiState<T> OnUpdate(Action<float> callback)
    {
        Update += callback;
        return this;
    }

    /// <summary>
    /// Registers a callback to be invoked when this state is entered.
    /// Multiple callbacks can be registered and will all be called in subscription order.
    /// </summary>
    /// <param name="callback">The delegate to invoke on entry.</param>
    /// <returns>This <see cref="TakobiState{T}"/> instance for fluent chaining.</returns>
    public TakobiState<T> OnEnter(Action callback)
    {
        Enter += callback;
        return this;
    }

    /// <summary>
    /// Registers a callback to be invoked when this state is exited.
    /// Multiple callbacks can be registered and will all be called in subscription order.
    /// </summary>
    /// <param name="callback">The delegate to invoke on exit.</param>
    /// <returns>This <see cref="TakobiState{T}"/> instance for fluent chaining.</returns>
    public TakobiState<T> OnExit(Action callback)
    {
        Exit += callback;
        return this;
    }

    /// <summary>
    /// Sets the minimum time (in seconds) that must pass after this state was last entered
    /// before it can be entered again. Useful for preventing rapid re-entry into the same state.
    /// </summary>
    /// <param name="duration">Cooldown duration in seconds. Must be greater than zero.</param>
    /// <returns>This <see cref="TakobiState{T}"/> instance for fluent chaining.</returns>
    public TakobiState<T> SetCooldown(double duration)
    {        
        Cooldown = ValidateDuration(duration);
        return this;
    }

    /// <summary>
    /// Returns <see langword="true"/> if this state is currently on cooldown and cannot be entered yet.
    /// </summary>
    /// <param name="totalTime">The current total elapsed time of the state machine.</param>
    public bool IsOnCooldown(double totalTime)
    {
        return totalTime < (LastFiredTime + Cooldown);
    }

    /// <summary>
    /// Registers a callback invoked specifically when this state times out,
    /// just before transitioning to <see cref="TimeoutTargetState"/>.<br/>
    /// Prefer this over <see cref="OnExit"/> when you need to distinguish
    /// a timeout exit from a regular transition exit.
    /// </summary>
    /// <param name="callback">The delegate to invoke on timeout.</param>
    /// <returns>This <see cref="TakobiState{T}"/> instance for fluent chaining.</returns>
    public TakobiState<T> OnTimeout(Action callback)
    {
        OnTimeoutCallback = callback;
        return this;
    }

    /// <summary>
    /// Configures this state to automatically transition to <paramref name="to"/>
    /// after <paramref name="duration"/> seconds have elapsed since entry.
    /// </summary>
    /// <param name="duration">How long (in seconds) to stay in this state before timing out. Clamped to zero minimum.</param>
    /// <param name="to">The state to transition to when the timeout elapses.</param>
    /// <returns>This <see cref="TakobiState{T}"/> instance for fluent chaining.</returns>
    public TakobiState<T> TimeoutAfter(double duration, T to)
    {
        Timeout = ValidateDuration(duration);
        TimeoutTargetState = to;
        return this;
    }

    /// <summary>
    /// Sets the minimum time (in seconds) this state must be active before
    /// any transition away from it is considered, regardless of conditions or events.<br/>
    /// Useful for ensuring animations or effects have time to play out.
    /// </summary>
    /// <param name="duration">Minimum active duration in seconds. Must be greater than zero.</param>
    /// <returns>This <see cref="TakobiState{T}"/> instance for fluent chaining.</returns>
    public TakobiState<T> SetMinDuration(double duration)
    {
        MinDuration = ValidateDuration(duration);
        return this;
    }

    /// <summary>
    /// Attaches <paramref name="machine"/> to this state, forwarding lifecycle events automatically.<br/>
    /// • <b>Enter</b> — resets (if <paramref name="resetOnEnter"/>) then starts the sub-machine.<br/>
    /// • <b>Update</b> — ticks the sub-machine with the current delta time.<br/>
    /// • <b>Exit</b> — pauses the sub-machine.
    /// </summary>
    /// <remarks>
    /// Set <paramref name="resetOnEnter"/> to <see langword="false"/> to resume the sub-machine
    /// from where it was paused on re-entry, instead of restarting from the initial state.
    /// <br/><br/>
    /// <b>Usage:</b><br/>
    /// <c>AddState(PlayerState.Combat).WithSubMachine(combatSM).SetMinDuration(0.2f);</c><br/>
    /// <c>AddState(PlayerState.Patrol).WithSubMachine(patrolSM, resetOnEnter: false);</c>
    /// </remarks>
    /// <typeparam name="TChild">Enum type representing the sub-machine's states.</typeparam>
    /// <param name="machine">The sub-machine to attach. Must be fully configured before calling this.</param>
    /// <param name="resetOnEnter">
    /// <see langword="true"/> (default) restarts the sub-machine on every entry.
    /// <see langword="false"/> resumes from the last paused state.
    /// </param>
    /// <returns>This <see cref="TakobiState{T}"/> instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="machine"/> is <see langword="null"/>.</exception>
    public TakobiState<T> WithSubMachine<TChild>(TakobiHSM<TChild> machine, bool resetOnEnter = true) where TChild : Enum
    {
        Enter += () =>
        {
            if (resetOnEnter) machine.Reset();
            else if (!machine.HasStarted) machine.Start();
            else machine.Resume();
        };
        
        Update += dt => machine.Tick(dt);
        Exit += machine.Pause;
        return this;
    }

    /// <summary>
    /// Internal method for adding a transition originating from this state.
    /// The transition is inserted in sorted order by priority then insertion index.<br/>
    /// Do not call directly — use <see cref="TakobiHSM{T}.AddTransition"/> instead.
    /// </summary>
    /// <param name="to">The target state id.</param>
    /// <param name="insertionCounter">The global insertion counter from the state machine, used for stable ordering.</param>
    /// <returns>The newly created <see cref="TakobiTransition{T}"/>.</returns>
    internal TakobiTransition<T> AddTransition(T to, int insertionCounter)
    {
        var transition = new TakobiTransition<T>(Id, to) { InsertionIndex = insertionCounter };
        Transitions.Add(transition);
        return transition;
    }

    private double ValidateDuration(double duration)
    {
        if (duration < 0.0)
            HSMLogger.LogWarning($"Invalid duration: {duration}, value should be zero or greater. Clamped to 0.");
        return Math.Max(0.0, duration);
    }
}
