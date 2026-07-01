# BlackboardCompare

Compares a blackboard value against a constant or another blackboard value.

## What it does

Checks the value stored at `Key` against `Value` using the chosen comparison `Mode`, succeeding if the comparison holds. `Value` can itself be a constant or a `"$otherKey"` blackboard reference (see [`BBValueResolver`](../core/BBValueResolver.md)), so you can compare two blackboard values directly.

## How it works

- Fails immediately if `Key` doesn't exist on the blackboard.
- Supports `Equal`, `NotEqual`, `Less`, `Greater`, `LessOrEqual`, and `GreaterOrEqual`, though which of these apply depends on the type of the existing value:
  - **Bool:** only `Equal` / `NotEqual`.
  - **Int / Float:** all six modes.
  - **String, StringName, NodePath, Vector2/3, Vector2I/3I:** only `Equal` / `NotEqual`.
  - Unsupported types or mode combinations push a warning and return `Failure`.

## Properties

| Property | Description |
|---|---|
| `Key` | *(inherited)* the blackboard key to read. |
| `Mode` | The comparison to perform. |
| `Value` | Constant or `"$key"` blackboard reference to compare against. |

## Example use case

"Is `$ammo` greater than `0`?" or "Does `$state` equal `$targetState`?"
