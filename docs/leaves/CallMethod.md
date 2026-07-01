# CallMethod

Calls a method on a node, with optional arguments and return-value handling.

## What it does

Invokes `Method` on `Source`, resolving each argument in `Args` first (constants or `"$key"` blackboard references), and always succeeds unless configured to fail on a falsy return value.

## How it works

- Arguments are resolved each tick via [`BBValueResolver`](../core/BBValueResolver.md)-style logic — any `"$key"` string is replaced with the current blackboard value for that key.
- If `StoreResultKey` is set, the method's return value is written to that blackboard key.
- If `FailOnFalseReturn` is enabled, a `null`/`nil` result or a literal `false` boolean result causes the node to return `Failure` instead of `Success`.
- The editor validates that `Source` has a method named `Method` and warns if the argument count/types don't match its signature.

## Properties

| Property | Description |
|---|---|
| `Source` | The node the method is called on. |
| `Method` | Name of the method to call. |
| `Args` | Arguments passed to the method — constants or `"$key"` blackboard references. |
| `StoreResultKey` | Optional blackboard key to store the return value in. |
| `FailOnFalseReturn` | If true, a `null`/`false` return value results in `Failure`. |

## Example use case

Calling a custom `TakeDamage(amount)` method on the agent, or `IsPathClear()` and storing/checking its result.
