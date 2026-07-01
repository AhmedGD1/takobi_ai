# Retry

Retries its child after a failure, up to a maximum number of attempts.

## What it does

If the child fails, `Retry` doesn't give up right away — it counts the attempt and tries again on the next tick, up to `MaxAttempts` times before finally reporting `Failure`.

## How it works

- `Success` or `Running` from the child pass straight through.
- On `Failure`, the attempt counter increases.
  - If `MaxAttempts` has been reached (and is greater than `0`), `Retry` returns `Failure`.
  - Otherwise, it returns `Running`, so the tree will try the child again on the next tick.
- Setting `MaxAttempts` to `0` allows unlimited retries.

## Properties

| Property | Description |
|---|---|
| `MaxAttempts` | How many failures are tolerated before giving up. `0` = unlimited. |

## Example use case

Attempting to pick a lock or find a path multiple times before giving up and trying something else.
