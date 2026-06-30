using Godot;

namespace TakobiAI.Conditions;

[Tool, GlobalClass, Icon("uid://baubi72ytw86h")]
public partial class BTCondition : BTNode
{
    protected sealed override Status OnTick(BTContext ctx) =>
        Check(ctx) ? Status.Success : Status.Failure;

    protected virtual bool Check(BTContext ctx) => true;
}

