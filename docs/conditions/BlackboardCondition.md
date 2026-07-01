# BlackboardCondition

Abstract base class for conditions that read a specific blackboard key.

## What it does

Adds a single exported `Key` property to [`BTCondition`](BTCondition.md), giving every blackboard-related condition (`BlackboardHas`, `BlackboardCompare`) a consistent way to point at the value they're checking.

## Properties

| Property | Description |
|---|---|
| `Key` | The blackboard key this condition inspects. |
