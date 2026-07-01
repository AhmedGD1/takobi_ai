# UntilSuccess

Keeps re-running its child until it succeeds.

## What it does

`Failure` is treated as "not done yet" and converted to `Running`, so the child keeps being ticked again and again. Only a `Success` from the child lets `UntilSuccess` finish, at which point it also reports `Success`.

## Example use case

Retrying an action indefinitely until it works — e.g. "keep trying to find a path until one is found."
