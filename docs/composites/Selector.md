# Selector

Runs its children one after another; succeeds as soon as one child succeeds.

## What it does

A `Selector` is the behavior-tree equivalent of a logical **OR**: it tries each child in order until one works. Think "try this, or if that fails, try this instead."

## How it works

- Ticks children in order, starting from the first.
- If a child returns `Running`, the selector returns `Running` and resumes at that same child next tick.
- If a child returns `Success`, the selector immediately returns `Success`.
- If a child returns `Failure`, the selector moves on to the next child.
- If every child fails, the selector returns `Failure`.

Once started, a `Selector` does **not** re-check earlier children on later ticks — it only continues from where it left off. For a version that re-evaluates from the start each tick, see [`ReactiveSelector`](ReactiveSelector.md).

## Example use case

"Attack the enemy if in range, otherwise chase, otherwise patrol" — the first option that succeeds wins.
