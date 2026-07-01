# TakobiAI — Node Reference

TakobiAI is a Behavior Tree framework for Godot (C#). This folder documents every `BTNode` in the library: what it does, how it behaves, and what its properties control.

Every node returns one of three states each tick — `Success`, `Failure`, or `Running` — defined in [`Status`](core/Status.md). Trees are driven by a [`BehaviorTree`](core/BehaviorTree.md) root, which ticks its single child node and shares a [`Blackboard`](core/Blackboard.md) through a [`BTContext`](core/BTContext.md).

## Core

| Node | Description |
|---|---|
| [BTNode](core/BTNode.md) | Abstract base class for every node in a tree. |
| [BehaviorTree](core/BehaviorTree.md) | Root node that owns and ticks the tree. |
| [Blackboard](core/Blackboard.md) | Shared key-value data store. |
| [BTContext](core/BTContext.md) | Per-tick context passed through the tree (agent, blackboard). |
| [BTService](core/BTService.md) | Background task ticked on its own interval, independent of the tree. |
| [BBValueResolver](core/BBValueResolver.md) | Resolves an exported value as either a constant or a blackboard reference. |
| [Status](core/Status.md) | The three possible outcomes of a tick: `Success`, `Failure`, `Running`. |

## Composites

Nodes with multiple children that decide which of them to run and how to combine their results.

| Node | Description |
|---|---|
| [BTComposite](composites/BTComposite.md) | Abstract base class for all composites. |
| [BTRandomComposite](composites/BTRandomComposite.md) | Base class for composites that shuffle their children. |
| [Sequence](composites/Sequence.md) | Runs children in order; fails on the first failure. |
| [Selector](composites/Selector.md) | Runs children in order; succeeds on the first success. |
| [ReactiveSequence](composites/ReactiveSequence.md) | Like `Sequence`, but re-checks earlier children every tick. |
| [ReactiveSelector](composites/ReactiveSelector.md) | Like `Selector`, but re-checks earlier children every tick. |
| [RandomSequence](composites/RandomSequence.md) | `Sequence` with a shuffled child order. |
| [RandomSelector](composites/RandomSelector.md) | `Selector` with a shuffled child order. |
| [Parallel](composites/Parallel.md) | Runs all children at once with configurable success/failure policies. |
| [WeightedSelector](composites/WeightedSelector.md) | Runs whichever child currently reports the highest weight. |

## Conditions

Nodes that check something and immediately return `Success` or `Failure` — never `Running`.

| Node | Description |
|---|---|
| [BTCondition](conditions/BTCondition.md) | Base class for all condition checks. |
| [BlackboardCondition](conditions/BlackboardCondition.md) | Base class for conditions that read a blackboard key. |
| [BlackboardCompare](conditions/BlackboardCompare.md) | Compares a blackboard value against another value. |
| [BlackboardHas](conditions/BlackboardHas.md) | Checks whether a blackboard key exists. |

## Decorators

Nodes with a single child that modify its result, timing, or repetition.

| Node | Description |
|---|---|
| [BTDecorator](decorators/BTDecorator.md) | Base class for all decorators. |
| [Chance](decorators/Chance.md) | Runs the child with a given probability. |
| [Cooldown](decorators/Cooldown.md) | Blocks the child from re-triggering for a set duration. |
| [Delayer](decorators/Delayer.md) | Waits before running the child. |
| [Failer](decorators/Failer.md) | Forces the child's result to `Failure`. |
| [Inverter](decorators/Inverter.md) | Flips `Success` and `Failure`. |
| [Repeater](decorators/Repeater.md) | Repeats the child a set number of times. |
| [Retry](decorators/Retry.md) | Retries the child on failure, up to a limit. |
| [RunOnce](decorators/RunOnce.md) | Runs the child once, then always succeeds. |
| [Succeeder](decorators/Succeeder.md) | Forces the child's result to `Success`. |
| [Timeout](decorators/Timeout.md) | Fails and aborts the child if it runs too long. |
| [UntilFail](decorators/UntilFail.md) | Keeps re-running the child until it fails. |
| [UntilSuccess](decorators/UntilSuccess.md) | Keeps re-running the child until it succeeds. |
| [Weight](decorators/Weight.md) | Assigns a dynamic weight to its child for `WeightedSelector`. |

## Leaves

Nodes at the bottom of the tree that do actual work.

| Node | Description |
|---|---|
| [BTAction](leaves/BTAction.md) | Base class for all action leaves. |
| [AwaitSignal](leaves/AwaitSignal.md) | Waits for a Godot signal to fire. |
| [BlackboardErase](leaves/BlackboardErase.md) | Removes a key from the blackboard. |
| [BlackboardSet](leaves/BlackboardSet.md) | Writes a value to the blackboard. |
| [CallMethod](leaves/CallMethod.md) | Calls a method on a node. |
| [IncrementValue](leaves/IncrementValue.md) | Adds to a numeric blackboard value. |
| [Log](leaves/Log.md) | Prints a message, with blackboard value interpolation. |
| [SignalEmitter](leaves/SignalEmitter.md) | Emits a Godot signal from a node. |
| [SubTree](leaves/SubTree.md) | Runs another `BehaviorTree` as a nested subtree. |
