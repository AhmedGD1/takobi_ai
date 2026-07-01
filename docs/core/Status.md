# Status

The three possible results of a node tick.

## Values

| Value | Meaning |
|---|---|
| `Success` | The node finished successfully. |
| `Failure` | The node finished unsuccessfully. |
| `Running` | The node hasn't finished yet and will be ticked again next frame. |

Every `BTNode.OnTick` returns one of these. Composites use the pattern of results from their children to decide what to do next; decorators use it to modify or react to their child's outcome.
