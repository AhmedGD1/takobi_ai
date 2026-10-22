# TakobiHSM — Hierarchical State Machine

`TakobiHSM<T>` is an enum-driven hierarchical state machine for Godot 4 (C#). States have Enter / Update / Exit callbacks, a minimum duration and a re-entry cooldown. Transitions are polled or event-driven, evaluated by priority, and can be global. Any state can host one or more sub-machines.

It is a plain C# class, not a `Node`. You own it, configure it once, and call `Update(delta)` from `_Process` or `_PhysicsProcess`.

```csharp
using TakobiAI.HSM;
```

## Contents

- [Quick start](#quick-start)
- [Core concepts](#core-concepts)
- [States](#states)
- [Transitions](#transitions)
- [Events](#events)
- [Sub-machines](#sub-machines)
- [Controlling the machine](#controlling-the-machine)
- [Update order](#update-order)
- [Behavior notes](#behavior-notes)
- [Full example](#full-example)
- [API reference](#api-reference)

---

## Quick start

```csharp
public enum PlayerState { Idle, Run, Dead }

var sm = new TakobiHSM<PlayerState>();

sm.AddState(PlayerState.Idle).OnEnter(PlayIdle);
sm.AddState(PlayerState.Run).OnUpdate(dt => Move(dt));
sm.AddState(PlayerState.Dead).OnEnter(Die);

sm.AddTransition(PlayerState.Idle, PlayerState.Run).When(() => moveInput != Vector2.Zero);
sm.AddTransition(PlayerState.Run, PlayerState.Idle).When(() => moveInput == Vector2.Zero);
sm.AddGlobalTransition(PlayerState.Dead).When(() => hp <= 0).SetPriority(10);

sm.SetInitialState(PlayerState.Idle);   // enters Idle and starts the machine

// _Process or _PhysicsProcess:
sm.Update(delta);
```

Setup order matters:

1. `AddState` for every state.
2. `AddTransition` / `AddGlobalTransition` / `AddTimeoutTransition` (these throw `ArgumentException` if a state isn't registered yet).
3. `SetInitialState` last. It enters the state immediately.

---

## Core concepts

| Concept | Meaning |
|---|---|
| **State** | One value of your enum, with callbacks, `MinDuration` and `Cooldown`. |
| **Transition** | A rule for leaving one state for another. Local (`from → to`) or global (any state → `to`). |
| **Polled transition** | No event set. Evaluated every update; fires when its condition returns `true`. |
| **Event-driven transition** | Listens for a named event. Evaluated only when that event is processed. |
| **Sub-machine** | Another `TakobiHSM<TChild>` that runs while a parent state is active. |
| **Lock** | Stops transitions and events but lets the current state keep updating. |
| **Pause** | Stops the whole machine: no timers, no callbacks. |

At most **one transition fires per update** on a given machine.

---

## States

Add a state with `AddState`. It returns the state for fluent configuration.

```csharp
sm.AddState(PlayerState.Dash)
  .OnEnter(StartDash)
  .OnUpdate(dt => MoveDash(dt))
  .OnExit(EndDash)
  .SetMinDuration(0.2)
  .SetCooldown(1.5);
```

Adding an id twice pushes a warning and returns the existing state.

### Callbacks

`OnEnter`, `OnUpdate` and `OnExit` are **multicast**: every call adds a subscriber, and subscribers run in the order they were added.

- `OnUpdate(Action<double>)` receives the delta time. It runs every update while the state is active, after the timers advance and before transitions are evaluated.
- `OnEnter` / `OnExit` must not change this machine's state. `ChangeState`, `TryChangeState` and `Restart` called from inside them are rejected with an error. Set a flag or trigger an event and let a transition handle it.

### Minimum duration

`SetMinDuration(seconds)` is how long the state must have been active before the machine can leave it. Until then, transitions and `TryChangeState` are refused. Useful for letting an animation or effect play out.

A single transition can bypass it with [`OverrideMinDuration`](#transition-options). `ChangeState` and `Restart` ignore it.

### Cooldown

`SetCooldown(seconds)` is how long after **leaving** the state before it can be entered again. The cooldown is measured from the exit, not the entry.

- It applies to transitions and `TryChangeState`.
- `ChangeState` and `Restart` ignore it.
- A state that has never been left is always enterable.
- `double.PositiveInfinity` makes a state that transitions can enter only once, until the machine is disabled.

Cooldowns are measured in the machine's `TotalElapsed`, which only advances while the machine is being updated.

---

## Transitions

```csharp
sm.AddTransition(State.Idle, State.Run).When(() => moveInput != Vector2.Zero);
sm.AddTransition(State.Run, State.Jump).OnEvent("jump").IfOnly(() => IsOnFloor());
sm.AddGlobalTransition(State.Dead).When(() => hp <= 0).SetPriority(10);
sm.AddTimeoutTransition(State.Attack, State.Idle, 0.4);
```

### Kinds

| Method | Meaning |
|---|---|
| `AddTransition(from, to)` | Local transition. Only considered while the machine is in `from`. |
| `AddGlobalTransition(to)` | Can fire from any state. Narrow it with `Exclude` or `OnlyFrom`. |
| `AddTimeoutTransition(from, to, seconds)` | Polled transition that fires once `from` has been active for `seconds`. |

A transition **never fires into the state the machine is already in**. Self-transitions are ignored (and `AddTransition(a, a)` pushes a warning). To deliberately restart a state, use `ChangeState` or `Restart`.

### Polled vs. event-driven

- **Polled** (no `OnEvent`): evaluated every update. Fires when `When` returns `true`. A polled transition with no condition **never fires**.
- **Event-driven** (`OnEvent`): evaluated only when that event is triggered. Fires if `When` passes, or immediately if no `When` is set. It is never polled.

To react to the same situation both ways, add two transitions.

### Transition options

| Method | Effect |
|---|---|
| `When(Func<bool>)` | The condition. Replaces any earlier condition. |
| `IfOnly(Func<bool>)` | A guard checked before `When`, for polled and event-driven transitions. A failing guard just skips this transition, so other transitions for the same event still get a chance. Replaces any earlier guard. |
| `OnEvent(StringName)` | Makes the transition event-driven. |
| `Callback(Action)` | Runs when the transition fires, **before** the source state's Exit and the target state's Enter. Don't change this machine's state from inside it. |
| `OverrideMinDuration(seconds)` | Replaces the source state's `MinDuration` for this transition only. `0` fires immediately. |
| `SetPriority(int)` | Higher values are evaluated first. Default `0`; negatives run after the default. |
| `Exclude(params T[])` | Global only. Never fires from these states. |
| `OnlyFrom(params T[])` | Global only. Fires only from these states (an allowlist). If used together with `Exclude`, a state must pass both. |

`Exclude` and `OnlyFrom` on a non-global transition push a warning and do nothing. The target state never needs excluding, because a transition never fires into the current state.

> **`AddTimeoutTransition` and `When`:** the timeout *is* the condition, so calling `When` on the result replaces it. Use `IfOnly` for extra requirements.

### What is checked, and in what order

For each transition, in priority order, the machine checks:

1. The transition applies to the current state (local `From` matches, or global and not excluded/allowed).
2. The machine is not locked, the minimum duration has passed (the transition's override, else the state's `MinDuration`), and the target is off cooldown.
3. `IfOnly` passes.
4. `When` passes (or, event-driven with no `When`, the event is enough).

The first transition that passes everything fires. Order is **priority descending, then insertion order**.

> **Set priorities while building the machine.** Transitions are sorted when `SetInitialState` runs and again when the transition count changes. A priority changed later is not picked up until then.

---

## Events

```csharp
sm.AddTransition(State.Run, State.Jump).OnEvent("jump").IfOnly(IsOnFloor);

// somewhere else:
sm.TriggerEvent("jump");
```

`TriggerEvent` queues the event. It is processed during `Update`, after the state's update callbacks and sub-machine updates (so an event triggered from inside an update callback is handled in that same update).

Rules:

- Each event fires **at most one** transition: the first applicable one listening for it.
- An event nobody can act on is **discarded**.
- Events are **ignored** if the machine is `Locked`, `Paused`, or not started.
- Locking discards events already queued.
- The queue is cleared on every state change, so events never leak into a later state. Events triggered from an Enter callback are kept for the next update.
- If the current state hosts sub-machines, the event is offered to them first. See [Sub-machines](#sub-machines).

---

## Sub-machines

Any state can host another machine. While the parent state is active, the sub-machine runs.

```csharp
AddState(PlayerState.Combat).WithSubMachine(combatSM);
AddState(PlayerState.Patrol).WithSubMachine(patrolSM, restartOnEnter: false);
```

The sub-machine can use a different enum (`TakobiHSM<TChild>`). It must be fully configured, with `SetInitialState` called, before the parent state is first entered.

### Lifecycle

The order is fixed however the state is configured, so it does not matter whether `WithSubMachine` is called before or after `OnEnter` / `OnUpdate` / `OnExit`.

| When | What happens |
|---|---|
| **Parent enter** | After the parent's own Enter callbacks: restarts the sub-machine (if `restartOnEnter`) and unpauses it. |
| **Parent update** | After the parent's own update callbacks: updates the sub-machine with the same delta. |
| **Parent exit** | Before the parent's own Exit callbacks. With `restartOnEnter`, **disables** the sub-machine (runs its current state's Exit, clears its timers, cooldowns, lock and pause). Otherwise **pauses** it, keeping its state and running no Exit. |

### `restartOnEnter`

- `true` (default): every parent entry returns the sub-machine to its initial state, with a clean slate.
- `false`: the sub-machine resumes exactly where it was paused. Its timers and cooldowns are frozen while the parent is inactive.

### Events and sub-machines

An event triggered on the parent is offered to the sub-machine **first** (innermost first for nested machines). If the sub-machine fires a transition for it, the event is consumed. Otherwise the parent handles it.

This includes the case where the sub-machine *has* a matching transition but it is blocked by a lock, minimum duration, cooldown, guard or condition: it did not fire, so the event falls through to the parent.

If a state hosts several sub-machines, they are offered the event in the order they were added, and the first to fire consumes it.

### Things to know

- **The initial state's Enter runs at setup.** `SetInitialState` enters the initial state immediately, so a sub-machine's initial Enter callback runs once when you configure it. With `restartOnEnter: true` it runs again on the parent's first entry (the restart exits and re-enters it). With `restartOnEnter: false` it does **not** run again when the parent is entered. Give a sub-machine an initial state whose Enter/Exit have no side effects, or keep those callbacks idempotent.
- **A sub-machine has its own `Locked` and `Paused` flags.** Locking the parent does not lock the child, and the child keeps updating while the parent is locked.
- **The child updates before the parent evaluates its own transitions.** A parent transition that fires this update does so after the child has already run this frame.
- **Reading child state from the parent** is just a condition: `.When(() => combatSM.IsInState(CombatState.Done))`.

---

## Controlling the machine

### Changing state by hand

| Method | Respects lock? | Respects `MinDuration`? | Respects cooldown? |
|---|---|---|---|
| `ChangeState(target)` | No | No | No |
| `TryChangeState(target)` | Yes | Yes | Yes |
| `Restart()` | No | No | No |

- `ChangeState(current)` restarts the current state (Exit then Enter).
- `CanChangeState(target)` reports whether `TryChangeState` would succeed: the machine is started and unlocked, the current state's minimum duration has passed, the target is registered and off cooldown, and the target differs from the current state.
- `ChangeState` and `TryChangeState` called from an Enter or Exit callback are rejected with an error.

### Lock

`Locked = true` stops transitions: polled and event-driven transitions can't fire, `TryChangeState` returns `false`, `TriggerEvent` ignores events, and queued events are discarded. The current state **keeps updating**. `ChangeState` and `Restart` still work.

### Pause

`Paused = true` makes `Update` do nothing: no timers advance and no callbacks run. `TriggerEvent` ignores events, but events queued before the pause are kept.

### Disable and Restart

- `Disable()` shuts the machine down: runs the current state's Exit, then clears the current and previous state, timers, cooldowns, pending events, and the `Locked` and `Paused` flags. States, transitions and the initial state are kept. Don't call it from an Enter or Exit callback.
- `Restart()` returns to the initial state (Exit then Enter) and ignores `Locked`. It keeps cooldowns, `TotalElapsed` and the lock/pause flags. On a disabled machine it starts the machine again without raising `StateChanged`. It pushes an error if `SetInitialState` was never called.

### `StateChanged`

```csharp
sm.StateChanged += (previous, current) => GD.Print($"{previous} -> {current}");
```

Raised after every state change, once the new state's Enter has run. It is **not** raised for the initial state or when a disabled machine starts again, but it **is** raised when a state restarts into itself.

---

## Update order

Each `Update(delta)` call does the following. If the machine is paused or not started, it does nothing.

1. Re-sorts transitions if the transition count changed.
2. Advances `TotalElapsed` and `StateElapsed`.
3. Runs the current state's update callbacks.
4. Updates the state's sub-machines.
5. If the machine is `Locked`, stops here.
6. Processes pending events, one at a time: offered to sub-machines first; otherwise the machine's event-driven transitions are evaluated. If one fires, the update ends.
7. Evaluates polled transitions.

If any callback changes the state, disables the machine or locks it, the rest of the update is skipped.

Event-driven transitions are evaluated in step 6 and polled ones in step 7, so **priority orders transitions within each step, not across them**. An event transition that fires in step 6 ends the update before any polled transition is checked, regardless of priorities.

---

## Behavior notes

- **Global transitions respect the current state's `MinDuration` and the target's cooldown.** A death transition added as `AddGlobalTransition(Dead).When(() => hp <= 0)` is delayed while the machine sits in a state with a minimum duration (for example `Dash` with `SetMinDuration(0.2)`). Add `.OverrideMinDuration(0)` to make it fire immediately.
- **The `Update` early-outs** use a state-change counter, so a callback that changes state ends the update cleanly instead of evaluating transitions against a stale state.
- **`GetCurrentId()` returns `default(T)` before the machine starts**, which is indistinguishable from your first enum member. Use `CurrentState is not null` or `IsInState` when that matters.
- **Don't change state from a transition's `Callback`.** The callback runs before the machine performs the change, so a `ChangeState` inside it is followed by the transition's own change.
- **Don't call `Disable()` from an Enter or Exit callback.** Use a flag or event and disable from outside the state change.
- **Configure before the first update.** Adding states or transitions later is allowed, but priorities are only re-sorted when the transition count changes.

---

## Full example

A player with movement, a jump, and a combat state that hosts its own sub-machine.

```csharp
using Godot;
using TakobiAI.HSM;

public enum PlayerState { Idle, Run, Jump, Combat, Dead }
public enum CombatState { Windup, Swing, Recover, Done }

public partial class Player : CharacterBody2D
{
    private readonly TakobiHSM<PlayerState> sm = new();
    private readonly TakobiHSM<CombatState> combat = new();

    private Vector2 moveInput;
    private int hp = 100;

    public override void _Ready()
    {
        // --- combat sub-machine (configure fully before the parent starts) ---
        combat.AddState(CombatState.Windup);   // neutral initial state: its Enter runs at setup
        combat.AddState(CombatState.Swing).OnEnter(EnableHitbox).OnExit(DisableHitbox);
        combat.AddState(CombatState.Recover);
        combat.AddState(CombatState.Done);

        combat.AddTimeoutTransition(CombatState.Windup, CombatState.Swing, 0.15);
        combat.AddTimeoutTransition(CombatState.Swing, CombatState.Recover, 0.20);
        combat.AddTimeoutTransition(CombatState.Recover, CombatState.Done, 0.20);
        combat.SetInitialState(CombatState.Windup);

        // --- player machine ---
        sm.AddState(PlayerState.Idle);
        sm.AddState(PlayerState.Run).OnUpdate(Move);
        sm.AddState(PlayerState.Jump).SetMinDuration(0.1).OnEnter(StartJump);
        sm.AddState(PlayerState.Combat).WithSubMachine(combat);   // restarts on every entry
        sm.AddState(PlayerState.Dead).OnEnter(Die);

        sm.AddTransition(PlayerState.Idle, PlayerState.Run).When(() => moveInput != Vector2.Zero);
        sm.AddTransition(PlayerState.Run, PlayerState.Idle).When(() => moveInput == Vector2.Zero);

        sm.AddGlobalTransition(PlayerState.Jump)
          .OnEvent("jump")
          .OnlyFrom(PlayerState.Idle, PlayerState.Run)
          .IfOnly(IsOnFloor);
        sm.AddTransition(PlayerState.Jump, PlayerState.Idle).When(IsOnFloor);

        sm.AddGlobalTransition(PlayerState.Combat)
          .OnEvent("attack")
          .OnlyFrom(PlayerState.Idle, PlayerState.Run);
        sm.AddTransition(PlayerState.Combat, PlayerState.Idle)
          .When(() => combat.IsInState(CombatState.Done));

        sm.AddGlobalTransition(PlayerState.Dead)
          .When(() => hp <= 0)
          .OverrideMinDuration(0)   // die immediately, even mid-jump
          .SetPriority(10);

        sm.StateChanged += (from, to) => GD.Print($"{from} -> {to}");
        sm.SetInitialState(PlayerState.Idle);
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e.IsActionPressed("jump")) sm.TriggerEvent("jump");
        if (e.IsActionPressed("attack")) sm.TriggerEvent("attack");
    }

    public override void _PhysicsProcess(double delta)
    {
        moveInput = Input.GetVector("left", "right", "up", "down");
        sm.Update(delta);   // also updates `combat` while in the Combat state
    }

    private void Move(double dt) { /* ... */ }
    private void StartJump() { /* ... */ }
    private void EnableHitbox() { /* ... */ }
    private void DisableHitbox() { /* ... */ }
    private void Die() { /* ... */ }
}
```

If the player dies mid-swing, the sub-machine is disabled as `Combat` is exited, so `Swing`'s Exit runs and the hitbox is switched off.

---

## API reference

### `TakobiHSM<T>`

| Member | Description |
|---|---|
| `CurrentState` | The active `TakobiState<T>`, or `null` before `SetInitialState` or after `Disable`. |
| `PreviousState` | The state active before the current one, or `null` if there has been no change. |
| `StateElapsed` | Seconds in the current state. Reset on every state change. |
| `TotalElapsed` | Seconds this machine has been updated. Reset by `Disable`. Used for cooldowns. |
| `Locked` | Stops transitions and events; the current state keeps updating. |
| `Paused` | `Update` does nothing. |
| `StateChanged` | Event `(T previous, T current)`, raised after each state change. |
| `AddState(T)` | Registers a state; returns it for configuration. |
| `AddTransition(T from, T to)` | Adds a local transition. Throws `ArgumentException` for an unregistered state. |
| `AddGlobalTransition(T to)` | Adds a transition that can fire from any state. Throws for an unregistered state. |
| `AddTimeoutTransition(T from, T to, double seconds)` | Polled transition that fires after `seconds` in `from`. |
| `SetInitialState(T)` | Enters the initial state and starts the machine. One call only; later calls warn. |
| `Update(double delta)` | Advances the machine. |
| `TriggerEvent(StringName)` | Queues an event for the next processing step. |
| `ChangeState(T)` | Forces a state change; ignores lock, minimum duration and cooldown. |
| `TryChangeState(T)` | Changes state only if `CanChangeState` allows it. Returns whether it changed. |
| `CanChangeState(T)` | Whether `TryChangeState` would succeed now. |
| `Restart()` | Returns to the initial state. |
| `Disable()` | Shuts the machine down and clears timers, cooldowns, events and flags. |
| `GetCurrentId()` / `GetPreviousId()` | Ids of the current/previous state, or `default`. |
| `TryGetState(T, out state)` / `GetStateOrNull(T)` | Looks up a registered state. |
| `IsInState(T)` | Whether the id is the current state. |
| `HasState(T)` / `HasInitialState()` | Registration checks. |

### `TakobiState<T>`

| Member | Description |
|---|---|
| `Id` | The state's enum id. |
| `OnEnter(Action)` / `OnUpdate(Action<double>)` / `OnExit(Action)` | Add a callback (multicast). |
| `SetMinDuration(double)` / `MinDuration` | Time the state must be active before it can be left. |
| `SetCooldown(double)` / `Cooldown` | Time after leaving before it can be entered again. |
| `WithSubMachine<TChild>(machine, restartOnEnter = true)` | Hosts a sub-machine. |
| `IsOnCooldown(double totalElapsed)` | Whether the cooldown has not yet elapsed. |
| `CapturedElapsed` | The machine's `TotalElapsed` when this state was last exited (`-∞` if never). |

### `TakobiTransition<T>`

| Member | Description |
|---|---|
| `When`, `IfOnly`, `OnEvent`, `Callback` | Condition, guard, event, and fire callback. |
| `OverrideMinDuration`, `SetPriority` | Per-transition minimum duration and priority. |
| `Exclude`, `OnlyFrom` | Global-only filters. |
| `IsGlobal`, `From`, `To`, `Priority`, `EventName`, `HasEvent`, `MinDurationOverride`, `InsertionIndex` | Read-only properties describing the transition. |
