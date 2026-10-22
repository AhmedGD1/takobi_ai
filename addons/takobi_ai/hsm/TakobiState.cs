using System;
using System.Collections.Generic;
using Godot;

namespace TakobiAI.HSM;

/// <summary>
/// A single state of a <see cref="TakobiHSM{T}"/>, with enter, update and exit callbacks,
/// a minimum duration and a re-entry cooldown. Created through <see cref="TakobiHSM{T}.AddState"/>.
/// </summary>
/// <remarks>
/// Callbacks are multicast: every <see cref="OnEnter"/>, <see cref="OnUpdate"/> and <see cref="OnExit"/> call
/// adds a subscriber, and subscribers run in the order they were added.
/// </remarks>
/// <example>
/// <code>
/// sm.AddState(PlayerState.Dash)
///   .OnEnter(StartDash)
///   .OnUpdate(dt => MoveDash(dt))
///   .OnExit(EndDash)
///   .SetMinDuration(0.2)
///   .SetCooldown(1.5);
/// </code>
/// </example>
/// <typeparam name="T">The enum type that identifies states.</typeparam>
/// <param name="id">The id of this state.</param>
public sealed class TakobiState<T>(T id) where T : Enum
{
    /// <summary>The id of this state.</summary>
    public T Id { get; } = id;

    /// <summary>Invoked every update while this state is active. Subscribe via <see cref="OnUpdate"/>.</summary>
    public Action<double> UpdateCallback { get; private set; }

    /// <summary>Invoked when this state is entered. Subscribe via <see cref="OnEnter"/>.</summary>
    public Action EnterCallback { get; private set; }

    /// <summary>Invoked when this state is exited. Subscribe via <see cref="OnExit"/>.</summary>
    public Action ExitCallback { get; private set; }

    /// <summary>
    /// Minimum time in seconds this state must be active before the machine can leave it.
    /// Set via <see cref="SetMinDuration"/>.
    /// </summary>
    public double MinDuration { get; private set; }

    /// <summary>
    /// Time in seconds after leaving this state before it can be entered again by a transition
    /// or <see cref="TakobiHSM{T}.TryChangeState"/>. Set via <see cref="SetCooldown"/>.
    /// </summary>
    public double Cooldown { get; private set; }

    /// <summary>
    /// The machine's <see cref="TakobiHSM{T}.TotalElapsed"/> at the moment this state was last exited.
    /// Negative infinity if it has never been left. Used for cooldown evaluation.
    /// </summary>
    public double CapturedElapsed { get; internal set; } = double.NegativeInfinity;

    private List<ISubMachine> subMachines;

    /// <summary>
    /// Adds a callback invoked every update while this state is active, after the machine
    /// advances its timers and before transitions are evaluated.
    /// </summary>
    /// <param name="action">The callback, receiving the delta time in seconds.</param>
    /// <returns>This state, for chaining.</returns>
    public TakobiState<T> OnUpdate(Action<double> action)
    {
        UpdateCallback += action;
        return this;
    }

    /// <summary>Adds a callback invoked when this state is entered.</summary>
    /// <remarks>Changing this machine's state from inside this callback is rejected with an error.</remarks>
    /// <param name="action">The callback to invoke.</param>
    /// <returns>This state, for chaining.</returns>
    public TakobiState<T> OnEnter(Action action)
    {
        EnterCallback += action;
        return this;
    }

    /// <summary>Adds a callback invoked when this state is exited.</summary>
    /// <remarks>Changing this machine's state from inside this callback is rejected with an error.</remarks>
    /// <param name="action">The callback to invoke.</param>
    /// <returns>This state, for chaining.</returns>
    public TakobiState<T> OnExit(Action action)
    {
        ExitCallback += action;
        return this;
    }

    /// <summary>
    /// Sets how long after leaving this state it cannot be entered again.
    /// A state that has never been left is always enterable.
    /// </summary>
    /// <remarks>
    /// The cooldown is measured from the moment the state is <b>exited</b>, not entered.
    /// It applies to transitions and <see cref="TakobiHSM{T}.TryChangeState"/>.
    /// <see cref="TakobiHSM{T}.ChangeState"/> and <see cref="TakobiHSM{T}.Restart"/> ignore it.
    /// Pass <see cref="double.PositiveInfinity"/> for a state that transitions can enter only once (until the machine is disabled).
    /// </remarks>
    /// <param name="duration">Cooldown in seconds. Negative values are clamped to zero.</param>
    /// <returns>This state, for chaining.</returns>
    public TakobiState<T> SetCooldown(double duration)
    {
        Cooldown = Mathf.Max(0.0, duration);
        return this;
    }

    /// <summary>
    /// Sets how long this state must be active before the machine can leave it.
    /// Until then, transitions and <see cref="TakobiHSM{T}.TryChangeState"/> are refused.
    /// </summary>
    /// <remarks>
    /// Useful for letting an animation or effect play out. Individual transitions can bypass it with
    /// <see cref="TakobiTransition{T}.OverrideMinDuration"/>.
    /// </remarks>
    /// <param name="duration">Minimum duration in seconds. Negative values are clamped to zero.</param>
    /// <returns>This state, for chaining.</returns>
    public TakobiState<T> SetMinDuration(double duration)
    {
        MinDuration = Mathf.Max(0.0, duration);
        return this;
    }

    /// <summary>
    /// Attaches <paramref name="machine"/> as a sub-machine that runs while this state is active.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description><b>Enter:</b> after this state's own Enter callbacks, restarts the sub-machine
    /// (if <paramref name="restartOnEnter"/>) and unpauses it.</description></item>
    /// <item><description><b>Update:</b> after this state's own update callbacks, updates the sub-machine with the same delta time.</description></item>
    /// <item><description><b>Events:</b> an event triggered on the parent is offered to the sub-machine first.
    /// If the sub-machine fires a transition for it, the event is consumed; otherwise the parent handles it.</description></item>
    /// <item><description><b>Exit:</b> before this state's own Exit callbacks, with <paramref name="restartOnEnter"/>
    /// disables the sub-machine, which runs its current state's Exit and clears its timers and cooldowns.
    /// Otherwise pauses it, keeping its current state and without running any Exit.</description></item>
    /// </list>
    /// <para>
    /// The order is the same however the state is configured, so it does not matter whether
    /// <c>WithSubMachine</c> is called before or after <see cref="OnEnter"/>, <see cref="OnUpdate"/> and <see cref="OnExit"/>.
    /// </para>
    /// <para>
    /// The sub-machine must be fully configured and have <see cref="TakobiHSM{T}.SetInitialState"/> called
    /// before this state is first entered. With <paramref name="restartOnEnter"/> set to <see langword="false"/>
    /// the sub-machine resumes where it was paused instead of returning to its initial state.
    /// </para>
    /// <para>
    /// <see cref="TakobiHSM{T}.SetInitialState"/> enters the initial state immediately, so a sub-machine's
    /// initial Enter callback runs once at setup and again on the parent's first entry when restarting.
    /// </para>
    /// <c>AddState(PlayerState.Combat).WithSubMachine(combatSM).SetMinDuration(0.2);</c><br/>
    /// <c>AddState(PlayerState.Patrol).WithSubMachine(patrolSM, restartOnEnter: false);</c>
    /// </remarks>
    /// <typeparam name="TChild">The enum type of the sub-machine's states.</typeparam>
    /// <param name="machine">The sub-machine to attach.</param>
    /// <param name="restartOnEnter">
    /// <see langword="true"/> (default) restarts the sub-machine on every entry.
    /// <see langword="false"/> resumes it from where it was paused.
    /// </param>
    /// <returns>This state, for chaining.</returns>
    public TakobiState<T> WithSubMachine<TChild>(TakobiHSM<TChild> machine, bool restartOnEnter = true) where TChild : Enum
    {
        ArgumentNullException.ThrowIfNull(machine);

        (subMachines ??= []).Add(new SubMachineHost<TChild>(machine, restartOnEnter));
        return this;
    }

    internal void InvokeEnter()
    {
        EnterCallback?.Invoke();

        if (subMachines is null)
            return;

        for (int i = 0; i < subMachines.Count; i++)
            subMachines[i].ParentEntered();
    }

    internal void InvokeExit()
    {
        if (subMachines is not null)
        {
            for (int i = 0; i < subMachines.Count; i++)
                subMachines[i].ParentExited();
        }

        ExitCallback?.Invoke();
    }

    internal void InvokeSubMachineUpdate(double delta)
    {
        if (subMachines is null)
            return;

        for (int i = 0; i < subMachines.Count; i++)
            subMachines[i].Update(delta);
    }

    /// <summary>Offers an event to the sub-machines, innermost first. Returns true if one of them fired a transition for it.</summary>
    internal bool OfferEventToSubMachines(StringName eventName)
    {
        if (subMachines is null)
            return false;

        for (int i = 0; i < subMachines.Count; i++)
        {
            if (subMachines[i].TryHandleEvent(eventName))
                return true;
        }

        return false;
    }

    /// <summary>Returns <see langword="true"/> if this state's cooldown has not elapsed and it can't be entered.</summary>
    /// <param name="totalElapsed">The machine's current <see cref="TakobiHSM{T}.TotalElapsed"/>.</param>
    public bool IsOnCooldown(double totalElapsed)
    {
        if (double.IsNegativeInfinity(CapturedElapsed))
            return false;

        return totalElapsed < CapturedElapsed + Cooldown;
    }
}

