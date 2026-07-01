# Repeater

Repeats its child a set number of times before succeeding.

## What it does

Re-runs the child over and over, counting completions, until it reaches `Times` — at which point `Repeater` succeeds. Setting `Times` to `0` repeats indefinitely.

## How it works

- While the child is `Running`, `Repeater` returns `Running`.
- When the child finishes, the completion counter increases.
- If the child fails and `FailOnChildFailure` is enabled, `Repeater` stops immediately and returns `Failure`.
- Once the counter reaches `Times` (and `Times > 0`), `Repeater` returns `Success`.
- Otherwise, it keeps going (`Running`), restarting the child on the next tick.

## Properties

| Property | Description |
|---|---|
| `Times` | Number of completions required to succeed. `0` = infinite. |
| `FailOnChildFailure` | If true, a single child failure stops the repeater with `Failure` instead of continuing. |

## Example use case

Making an NPC patrol between waypoints a fixed number of laps, or loop forever if `Times` is `0`.
