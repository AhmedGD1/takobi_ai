# BTNode

Abstract base class for every node in a TakobiAI behavior tree.

## What it does

`BTNode` defines the shared lifecycle that composites, decorators, conditions, and leaves all build on. It's never used directly in a tree — every node in this reference derives from it.

## How it works

Each tree tick, a node's `Tick(ctx)` method is called:

1. If the node wasn't already running, `OnEnter` fires first.
2. `OnTick` runs and returns a `Status` (`Success`, `Failure`, or `Running`).
3. If the result isn't `Running`, `OnExit` fires and the node is marked as no longer running.

If a running node needs to be cancelled early (for example, a `Sequence` moving on after a sibling fails), `Abort` is called instead. This fires `OnAbort` followed by `OnExit(Status.Failure)`, giving the node a chance to clean up (cancel timers, disconnect signals, etc.).

`BTNode` also tracks a bit of state for you automatically:

- `IsRunning` — whether the node is currently mid-execution.
- `LastStatus` — the result of the most recent tick.
- `TickCount` — how many times the node has ticked.
- `GetWeight(ctx)` — used by weight-based composites like `WeightedSelector`; returns `0` unless overridden.

## Writing a custom node

Override `OnTick` at minimum. Override `OnEnter`, `OnExit`, and `OnAbort` only if your node needs setup or cleanup logic.
