# BTAction

Base class for leaf nodes that do actual work.

## What it does

`BTAction` is the base every leaf in this library (`Log`, `CallMethod`, `BlackboardSet`, etc.) builds on. Used directly and unmodified, it's an empty placeholder that always succeeds — useful as a stub while building out a tree.

## Writing a custom action

Inherit from `BTAction` and override `OnTick(ctx)` to implement your own gameplay logic, returning `Success`, `Failure`, or `Running` as appropriate. Override `OnEnter`/`OnExit`/`OnAbort` if the action needs setup or cleanup (timers, signal connections, etc.).
