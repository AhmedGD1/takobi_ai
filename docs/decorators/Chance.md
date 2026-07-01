# Chance

Runs its child only some of the time, based on a probability.

## What it does

Each time `Chance` runs, it rolls the dice: with probability `Probability`, it ticks the child normally; otherwise it returns `Failure` without touching the child at all.

## Properties

| Property | Description |
|---|---|
| `Probability` | Chance (0–100%) that the child runs on a given attempt. |

## Example use case

A monster that only has a 20% chance of taunting the player each time the taunt branch is reached.
