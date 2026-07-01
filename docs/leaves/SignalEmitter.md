# SignalEmitter

Emits a Godot signal from a node, with optional resolved arguments.

## What it does

Fires `Signal` on `Emitter`, passing along `Args` (each resolved from a constant or `"$key"` blackboard reference), and always succeeds.

## How it works

- Arguments are resolved each tick via [`BBValueResolver`](../core/BBValueResolver.md)-style logic, the same mechanism used by `CallMethod`.
- The editor validates that `Emitter` actually has a signal named `Signal` and warns if the argument count/types don't match its declared parameters.

## Properties

| Property | Description |
|---|---|
| `Emitter` | The node the signal is emitted from. |
| `Signal` | Name of the signal to emit. |
| `Args` | Arguments to pass — constants or `"$key"` blackboard references. |

## Example use case

Triggering a `hit_landed` or `state_changed` signal that other parts of the scene are listening for, optionally carrying blackboard data like damage amount.
