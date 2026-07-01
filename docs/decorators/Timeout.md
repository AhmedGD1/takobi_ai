# Timeout

Aborts and fails its child if it runs longer than a set duration.

## What it does

Starts a timer when it begins running. If the child is still `Running` once `Duration` has elapsed, `Timeout` aborts it and returns `Failure` — otherwise the child's result passes through normally.

## Properties

| Property | Description |
|---|---|
| `Duration` | Maximum time (seconds) the child is allowed to run. |

## Example use case

Giving a "chase the player" action a maximum duration, so the agent gives up and moves on instead of chasing forever.
