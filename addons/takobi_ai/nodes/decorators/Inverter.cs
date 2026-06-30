using Godot;

namespace TakobiAI.Decorators;

[Tool, GlobalClass, Icon("uid://cuhr2cw71miwn")]
public partial class Inverter : BTDecorator
{
    protected override Status OnTick(BTContext ctx)
    {
        if (Child is null) return Status.Failure;
        return Child.Tick(ctx) switch
        {
            Status.Success => Status.Failure,
            Status.Failure => Status.Success,
            _ => Status.Running
        };
    }
}

