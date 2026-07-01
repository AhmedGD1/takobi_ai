# SubTree

Runs another `BehaviorTree` as a nested subtree inside this one.

## What it does

Delegates ticking to a separate `Tree`, letting you compose large behavior trees out of smaller, reusable pieces instead of one giant tree.

## How it works

- `Tree` is the `BehaviorTree` to run as a child. When assigned, `SubTree` takes ownership of it via `SetSubTree`, switching it out of its normal self-ticking mode so it's driven by this leaf instead.
- `ShareBlackboard` controls data isolation:
  - `true` (default) — the subtree uses the *same* blackboard as the parent tree, so they can read/write the same keys.
  - `false` — the subtree keeps its own separate blackboard.
- Each tick, `SubTree` ticks the nested tree and returns whatever status it produces.
- `SubTreeDepth` (tracked on `BTContext`) increases with each nested level. If it exceeds a maximum depth (16), the node fails with an error — this guards against accidentally creating a circular reference (a tree that contains itself, directly or indirectly).
- Aborting `SubTree` aborts the nested tree as well.

## Properties

| Property | Description |
|---|---|
| `Tree` | The `BehaviorTree` to run as a nested subtree. |
| `ShareBlackboard` | Whether the subtree shares the parent's blackboard or uses its own. |

## Example use case

Extracting a reusable "combat" or "flee" behavior tree that multiple enemy types can plug in via `SubTree`, rather than duplicating the same branch across every tree.
