using Godot;

namespace TakobiAI.Conditions;

[Tool, GlobalClass, Icon("uid://dpmn0tam1w8f6")]
public partial class BlackboardHas : BlackboardCondition
{
    protected override bool Check(BTContext ctx) => ctx.Blackboard.Has(Key);
}

