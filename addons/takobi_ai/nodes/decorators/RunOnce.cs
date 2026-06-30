using Godot;

namespace TakobiAI.Decorators;

[Tool, GlobalClass, Icon("uid://dc1xar1uxn3lj")]
public partial class RunOnce : BTDecorator
{
    public enum ResetMode
    {
        Never,
        OnEnter
    }

    [Export] public ResetMode Reset { get; set; } = ResetMode.Never;

    private bool occurred;

    protected override void OnEnter(BTContext ctx)
    {
        if (Reset == ResetMode.OnEnter)
            occurred = false;
    }

    protected override Status OnTick(BTContext ctx)
    {
        if (occurred) return Status.Success;

        var status = Child?.Tick(ctx) ?? Status.Failure;

        if (status != Status.Running)
            occurred = true;

        return status;
    }
}

