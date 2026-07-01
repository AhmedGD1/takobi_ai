# Delayer

Waits a duration before running its child.

## What it does

Returns `Running` until a delay elapses, then ticks the child as normal. The delay length can come from three different sources.

## How it works

- `Source` selects where the delay length comes from:
  - `Constant` — always waits `Duration` seconds.
  - `Random` — waits a random duration between `MinDuration` and `Duration`.
  - `Blackboard` — reads the delay (in seconds) from `BlackboardKey`.
- The delay is recalculated fresh each time the decorator starts running (`OnEnter`).
- Once the delay has elapsed, the child ticks normally on every subsequent tick — if there's no child, it returns `Success`.

## Properties

| Property | Description |
|---|---|
| `Source` | Where the delay duration comes from. |
| `Duration` | Delay length in seconds (`Constant`), or the upper bound (`Random`). |
| `MinDuration` | Lower bound for a `Random` delay. |
| `BlackboardKey` | Blackboard key to read the delay from (`Blackboard` source). |

## Example use case

Adding a pause before an NPC reacts to a noise, to avoid instant, robotic responses.
