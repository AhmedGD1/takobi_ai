using Godot;

namespace TakobiAI.Composites;

[Tool, GlobalClass, Icon("uid://vd6cr3a85qse")]
public partial class Selector : BTComposite
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
                case Status.Success: return Status.Success;
                case Status.Failure: currentIndex++; break;
            }
        }

        return Status.Failure;
    }
}

