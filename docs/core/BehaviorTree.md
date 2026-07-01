# BehaviorTree

The root node that owns and drives a behavior tree.

## What it does

`BehaviorTree` is the entry point for a tree. It holds the tree's `Blackboard`, ticks its single `BTNode` child on a schedule, and manages any background `BTService`s attached to it.

## How it works

- **Ticking:** runs on `_Process` or `_PhysicsProcess`, controlled by `Mode` (`Idle` or `Physics`), at a fixed rate set by `TicksPerSecond` — independent of the engine's frame rate.
- **Agent & Blackboard:** `Agent` is the node your leaves act on (e.g. the character); `Blackboard` is the shared data store passed to every node via `BTContext`. If no blackboard is assigned, one is created automatically with a warning.
- **Services:** any `BTService` resources in `Services` are ticked every frame at their own interval, separate from the tree's tick rate — useful for perception or bookkeeping that shouldn't block the tree.
- **Active / inactive:** toggling `Active` starts or stops ticking without removing the tree from the scene.
- **Subtrees:** a tree can be driven externally as a nested subtree (via the [`SubTree`](../leaves/SubTree.md) leaf) instead of ticking itself — `SetSubTree` switches it into that mode.
- **Abort:** `Abort()` cancels the currently running branch, propagating down through the tree.

## Properties

| Property | Description |
|---|---|
| `Agent` | The node this tree controls. |
| `Mode` | `Idle` or `Physics` — which process callback drives ticking. |
| `Active` | Whether the tree is currently ticking. |
| `TicksPerSecond` | Tick rate, decoupled from frame rate. |
| `Blackboard` | The shared data store for this tree. |
| `Services` | Background tasks ticked alongside the tree. |
| `CustomMonitor` | Enables a Godot performance monitor tracking tick time (debug builds). |
