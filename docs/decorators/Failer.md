# Failer

Forces its child's finished result to always be `Failure`.

## What it does

Ticks the child as normal and passes `Running` straight through, but rewrites any finished result — `Success` or `Failure` — to `Failure`.

## Example use case

Running an optional side-action inside a `Sequence` without letting its success stop the sequence from continuing down a `Selector` branch, or intentionally forcing a fallback path to be taken.
