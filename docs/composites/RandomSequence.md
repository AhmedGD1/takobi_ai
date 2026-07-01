# RandomSequence

A [`Sequence`](Sequence.md) whose children are evaluated in a random order that's reshuffled each time the sequence starts.

## What it does

Same success/failure logic as `Sequence` — every child must succeed for the sequence to succeed — but the order children are visited in changes from one run to the next, so the sequence doesn't always attempt the same step first.

## How it works

Built on [`BTRandomComposite`](BTRandomComposite.md): the child order is shuffled on `OnEnter`, then ticked through exactly like a `Sequence` — `Failure` stops it immediately, `Success` advances to the next child, and the run succeeds only once every child has succeeded.

## Example use case

A sequence of idle animations or flavor actions where you don't want the same one to always play first.
