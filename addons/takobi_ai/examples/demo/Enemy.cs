using Godot;

namespace TakobiAI.Demo;

[GlobalClass]
public partial class Enemy : CharacterBody2D
{
    /// <summary>
    /// Blackboard used in Enemy's behavior Tree
    /// </summary>
    [Export] private Blackboard blackboard;

    public override void _Ready()
    {
        var player = GetTree().GetFirstNodeInGroup("Player") as CharacterBody2D;
        
        // adds player to blackboard as "target"
        blackboard.SetValue("target", player);
    }

    public override void _PhysicsProcess(double delta)
    {
        MoveAndSlide();
    }

    // called by player script
    public void ReceiveDamage(float amount)
    {
        var currentHealth = blackboard.GetValue<float>("health");
        blackboard.SetValue("health", currentHealth - amount);
    }

    public void Die()
    {
        QueueFree();
    }
}
