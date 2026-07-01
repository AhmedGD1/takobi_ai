# BBValueResolver

Helper struct that resolves an exported value as either a constant or a live blackboard reference.

## What it does

Several nodes (`BlackboardSet`, `BlackboardCompare`, `IncrementValue`, and others) expose a `Value`/`Amount` property that can be either a fixed value or a reference to another blackboard key. `BBValueResolver` is what makes that work.

## How it works

When a value is assigned:

- If it's a string or `StringName` starting with `$`, the resolver stores everything after the `$` as a blackboard key and switches to blackboard mode.
- Otherwise, it stores the value itself as a constant.

At runtime, calling `Resolve(ctx)`:

- Returns the constant directly, **or**
- Looks up the referenced key on `ctx.Blackboard` and returns its current value (pushing a warning and returning a default if the key is missing).

## Example

Setting a node's `Value` property to `"$health"` means "use whatever is currently stored under the `health` key," rather than the literal string `"$health"`. Any other value is treated as a plain constant.
