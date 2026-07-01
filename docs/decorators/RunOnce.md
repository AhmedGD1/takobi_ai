# RunOnce

Runs its child a single time, then always succeeds without running it again.

## What it does

The first time the child finishes, `RunOnce` remembers that it has "occurred." From then on, it skips ticking the child entirely and just returns `Success` — unless reset.

## How it works

- Before the child has finished once, `RunOnce` ticks it normally and passes its result through.
- Once the child finishes (`Success` or `Failure`), `RunOnce` marks itself as occurred.
- After that, every tick immediately returns `Success` without touching the child.
- `Reset` controls whether that memory persists:
  - `Never` — stays "occurred" forever (until the tree/node is reset externally).
  - `OnEnter` — clears back to unoccurred every time `RunOnce` starts running again.

## Properties

| Property | Description |
|---|---|
| `Reset` | Whether the "already ran" state resets each time this node starts. |

## Example use case

Playing a one-time introduction line or setup action that shouldn't repeat every time its branch is revisited.
