using Godot;

namespace TakobiAI.Decorators;

[Tool, GlobalClass, Icon("uid://iepe8ac3jwba")]
public partial class Succeeder : BTDecorator
{
    protected override Status OnTick(BTContext ctx)
    {
        if (Child is null) return Status.Failure;

        Status status = Child.Tick(ctx);
        if (status == Status.Running) return Status.Running;

        return Status.Success;
    }
}