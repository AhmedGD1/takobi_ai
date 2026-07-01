# Weight

Assigns a dynamic score to its child, used by [`WeightedSelector`](../composites/WeightedSelector.md) to pick which sibling runs.

## What it does

`Weight` doesn't change its child's behavior when ticked — it just passes the tick straight through. What it adds is a `GetWeight(ctx)` value that a parent `WeightedSelector` reads to decide priority between siblings.

## How it works

- `Source` selects where the weight comes from:
  - `Constant` — always returns `Amount`.
  - `Blackboard` — reads a `float` from `BlackboardKey`.
- `OnTick` simply ticks the child and returns its result unchanged.

## Properties

| Property | Description |
|---|---|
| `Source` | Where the weight value comes from. |
| `Amount` | Fixed weight (0–100), used when `Source` is `Constant`. |
| `BlackboardKey` | Blackboard key to read the weight from, used when `Source` is `Blackboard`. |

## Example use case

Wrapping "flee" and "attack" branches under a `WeightedSelector`, each with a `Weight` reading a computed urgency score from the blackboard, so the agent always picks whichever is currently most pressing.
