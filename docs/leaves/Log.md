# Log

Prints a message to the console, with blackboard values interpolated into the text.

## What it does

Prints `Message` using rich text formatting, always succeeding immediately. Any `$key` pattern inside the message is replaced with that key's current blackboard value before printing.

## How it works

- Runs on `OnEnter`, so it fires once as soon as the node starts, not on every tick.
- Any substring matching `$key` (letters, numbers, underscores, or dots) is looked up on the blackboard.
- If the key doesn't exist, it's replaced with `<missing:key>` instead of failing silently.

## Properties

| Property | Description |
|---|---|
| `Message` | The text to print. Supports `$key` interpolation and multiline input. |

## Example use case

Debug logging like `"Attacking $target with $weapon"` while iterating on a tree.
