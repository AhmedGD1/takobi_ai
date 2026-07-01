# BTDecorator

Abstract base class for nodes that wrap a single child.

## What it does

`BTDecorator` provides the shared plumbing every decorator needs: finding its one `BTNode` child and aborting it when the decorator itself is aborted. Concrete decorators (`Inverter`, `Retry`, `Timeout`, etc.) build on this to modify their child's timing, repetition, or result.

## How it works

- On `_Ready`, it grabs the first `BTNode` child found in the scene tree as `Child`.
- Shows a configuration warning if there's no child, or more than one (only the first is ever used).
- On `OnAbort`, it aborts `Child` if one is set.

Concrete decorators override `OnTick` (and sometimes `OnEnter`) to decide how and when to tick `Child`, and how to interpret its result.
