# Inverter

Flips its child's `Success` and `Failure` results.

## What it does

`Success` becomes `Failure`, `Failure` becomes `Success`, and `Running` passes through unchanged.

## Example use case

Turning a "target visible" condition into "target NOT visible" without writing a second condition node.
