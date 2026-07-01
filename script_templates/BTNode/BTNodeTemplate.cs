// meta-name: BT Node
// meta-description: Base Takobi AI behavior tree node.

using Godot;
using System;

namespace TakobiAI;

[Tool, GlobalClass]
public partial class _CLASS_ : BTNode
{
    protected override void OnEnter(BTContext ctx)
    {
        base.OnEnter(ctx);
    }

    protected override Status OnTick(BTContext ctx)
    {
        return Status.Success;
    }

    protected override void OnExit(BTContext ctx, Status status)
    {
        base.OnExit(ctx, status);
    }

    protected override void OnAbort(BTContext ctx)
    {
        base.OnAbort(ctx);
    }
}