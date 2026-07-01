# BTContext

The per-tick context object passed through every node in the tree.

## What it does

`BTContext` bundles the two things a node typically needs to do its job:

- `Agent` — the `Node` the tree is controlling (usually the character or actor).
- `Blackboard` — the shared data store for reading and writing state.

It also tracks `SubTreeDepth`, which increments as the tree descends into nested [`SubTree`](../leaves/SubTree.md) leaves and is used to guard against circular subtree references.

## How it works

Every node's `Tick(ctx)`, `OnEnter(ctx)`, `OnTick(ctx)`, and `OnAbort(ctx)` methods receive the same `BTContext` instance for a given tick, so any node can access the current agent and blackboard without needing its own reference to them.
