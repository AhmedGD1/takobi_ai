using Godot;

namespace TakobiAI.Decorators;

[Tool, GlobalClass, Icon("uid://021i6gvxlo8q")]
public partial class Retry : BTDecorator
{
    [Export(PropertyHint.Range, "0, 10000")]
    public int MaxAttempts { get; set; } = 3;

    private int attempts;

    protected override void OnEnter(BTContext ctx) => attempts = 0;

    protected override Status OnTick(BTContext ctx)
    {
        if (Child is null) return Status.Failure;

        var status = Child.Tick(ctx);

        if (status == Status.Failure)
        {
            attempts++;

            if (MaxAttempts > 0 && attempts >= MaxAttempts)
                return Status.Failure;
            return Status.Running;
        }

        return status;
    }
}

