# WeightedSelector

Runs whichever child currently reports the highest weight, re-evaluated every tick.

## What it does

Instead of trying children in a fixed or random order, `WeightedSelector` asks each child for a numeric weight and runs the single highest-scoring one. This is useful for utility-style AI, where several behaviors compete and the "best" one right now should win.

## How it works

- Each tick, it calls `GetWeight(ctx)` on every child and picks the one with the highest score.
- If the previously-running child is no longer the highest-scoring one, that child is aborted and the new one starts.
- The chosen child is ticked, and its result becomes the selector's result.
- If no children are present, it returns `Failure`.

Children are expected to report their weight via the [`Weight`](../decorators/Weight.md) decorator, which wraps a child and gives it a constant or blackboard-driven score. The editor shows a warning if a child isn't wrapped in `Weight`.

## Example use case

Choosing between "flee," "attack," and "heal" based on which currently has the highest computed priority (e.g. driven by health and threat level on the blackboard).
