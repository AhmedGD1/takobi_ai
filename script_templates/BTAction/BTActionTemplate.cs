// meta-name: BT Action
// meta-description: Creates a Takobi AI action node.

using Godot;
using System;

namespace TakobiAI.Leaves;

[Tool, GlobalClass]
public partial class _CLASS_ : BTAction
{
    protected override void OnEnter(BTContext ctx)
    {
        base.OnEnter(ctx);
        // Called once when the node starts.
    }

    protected override Status OnTick(BTContext ctx)
    {
        // Return Success, Failure, or Running.
        return Status.Success;
    }

    protected override void OnExit(BTContext ctx, Status status)
    {
        base.OnExit(ctx, status);
        // Called when node finishes execution.
    }

    protected override void OnAbort(BTContext ctx)
    {
        base.OnAbort(ctx);
        // Called if execution is interrupted.
    }
}