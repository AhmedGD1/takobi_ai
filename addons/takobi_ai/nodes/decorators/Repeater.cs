using Godot;

namespace TakobiAI.Decorators;

[Tool, GlobalClass, Icon("uid://xepntu78s35c")]
public partial class Repeater : BTDecorator
{
    [Export(PropertyHint.Range, "2,100,1")]
    public int Times { get; set; } = 2;

    [Export] 
    public bool FailOnChildFailure { get; set; }

    private int count;

    protected override void OnEnter(BTContext ctx) => count = 0;

    protected override Status OnTick(BTContext ctx)
    {
        if (Child is null) 
            return Status.Failure;

        Status status = Child.Tick(ctx);

        if (status == Status.Running)
            return Status.Running;
            
        if (status == Status.Failure && FailOnChildFailure) 
            return Status.Failure;

        count++;

        if (count >= Times)
            return Status.Success;
            
        return Status.Running;
    }
}
