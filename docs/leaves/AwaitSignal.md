# AwaitSignal

Waits until a specified Godot signal fires on a given node.

## What it does

Runs (`Running`) until `Emitter` emits `Signal`, at which point it succeeds. Useful for syncing the tree with animations, timers, or other scene-level events.

## How it works

- On entering, connects to `Signal` on `Emitter` as a one-shot connection.
- Returns `Running` until the signal is received, then `Success` on the next tick.
- Supports signals with zero up to eight arguments — the payload itself isn't used, only the fact that the signal fired.
- If aborted before the signal fires, the connection is cleanly disconnected.

## Properties

| Property | Description |
|---|---|
| `Emitter` | The node whose signal is being awaited. |
| `Signal` | The name of the signal to wait for. |

## Example use case

Waiting for an `animation_finished` signal before moving on to the next step in a sequence.
