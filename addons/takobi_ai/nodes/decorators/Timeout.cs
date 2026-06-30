using Godot;

namespace TakobiAI.Decorators;

[Tool, GlobalClass, Icon("uid://hcp81td6q0ym")]
public partial class Timeout : BTDecorator
{
    [Export(PropertyHint.Range, "0.05, 10, suffix:s")]
    public float Duration { get; set; } = 1f;

    private ulong startTime;

    protected override void OnEnter(BTContext ctx) =>
        startTime = Time.GetTicksMsec();

    protected override Status OnTick(BTContext ctx)
    {
        if (Child == null) return Status.Failure;

        double elapsed = (Time.GetTicksMsec() - startTime) / 1000.0;

        if (elapsed >= Duration)
        {
            Child.Abort(ctx);
            return Status.Failure;
        }

        return Child.Tick(ctx);
    }
}

