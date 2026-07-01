# BTComposite

Abstract base class for nodes with multiple children.

## What it does

`BTComposite` provides the shared plumbing every composite (`Sequence`, `Selector`, `Parallel`, etc.) needs: collecting its `BTNode` children and aborting all of them if the composite itself is aborted.

## How it works

- On `_Ready`, it gathers every direct child that is a `BTNode` into `Children`, in scene-tree order.
- If the composite has no `BTNode` children, it shows a configuration warning in the editor.
- On `OnAbort`, it aborts every child — ensuring no child is left mid-execution if the composite is cancelled from above.

Concrete composites override `OnTick` to decide *which* children run and how their results combine.
