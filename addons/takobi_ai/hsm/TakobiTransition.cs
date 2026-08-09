using System;
using System.Collections.Generic;
using FSMEventName = Godot.StringName;

namespace TakobiAI.HSM;

public sealed class TakobiTransition<T>(T from, T to, bool isGlobal = false) where T : Enum
{
    /// <summary>The state this transition originates from.</summary>
    public T From { get; private set; } = from;

    /// <summary>The state this transition leads to.</summary>
    public T To { get; private set; } = to;

    /// <summary>Optional callback invoked when this transition fires. Set via <see cref="Do"/>.</summary>
    public Action Callback { get; private set; }

    /// <summary>
    /// The condition evaluated each tick to determine if this transition should fire.
    /// Set via <see cref="When"/>. Ignored for event-driven transitions — use <see cref="Guard"/> for those.
    /// </summary>
    public Func<bool> Condition { get; private set; }

    /// <summary>
    /// A blocker evaluated before <see cref="Condition"/> or event matching.
    /// If it returns <see langword="false"/>, the transition is skipped entirely.
    /// Set via <see cref="IfOnly"/>.
    /// </summary>
    public Func<bool> Guard { get; private set; }

    /// <summary>The event name this transition listens for. Set via <see cref="OnEvent"/>.</summary>
    public FSMEventName EventName { get; private set; }

    /// <summary>
    /// Overrides the source state's <see cref="TakobiState{T}.MinDuration"/> for this specific transition.
    /// <see langword="null"/> means the state's own min duration is used as fallback.
    /// </summary>
    public double? MinTimeOverride { get; private set; }

    /// <summary>
    /// Evaluation priority. Higher values are checked first within the same state.
    /// Ties are broken by insertion order. Defaults to <c>0</c>.
    /// </summary>
    public int Priority { get; private set; }

    /// <summary>Insertion order index assigned by the state machine. Used as a tiebreaker after <see cref="Priority"/>.</summary>
    public int InsertionIndex { get; internal set; }

    /// <summary>
    /// <see langword="true"/> if this transition was added via 
    /// <see cref="TakobiHSM{T}.AddGlobalTransition"/> and is evaluated 
    /// regardless of the current active state.
    /// </summary>
    public bool IsGlobal { get; private set; } = isGlobal;

    // only used with global transitions
    private readonly HashSet<T> excludedStates = [];
    private readonly HashSet<T> availableStates = [];

    /// <summary>
    /// Sets the condition evaluated each tick to determine whether this transition should fire.<br/>
    /// The transition fires on the first tick the condition returns <see langword="true"/>,
    /// provided no <see cref="IfOnly"/> guard blocks it.
    /// </summary>
    /// <param name="condition">A delegate returning <see langword="true"/> when the transition should fire.</param>
    /// <returns>This <see cref="TakobiTransition{T}"/> instance for fluent chaining.</returns>
    public TakobiTransition<T> When(Func<bool> condition)
    {
        Condition = condition;
        return this;
    }

    /// <summary>
    /// Sets a guard that must pass before this transition is considered at all —
    /// regardless of whether it is condition-driven or event-driven.<br/>
    /// Unlike <see cref="When"/>, a failing guard does not consume the event from the queue.
    /// </summary>
    /// <param name="guard">A delegate returning <see langword="true"/> when the transition is allowed.</param>
    /// <returns>This <see cref="TakobiTransition{T}"/> instance for fluent chaining.</returns>
    public TakobiTransition<T> IfOnly(Func<bool> guard)
    {
        Guard = guard;
        return this;
    }

    /// <summary>
    /// Makes this transition event-driven — it will only be considered when the matching
    /// event is triggered via <see cref="TakobiHSM{T}.TriggerEvent"/>.<br/>
    /// Can be combined with <see cref="IfOnly"/> to add a guard on top of the event check.<br/>
    /// To trigger the same transition via polling instead, use <see cref="When"/> as a separate transition.
    /// </summary>
    /// <param name="eventName">The event name to listen for.</param>
    /// <returns>This <see cref="TakobiTransition{T}"/> instance for fluent chaining.</returns>
    public TakobiTransition<T> OnEvent(FSMEventName eventName)
    {
        EventName = eventName;
        return this;
    }

    /// <summary>
    /// Registers a callback to be invoked when this transition fires,
    /// before the destination state's <see cref="TakobiState{T}.Enter"/> is called.
    /// </summary>
    /// <param name="callback">The delegate to invoke on transition.</param>
    /// <returns>This <see cref="TakobiTransition{T}"/> instance for fluent chaining.</returns>
    public TakobiTransition<T> Do(Action callback)
    {
        Callback = callback;
        return this;
    }

    /// <summary>
    /// Allows this transition to fire immediately on entry, ignoring both the source state's
    /// <see cref="TakobiState{T}.MinDuration"/> and any <see cref="OverrideMinDuration"/> value.<br/>
    /// Useful for unconditional fallthrough transitions that should never be held back.
    /// </summary>
    /// <returns>This <see cref="TakobiTransition{T}"/> instance for fluent chaining.</returns>
    public TakobiTransition<T> ForceInstant()
    {
        MinTimeOverride = 0.0;
        return this;
    }

    /// <summary>
    /// Overrides the minimum time required in the source state before this specific transition
    /// can fire.<br/>
    /// Calling this after <see cref="ForceInstant"/> will overwrite it.
    /// </summary>
    /// <param name="duration">Minimum time in seconds. Must be greater than zero.</param>
    /// <returns>This <see cref="TakobiTransition{T}"/> instance for fluent chaining.</returns>
    public TakobiTransition<T> OverrideMinDuration(double duration)
    {
        if (duration <= 0f)
            HSMLogger.LogWarning($"Invalid Minimum Time duration. Value: -> {duration} <- should be greater than zero");

        MinTimeOverride = Math.Max(0f, duration);
        return this;
    }

    /// <summary>
    /// Sets the evaluation priority of this transition relative to others on the same state.
    /// Higher values are evaluated first. Ties fall back to insertion order.
    /// </summary>
    /// <param name="priority">Priority value. Must be zero or greater.</param>
    /// <returns>This <see cref="TakobiTransition{T}"/> instance for fluent chaining.</returns>
    public TakobiTransition<T> SetPriority(int priority)
    {
        if (priority < 0)
            HSMLogger.LogWarning($"Invalid priority: {priority}, value should be zero or greater");

        Priority = priority;
        return this;
    }

    /// <summary>
    /// Excludes specific states from triggering this global transition —
    /// it will be skipped when the machine is in any of the listed states.<br/>
    /// Only meaningful on global transitions; has no effect on state-specific ones.
    /// </summary>
    /// <remarks>
    /// <b>Usage:</b><br/>
    /// <c>AddGlobalTransition(State.Dead).When(() => hp &lt;= 0).AddException(State.Dead, State.Invincible);</c>
    /// </remarks>
    /// <param name="states">One or more state ids to exclude.</param>
    /// <returns>This <see cref="TakobiTransition{T}"/> instance for fluent chaining.</returns>
    public TakobiTransition<T> AddException(params T[] states)
    {
        if (!IsGlobal)
            HSMLogger.LogWarning("AddException() has no effect on non-global transitions");

        foreach (var state in states)
            excludedStates.Add(state);

        return this;
    }

    /// <summary>
    /// Restricts this global transition to only fire when the machine is in one of the listed states.<br/>
    /// Acts as an allowlist — all other states are ignored. Only meaningful on global transitions.
    /// </summary>
    /// <remarks>
    /// <b>Usage:</b><br/>
    /// <c>AddGlobalTransition(State.Stagger).When(() => gotHit).OnlyFrom(State.Idle, State.Walk, State.Run);</c>
    /// </remarks>
    /// <param name="states">One or more state ids from which this transition is allowed to fire.</param>
    /// <returns>This <see cref="TakobiTransition{T}"/> instance for fluent chaining.</returns>
    public TakobiTransition<T> OnlyFrom(params T[] states)
    {
        if (!IsGlobal)
            HSMLogger.LogWarning("OnlyFrom() has no effect on non-global transitions");

        foreach (var state in states)
            availableStates.Add(state);

        return this;
    }

    internal static int Compare(TakobiTransition<T> a, TakobiTransition<T> b)
    {
        int priorityCompare = b.Priority.CompareTo(a.Priority);
        return priorityCompare != 0 ? priorityCompare : a.InsertionIndex.CompareTo(b.InsertionIndex);
    }

    internal bool IsMinTimeExceeded(double time, double fallbackMinTime)
    {
        double required = MinTimeOverride ?? fallbackMinTime;
        return time >= required;
    }

    internal bool IsGuardBlocked() => Guard != null && !Guard();
    internal bool IsExcluded(T current) => IsGlobal && excludedStates.Contains(current);
    internal bool IsIncluded(T current) => 
        !IsGlobal || 
        availableStates.Count == 0 ||
        availableStates.Contains(current);
}
