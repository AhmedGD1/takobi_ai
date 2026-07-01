# Succeeder

Forces its child's finished result to always be `Success`.

## What it does

Ticks the child as normal and passes `Running` straight through, but rewrites any finished result — `Success` or `Failure` — to `Success`.

## Example use case

Running an optional or "best effort" branch inside a `Sequence` without letting its failure stop the rest of the sequence from continuing.
