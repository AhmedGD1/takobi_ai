# BTRandomComposite

Abstract base class for composites that visit their children in a shuffled order.

## What it does

Extends [`BTComposite`](BTComposite.md) with a `ShuffledChildren` array that's reshuffled every time the composite starts running. `RandomSequence` and `RandomSelector` both build on this.

## How it works

- On `_Ready`, it copies `Children` into `ShuffledChildren`.
- On `OnEnter` (i.e. each time the composite starts a fresh run), it shuffles `ShuffledChildren` in place using a Fisher–Yates shuffle.

Subclasses tick through `ShuffledChildren` instead of `Children`, so the order of evaluation changes each run while the underlying child set stays the same.
