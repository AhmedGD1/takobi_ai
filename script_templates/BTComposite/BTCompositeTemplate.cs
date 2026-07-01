// meta-name: BT Composite
// meta-description: Creates a Takobi AI composite node (Sequence, Selector, etc).

using Godot;
using System;

namespace TakobiAI.Composites;

[Tool, GlobalClass]
public partial class _CLASS_ : BTComposite
{
    protected override void OnEnter(BTContext ctx)
    {
        base.OnEnter(ctx);
        // Called when composite starts evaluating children.
    }

    protected override Status OnTick(BTContext ctx)
    {
        // Iterate and tick children here.
        return Status.Success;
    }

    protected override void OnExit(BTContext ctx, Status status)
    {
        base.OnExit(ctx, status);
        // Called when composite finishes.
    }
}