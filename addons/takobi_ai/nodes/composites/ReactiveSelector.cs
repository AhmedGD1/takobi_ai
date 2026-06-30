using Godot;

namespace TakobiAI.Composites;

[Tool, GlobalClass]
public partial class ReactiveSelector : BTComposite
{
    private int runningIndex = -1;

    protected override void OnEnter(BTContext ctx) => runningIndex = -1;

    protected override Status OnTick(BTContext ctx)
    {
        for (int i = 0; i < Children.Length; i++)
        {
            var status = Children[i].Tick(ctx);

            if (status == Status.Running)
            {
                if (runningIndex != -1 && runningIndex != i)
                    Children[runningIndex].Abort(ctx);
                runningIndex = i;
                return Status.Running;
            }

            if (status == Status.Success)
            {
                if (runningIndex != -1 && runningIndex != i)
                    Children[runningIndex].Abort(ctx);
                runningIndex = -1;
                return Status.Success;
            }
        }

        runningIndex = -1;
        return Status.Failure;
    }
}

