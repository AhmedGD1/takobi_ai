using Godot;

namespace TakobiAI.Conditions;

[Tool, GlobalClass]
public abstract partial class BlackboardCondition : BTCondition
{
    [Export] public StringName Key { get; set; } = string.Empty;
}

