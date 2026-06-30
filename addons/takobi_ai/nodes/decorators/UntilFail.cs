using Godot;

namespace TakobiAI.Decorators;

[Tool, GlobalClass, Icon("uid://c0rfpj623gt0y")]
public partial class UntilFail : BTDecorator
{
    protected override Status OnTick(BTContext ctx)
    {
        if (Child is null) return Status.Success;

        return Child.Tick(ctx) switch
        {
            Status.Failure => Status.Success,
            _              => Status.Running
        };
    }
}

