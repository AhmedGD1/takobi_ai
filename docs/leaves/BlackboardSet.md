# BlackboardSet

Writes a value to the blackboard.

## What it does

Sets `Key` on the blackboard to `Value` and always succeeds — unless `Key` is unassigned, in which case it fails with a warning.

## How it works

`Value` can be a plain constant or a `"$otherKey"` blackboard reference (see [`BBValueResolver`](../core/BBValueResolver.md)), so this node can also be used to copy one blackboard value into another key.

## Properties

| Property | Description |
|---|---|
| `Key` | The blackboard key to write to. |
| `Value` | Constant value, or `"$key"` to copy another blackboard value. |

## Example use case

Recording a spotted enemy as `$target`, or flipping a `$is_alert` flag to `true`.
