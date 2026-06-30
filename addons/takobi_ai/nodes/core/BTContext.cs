using Godot;

namespace TakobiAI;

public sealed class BTContext(Node agent, Blackboard blackboard)
{
    public Node Agent { get; set; } = agent;
    public Blackboard Blackboard { get; set; } = blackboard;

    public int SubTreeDepth { get; set; }
}

