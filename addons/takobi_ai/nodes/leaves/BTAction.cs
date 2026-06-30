using Godot;

namespace TakobiAI.Leaves;

[Tool, GlobalClass, Icon("uid://dpvb2f7pdavcy")]
public partial class BTAction : BTNode
{
    protected override Status OnTick(BTContext ctx) => Status.Success;
}

