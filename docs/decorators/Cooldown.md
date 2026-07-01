# Cooldown

Blocks its child from running again for a set duration after it finishes.

## What it does

The first time the child runs and finishes, `Cooldown` starts a timer. While that timer is active, `Cooldown` immediately returns `Failure` without ticking the child at all — once it expires, the child is allowed to run again.

## How it works

- `Trigger` controls what starts the cooldown: `Success`, `Failure`, or `Both`.
- While on cooldown, `OnTick` returns `Failure` immediately.
- Once `Duration` has elapsed, the cooldown clears and the child ticks normally again.
- `Reset()` can be called externally to clear an active cooldown early.

## Properties

| Property | Description |
|---|---|
| `Duration` | How long (seconds) the child is blocked after triggering. |
| `Trigger` | Which child result(s) start the cooldown. |

## Example use case

Preventing an ability or attack from being reused more than once every few seconds.
