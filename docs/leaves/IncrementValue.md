# IncrementValue

Adds an amount to an existing numeric blackboard value.

## What it does

Reads the current value at `Key`, adds `Amount` to it (which can itself be a constant or a `"$key"` blackboard reference), and writes the result back.

## How it works

- Fails with a warning if `Key` is empty or doesn't already exist on the blackboard.
- Only works on `Int` or `Float` values at `Key` — any other type fails with a warning.
- The type of the existing value is preserved (adding to an int keeps it an int, adding to a float keeps it a float).

## Properties

| Property | Description |
|---|---|
| `Key` | Blackboard key holding the numeric value to update. |
| `Amount` | Constant or `"$key"` blackboard reference for how much to add. |

## Example use case

Incrementing a `$score` or `$ammo_used` counter, or decrementing `$health` by passing a negative amount.
