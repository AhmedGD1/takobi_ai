# BTCondition

Base class for nodes that check something and return a result immediately.

## What it does

`BTCondition` is a leaf that never runs across multiple ticks — it evaluates a condition on the spot and returns `Success` or `Failure` right away. It's the base for all blackboard-checking conditions in this library.

## How it works

`OnTick` is sealed and simply calls `Check(ctx)`: returning `Success` if it returns `true`, `Failure` otherwise. By default `Check` returns `true`, so a bare `BTCondition` always succeeds — subclasses override `Check` to implement real logic.

## Writing a custom condition

Inherit from `BTCondition` and override `Check(ctx)` with a `bool`-returning test. No need to worry about `Running` — conditions always resolve immediately.
