# Sequence

Runs its children one after another; the whole sequence fails as soon as one child fails.

## What it does

A `Sequence` is the behavior-tree equivalent of a logical **AND**: all children must succeed for the sequence to succeed. Think "do this, then this, then this."

## How it works

- Ticks children in order, starting from the first.
- If a child returns `Running`, the sequence returns `Running` and resumes at that same child next tick.
- If a child returns `Failure`, the sequence immediately returns `Failure`.
- If a child returns `Success`, the sequence moves on to the next child.
- If every child succeeds, the sequence returns `Success`.

Once started, a `Sequence` does **not** re-check earlier children on later ticks — it only continues from where it left off. For a version that re-evaluates from the start each tick, see [`ReactiveSequence`](ReactiveSequence.md).

## Example use case

"Walk to the door, then open it, then walk through" — each step must succeed before moving to the next.
