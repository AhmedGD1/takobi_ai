# ReactiveSequence

A `Sequence` that re-checks every child from the start on every tick, instead of resuming from where it left off.

## What it does

Behaves like [`Sequence`](Sequence.md) — all children must succeed for the sequence to succeed — but stays reactive to earlier conditions changing while a later child is still running.

## How it works

Every tick, it evaluates children from the first one again:

- The first child to return `Failure` stops the sequence and returns `Failure`.
- The first child to return `Running` becomes the new "running" child — if a *different* child was running before, that old one is aborted — and the sequence returns `Running`.
- If every child succeeds, the sequence returns `Success`.

## Example use case

"While the enemy is still visible, keep approaching and attacking." If a condition checked early in the sequence (e.g. "enemy visible") starts failing while a later action (e.g. "attack") is running, the reactive check catches it immediately and aborts the running action — a plain `Sequence` would not notice until the running child finished on its own.
