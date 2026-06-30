using Godot;

namespace TakobiAI.Decorators;


[Tool, GlobalClass, Icon("uid://d0skgehivurbo")]
public partial class UntilSuccess : BTDecorator
{
    protected override Status OnTick(BTContext ctx)
    {
        if (Child is null) return Status.Failure;
        var status = Child.Tick(ctx);
        return status == Status.Success ? Status.Success : Status.Running;
    }
}


