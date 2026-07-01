# ReactiveSelector

A `Selector` that re-checks every child from the start on every tick, instead of resuming from where it left off.

## What it does

Behaves like [`Selector`](Selector.md) — the first child to succeed wins — but stays reactive to a higher-priority option becoming available while a lower-priority child is still running.

## How it works

Every tick, it evaluates children from the first one again:

- The first child to return `Success` stops the selector and returns `Success`.
- The first child to return `Running` becomes the new "running" child — if a *different* child was running before, that old one is aborted — and the selector returns `Running`.
- If every child fails, the selector returns `Failure`.

## Example use case

"Attack if the enemy is in range, otherwise keep chasing." If the enemy comes into range while the chase action is still running, the reactive check switches to the attack immediately, rather than waiting for the chase to finish on its own.
