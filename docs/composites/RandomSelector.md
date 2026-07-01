# RandomSelector

A [`Selector`](Selector.md) whose children are evaluated in a random order that's reshuffled each time the selector starts.

## What it does

Same success/failure logic as `Selector` — the first child that succeeds wins — but the order children are tried in changes from one run to the next, so the same option doesn't always win by default.

## How it works

Built on [`BTRandomComposite`](BTRandomComposite.md): the child order is shuffled on `OnEnter`, then ticked through exactly like a `Selector` — `Success` stops it immediately, `Failure` moves on to the next child, and the run fails only if every child fails.

## Example use case

Picking between several equally-valid reactions (taunts, wander directions, idle behaviors) without always favoring the first one defined.
