# BTService

Abstract base class for background tasks that run alongside a tree, independent of its tick.

## What it does

A `BTService` is a `Resource` attached to a `BehaviorTree`'s `Services` list. Unlike tree nodes, it isn't part of the tick chain and doesn't return a `Status` — it simply runs its own logic on a fixed interval, every frame, regardless of what the tree is currently doing.

This is useful for things like perception checks, timers, or blackboard bookkeeping that should keep running even while the tree is busy executing a long `Running` branch elsewhere.

## How it works

- `Interval` (seconds) controls how often `OnTick` fires.
- The `BehaviorTree` calls `Tick(delta, ctx)` every frame; internally it accumulates `delta` and fires `OnTick(ctx)` as many times as the elapsed time allows, keeping the service's rate independent of frame rate.
- `Reset()` clears the accumulated timer.

## Writing a custom service

Override `OnTick(ctx)` with whatever periodic logic you need — it receives the same `BTContext` (agent + blackboard) as the tree's nodes.
