using System;
using System.Collections.Generic;
using Godot;

namespace TakobiAI.HSM;

/// <summary>
/// A rule for moving a <see cref="TakobiHSM{T}"/> from one state to another.
/// Created through <see cref="TakobiHSM{T}.AddTransition"/>, <see cref="TakobiHSM{T}.AddGlobalTransition"/>
/// or <see cref="TakobiHSM{T}.AddTimeoutTransition"/> and configured with the fluent methods below.
/// </summary>
/// <remarks>
/// <para>
/// A transition is either <b>polled</b> or <b>event-driven</b>:
/// </para>
/// <list type="bullet">
/// <item><description>
/// <b>Polled</b> (no <see cref="OnEvent"/>): evaluated every update. Fires when <see cref="When"/> returns <see langword="true"/>.
/// A polled transition without a condition never fires.
/// </description></item>
/// <item><description>
/// <b>Event-driven</b> (<see cref="OnEvent"/>): evaluated only when the matching event is triggered.
/// Fires if <see cref="When"/> passes, or immediately if no condition is set.
/// </description></item>
/// </list>
/// <para>
/// Before the condition runs, the machine checks (in this order): the transition applies to the current state,
/// the minimum duration has passed, the target is off cooldown, the machine is not locked, and <see cref="IfOnly"/> passes.
/// Transitions are evaluated by <see cref="Priority"/> (highest first), then by insertion order.
/// </para>
/// <para>
/// A transition never fires into the state the machine is already in, so self-transitions are ignored.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// sm.AddTransition(State.Idle, State.Run).When(() => moveInput != Vector2.Zero);
/// sm.AddTransition(State.Run, State.Jump).OnEvent("jump").IfOnly(() => IsOnFloor());
/// sm.AddGlobalTransition(State.Dead).When(() => hp &lt;= 0).Exclude(State.Dead).SetPriority(10);
/// </code>
/// </example>
/// <typeparam name="T">The enum type that identifies states.</typeparam>
/// <param name="from">The state this transition leaves. Ignored for global transitions.</param>
/// <param name="to">The state this transition enters.</param>
public sealed class TakobiTransition<T> where T : Enum
{
    /// <summary>
    /// <see langword="true"/> if this transition can fire from any state.
    /// Restrict it with <see cref="Exclude"/> or <see cref="OnlyFrom"/>.
    /// </summary>
    public bool IsGlobal { get; init; }

    /// <summary>The state this transition leaves. Meaningless for global transitions.</summary>
    public T From { get; init; }

    /// <summary>The state this transition enters.</summary>
    public T To { get; init; }

    /// <summary>Order in which the transition was added. Breaks ties between equal priorities.</summary>
    public int InsertionIndex { get; internal set; }

    /// <summary>Evaluation priority. Higher values are evaluated first. Defaults to <c>0</c>. Set via <see cref="SetPriority"/>.</summary>
    public int Priority { get; private set; }

    /// <summary>Callback invoked when this transition fires. Set via <see cref="Callback"/>.</summary>
    public Action ActionCallback { get; private set; }

    /// <summary>The condition for this transition. Set via <see cref="When"/>.</summary>
    public Func<bool> Condition { get; private set; }

    /// <summary>A blocker checked before <see cref="Condition"/>. Set via <see cref="IfOnly"/>.</summary>
    public Func<bool> Guard { get; private set; }

    /// <summary>The event this transition listens for, or <see langword="null"/> if it is polled. Set via <see cref="OnEvent"/>.</summary>
    public StringName EventName { get; private set; }

    /// <summary>
    /// Minimum time in the source state before this transition may fire, overriding
    /// <see cref="TakobiState{T}.MinDuration"/>. A negative value (the default) means no override.
    /// Set via <see cref="OverrideMinDuration"/>.
    /// </summary>
    public double MinDurationOverride { get; private set; } = -1.0;

    /// <summary><see langword="true"/> if this transition is event-driven.</summary>
    public bool HasEvent => EventName is not null;

    private readonly HashSet<T> excludedStates;
    private readonly HashSet<T> availableStates;

    /// <summary>
    /// Orders transitions for evaluation: higher <see cref="Priority"/> first,
    /// then lower <see cref="InsertionIndex"/> first.
    /// </summary>
    public static int Compare(TakobiTransition<T> a, TakobiTransition<T> b)
    {
        int priorityCompare = b.Priority.CompareTo(a.Priority);
        return priorityCompare != 0 ? priorityCompare : a.InsertionIndex.CompareTo(b.InsertionIndex);
    }

    public TakobiTransition(T from, T to, bool isGlobal)
    {
        From = from;
        To = to;

        IsGlobal = isGlobal;

        if (IsGlobal)
        {
            excludedStates = [];
            availableStates = [];
        }
    }

    /// <summary>
    /// Sets the condition that decides whether this transition fires.
    /// </summary>
    /// <remarks>
    /// Polled transitions fire on the first update where the condition returns <see langword="true"/>.
    /// On an event-driven transition the condition is an extra requirement on top of the event.
    /// Calling this again replaces the previous condition. On a transition created by
    /// <see cref="TakobiHSM{T}.AddTimeoutTransition"/> it replaces the timeout, so use <see cref="IfOnly"/> there instead.
    /// </remarks>
    /// <param name="condition">Returns <see langword="true"/> when the transition should fire.</param>
    /// <returns>This transition, for chaining.</returns>
    public TakobiTransition<T> When(Func<bool> condition)
    {
        Condition = condition;
        return this;
    }

    /// <summary>
    /// Sets a guard that must return <see langword="true"/> for this transition to be considered.
    /// </summary>
    /// <remarks>
    /// The guard runs after the cheap checks (state, minimum duration, cooldown, lock) and before <see cref="When"/>.
    /// It applies to both polled and event-driven transitions. A failing guard just skips this transition,
    /// so other transitions listening for the same event still get their chance.
    /// Calling this again replaces the previous guard.
    /// </remarks>
    /// <param name="guard">Returns <see langword="true"/> when the transition is allowed.</param>
    /// <returns>This transition, for chaining.</returns>
    public TakobiTransition<T> IfOnly(Func<bool> guard)
    {
        Guard = guard;
        return this;
    }

    /// <summary>
    /// Makes this transition event-driven. It is only considered when <paramref name="eventName"/> is triggered
    /// via <see cref="TakobiHSM{T}.TriggerEvent"/>, and is never polled.
    /// </summary>
    /// <remarks>
    /// Use <see cref="When"/> or <see cref="IfOnly"/> to add extra requirements. To react to the same
    /// situation by polling as well, add a separate polled transition.
    /// </remarks>
    /// <param name="eventName">The event to listen for.</param>
    /// <returns>This transition, for chaining.</returns>
    public TakobiTransition<T> OnEvent(StringName eventName)
    {
        EventName = eventName;
        return this;
    }

    /// <summary>
    /// Sets a callback invoked when this transition fires, before the source state's Exit
    /// and before the destination state's Enter.
    /// </summary>
    /// <param name="action">The callback to invoke.</param>
    /// <returns>This transition, for chaining.</returns>
    public TakobiTransition<T> Callback(Action action)
    {
        ActionCallback = action;
        return this;
    }

    /// <summary>
    /// Overrides the source state's <see cref="TakobiState{T}.MinDuration"/> for this transition only.
    /// </summary>
    /// <remarks>Pass <c>0</c> to let this transition fire immediately, ignoring the state's minimum duration.</remarks>
    /// <param name="duration">Minimum time in seconds. Negative values are clamped to zero.</param>
    /// <returns>This transition, for chaining.</returns>
    public TakobiTransition<T> OverrideMinDuration(double duration)
    {
        MinDurationOverride = Mathf.Max(0.0, duration);
        return this;
    }

    /// <summary>
    /// Sets the evaluation priority. Higher values are evaluated first; ties fall back to insertion order.
    /// </summary>
    /// <remarks>
    /// Set priorities while building the machine, before the first update. The machine sorts its transitions
    /// when the transition count changes, so a priority changed later is not picked up until then.
    /// </remarks>
    /// <param name="priority">The priority. Negative values are allowed and run after the default.</param>
    /// <returns>This transition, for chaining.</returns>
    public TakobiTransition<T> SetPriority(int priority)
    {
        Priority = priority;
        return this;
    }

    /// <summary>
    /// Prevents this global transition from firing while the machine is in any of the given states.
    /// Only meaningful on global transitions.
    /// </summary>
    /// <remarks>
    /// The target state never needs to be excluded, because a transition never fires into the current state.
    /// <para><c>AddGlobalTransition(State.Stagger).When(() => gotHit).Exclude(State.Dead, State.Invincible);</c></para>
    /// </remarks>
    /// <param name="states">The states to exclude.</param>
    /// <returns>This transition, for chaining.</returns>
    public TakobiTransition<T> Exclude(params T[] states)
    {
        if (!IsGlobal)
        {
            GD.PushWarning($"{nameof(Exclude)}() has no effect on non-global transitions.");
            return this;
        }
        
        foreach (var state in states)
            excludedStates.Add(state);
        return this;
    }

    /// <summary>
    /// Restricts this global transition to fire only while the machine is in one of the given states.
    /// Only meaningful on global transitions.
    /// </summary>
    /// <remarks>
    /// This acts as an allowlist: all other states are ignored. If <see cref="Exclude"/> is also used,
    /// a state must be allowed by both. Without any call to this method, every state is allowed.
    /// <para><c>AddGlobalTransition(State.Stagger).When(() => gotHit).OnlyFrom(State.Idle, State.Walk);</c></para>
    /// </remarks>
    /// <param name="states">The states this transition may fire from.</param>
    /// <returns>This transition, for chaining.</returns>
    public TakobiTransition<T> OnlyFrom(params T[] states)
    {
        if (!IsGlobal)
        {
            GD.PushWarning($"{nameof(OnlyFrom)}() has no effect on non-global transitions.");
            return this;
        }
        
        foreach (var state in states)
            availableStates.Add(state);
        return this;
    }

    /// <summary>
    /// Returns <see langword="true"/> if this transition does not apply while the machine is in
    /// <paramref name="current"/>.
    /// </summary>
    /// <remarks>
    /// A transition is blocked when it already leads to the current state, when a local transition
    /// starts from a different state, or when a global transition is excluded or not allowed for the current state.
    /// </remarks>
    /// <param name="current">The id of the machine's current state.</param>
    public bool IsBlockedFor(T current)
    {
        var eq = EqualityComparer<T>.Default;

        if (eq.Equals(To, current)) 
            return true;

        if (!IsGlobal) 
            return !eq.Equals(From, current);

        return excludedStates.Contains(current) || (availableStates.Count > 0 && !availableStates.Contains(current));
    }

    /// <summary>
    /// Evaluates <see cref="Condition"/>. With no condition set, returns <see langword="true"/> for
    /// event-driven transitions (the event is enough) and <see langword="false"/> for polled ones.
    /// </summary>
    public bool IsConditionMet() => Condition?.Invoke() ?? HasEvent;

    /// <summary>Evaluates <see cref="Guard"/>. Returns <see langword="true"/> if no guard is set.</summary>
    public bool IsGuardMet() => Guard?.Invoke() ?? true;
}
