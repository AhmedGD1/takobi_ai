# BlackboardHas

Checks whether a blackboard key exists.

## What it does

Succeeds if `Key` is present on the blackboard, fails otherwise. Doesn't inspect the value itself — just its presence.

## Properties

| Property | Description |
|---|---|
| `Key` | *(inherited)* the blackboard key to check for. |

## Example use case

"Does the agent have a `$target` assigned yet?" before running behavior that depends on one.
