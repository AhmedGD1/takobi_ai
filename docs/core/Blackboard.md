# Blackboard

Shared key-value data store used to pass state between nodes in a tree.

## What it does

A `Blackboard` is a `Resource` holding a dictionary of named values. Nodes read and write it through `BTContext` to communicate — for example, a condition checking `$target_visible` that an action set earlier in the tree.

## How it works

- `SetValue(key, value)` / `GetValue(key)` — store or read a value by key, with an optional typed generic overload and fallback value.
- `TryGetValue(key, out result)` — safe lookup without a fallback.
- `Has(key)` — checks whether a key exists.
- `Erase(key)` / `Clear()` — remove a single key or wipe the blackboard.

Many exported properties across the library (e.g. on `BlackboardSet` or `BlackboardCompare`) accept either a constant value or a `"$key"` string that gets resolved against the blackboard at runtime — see [`BBValueResolver`](BBValueResolver.md).

## Notes

Being a `Resource`, a `Blackboard` can be saved as a `.tres` file and shared or duplicated across trees — useful for giving multiple agents a starting set of shared or per-agent data.
