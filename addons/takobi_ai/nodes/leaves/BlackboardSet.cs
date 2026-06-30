using Godot;

namespace TakobiAI.Leaves;

[Tool, GlobalClass, Icon("uid://biw8uoihcq8er")]
public partial class BlackboardSet : BTAction
{
    [Export] public StringName Key { get; private set; } = "";

    [Export]
    public Variant Value
    {
        get => _value;
        set
        {
            _value = value;
            resolver = BBValueResolver.From(value);
        }
    }

    private Variant _value = 0f;
    private BBValueResolver resolver;

    protected override Status OnTick(BTContext ctx)
    {
        if (string.IsNullOrEmpty(Key))
        {
            GD.PushWarning("Blackboard Key is not assigned");
            return Status.Failure;
        }

        ctx.Blackboard.SetValue(Key, resolver.Resolve(ctx));
        return Status.Success;
    }
}

