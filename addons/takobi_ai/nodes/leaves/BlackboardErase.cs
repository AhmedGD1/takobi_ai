using Godot;

namespace TakobiAI.Leaves;

[Tool, GlobalClass, Icon("uid://b4ojjldx1lp1b")]
public partial class BlackboardErase : BTAction
{
    [Export] public StringName Key { get; set; } = string.Empty;

    protected override Status OnTick(BTContext ctx)
    {
        return ctx.Blackboard.Erase(Key) ? Status.Success : Status.Failure;
    }
}

