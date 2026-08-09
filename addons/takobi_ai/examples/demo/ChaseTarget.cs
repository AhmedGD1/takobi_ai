using Godot;
using TakobiAI.Leaves;

namespace TakobiAI.Demo;

[Tool, GlobalClass]
public partial class ChaseTarget : BTAction
{
	[Export] private float speed = 60f;

	private readonly StringName TargetKey = "target";

	protected override Status OnTick(BTContext ctx)
	{
		var agent = ctx.Agent as CharacterBody2D;
		var target = ctx.Blackboard.GetValue<Node2D>(TargetKey, null);

		if (target is null)
			return Status.Failure;

		var direction = agent.GlobalPosition.DirectionTo(target.GlobalPosition);
		agent.Velocity = direction * speed;

		return Status.Running;
	}
}
