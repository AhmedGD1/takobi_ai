using Godot;

namespace TakobiAI.Decorators;

[Tool, GlobalClass, Icon("uid://hcp81td6q0ym")]
public partial class Timeout : BTDecorator
{
    [Export(PropertyHint.Range, "0.05,50,0.05,suffix:s")]
    public float Duration { get; set; } = 1f;

    private TimedGate gate = new();

    protected override void OnEnter(BTContext ctx) => gate.Start();

    protected override Status OnTick(BTContext ctx)
    {
        if (Child is null) 
            return Status.Failure;

        if (gate.HasElapsed(Duration))
        {
            Child.Abort(ctx);
            return Status.Failure;
        }
        
        return Child.Tick(ctx);
    }
}
