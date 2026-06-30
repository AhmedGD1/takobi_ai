using Godot;

namespace TakobiAI.Decorators;

[Tool, GlobalClass, Icon("uid://5u6svcbnweva")]
public partial class Chance : BTDecorator
{
    [Export(PropertyHint.Range, "1, 100, 1, suffix:%")] 
    public float Probability { get; set; } = 50f;

    protected override Status OnTick(BTContext ctx)
    {
        if (Child is null) return Status.Failure;
        if (GD.Randf() >= (Probability / 100f)) return Status.Failure;

        return Child.Tick(ctx);
    }
}

