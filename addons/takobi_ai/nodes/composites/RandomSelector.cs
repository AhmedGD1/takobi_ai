using Godot;

namespace TakobiAI.Composites;

[Tool, GlobalClass, Icon("uid://bj8y50etfv0xs")]
public partial class RandomSelector : BTRandomComposite
{
    private int currentIndex;

    protected override void OnEnter(BTContext ctx)
    {
        base.OnEnter(ctx);
        currentIndex = 0;
    }

    protected override Status OnTick(BTContext ctx)
    {
        while (currentIndex < ShuffledChildren.Length)
        {
            var status = ShuffledChildren[currentIndex].Tick(ctx);

            switch (status)
            {
                case Status.Running: return Status.Running;
                case Status.Success: return Status.Success;
                case Status.Failure: currentIndex++; break;
            }
        }

        return Status.Failure;
    }
}


