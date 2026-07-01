// meta-name: BT Decorator
// meta-description: Creates a Takobi AI decorator node (modifies child behavior).

using Godot;
using System;

namespace TakobiAI.Decorators;

[Tool, GlobalClass, Icon("uid://dpvb2f7pdavcy")]
public partial class _CLASS_ : BTDecorator
{
    protected override Status OnTick(BTContext ctx)
    {
        // Modify or control child execution.
        return Child.Tick(ctx);
    }
}

