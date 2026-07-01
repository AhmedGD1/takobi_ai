# Parallel

Runs all of its children at the same time, every tick, instead of one at a time.

## What it does

Unlike `Sequence` or `Selector`, `Parallel` ticks every child on every update and decides its own result based on how many children have succeeded or failed, according to two configurable policies.

## How it works

- `SuccessPolicy` — `One` (succeed as soon as any single child succeeds) or `All` (succeed only once every child has succeeded).
- `FailurePolicy` — `One` (fail as soon as any single child fails) or `All` (fail only once every child has failed).
- Each tick, children that have already finished (`Success`/`Failure`) are skipped; only children still `Running` are ticked again.
- As soon as a policy's condition is met, any children still `Running` are aborted, and `Parallel` returns the corresponding result.
- If neither policy's condition is met yet, `Parallel` returns `Running`.

## Properties

| Property | Description |
|---|---|
| `SuccessPolicy` | `One` or `All` — how many children must succeed for `Parallel` to succeed. |
| `FailurePolicy` | `One` or `All` — how many children must fail for `Parallel` to fail. |

## Example use case

Playing a movement action and a facing/aim action simultaneously, succeeding once both are done (`SuccessPolicy = All`).
