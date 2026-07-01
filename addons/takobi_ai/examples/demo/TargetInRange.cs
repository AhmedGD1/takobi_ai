using Godot;

namespace TakobiAI.Conditions;

[Tool, GlobalClass]
public partial class TargetInRange : BTCondition
{
	/// <summary>
	/// Required distance to chase player;
	/// </summary>
	[Export] private float range = 100f;

	/// <summary>
	/// Target Key (player)
	/// </summary>
	private readonly StringName TargetKey = "target";

    protected override bool Check(BTContext ctx)
    {
		var agent = ctx.Agent as Node2D;
        var target = ctx.Blackboard.GetValue<Node2D>(TargetKey, null);

		if (target is null)
			return false;
		
		// returns success when player is close enough
		return agent.GlobalPosition.DistanceTo(target.GlobalPosition) < range;
    }

}
