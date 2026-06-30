using Godot;

namespace TakobiAI.Composites;

[Tool, GlobalClass, Icon("uid://du8my8n0cspu")]
public partial class RandomSequence : BTRandomComposite
{
    private int currentIndex = 0;

    protected override void OnEnter(BTContext ctx)
    {
        base.OnEnter(ctx); // important
        currentIndex = 0;
    }

    protected override Status OnTick(BTContext ctx)
    {
        while (currentIndex < ShuffledChildren.Length)
        {
            Status status = ShuffledChildren[currentIndex].Tick(ctx);

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

