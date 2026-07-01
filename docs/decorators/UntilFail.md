# UntilFail

Keeps re-running its child until it fails.

## What it does

`Success` is treated as "not done yet" and converted to `Running`, so the child keeps being ticked again and again. Only a `Failure` from the child lets `UntilFail` finish, at which point it reports `Success`.

## Example use case

Repeating an action for as long as a condition holds — e.g. "keep gathering resources until there are none left to gather."
