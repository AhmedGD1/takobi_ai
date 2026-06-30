using Godot;

namespace TakobiAI.Composites;

[Tool, GlobalClass, Icon("uid://c0t56nwk1h572")]
public partial class Sequence : BTComposite
{
    private int currentIndex = 0;

    protected override void OnEnter(BTContext ctx) => currentIndex = 0;

    protected override Status OnTick(BTContext ctx)
    {
        while (currentIndex < Children.Length)
        {
            Status status = Children[currentIndex].Tick(ctx);

            switch (status)
            {
                case Status.Running: return Status.Running;
                case Status.Failure: return Status.Failure;
                case Status.Success: currentIndex++; break;
            }
        }

        return Status.Success;
    }
}

