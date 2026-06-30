using Godot;

namespace TakobiAI.Leaves;

[Tool, GlobalClass, Icon("uid://2s56sap7w5jv")]
public partial class IncrementValue : BTAction
{
    [Export] public StringName Key { get; set; } = string.Empty;
    [Export] public Variant Amount
    {
        get => _amount;
        set
        {
            _amount = value;
            resolver = BBValueResolver.From(value);
        }
    }

    private Variant _amount = 1f;
    private BBValueResolver resolver;

    protected override Status OnTick(BTContext ctx)
    {
        if (string.IsNullOrEmpty(Key))
        {
            GD.PushWarning($"[BT] {Name}: Key is not assigned.");
            return Status.Failure;
        }

        if (!ctx.Blackboard.Has(Key))
        {
            GD.PushWarning($"[BT] {Name}: Key '{Key}' does not exist in the Blackboard.");
            return Status.Failure;
        }

        Variant existing = ctx.Blackboard.GetValue(Key);
        Variant resolved = resolver.Resolve(ctx);

        switch (existing.VariantType)
        {
            case Variant.Type.Int:
                ctx.Blackboard.SetValue(Key, existing.AsInt32() + resolved.AsInt32());
                return Status.Success;
            
            case Variant.Type.Float:
                ctx.Blackboard.SetValue(Key, existing.AsSingle() + resolved.AsSingle());
                return Status.Success;
        }

        GD.PushWarning($"[BT] {Name}: Key '{Key}' is not a numeric type (int or float).");
        return Status.Failure;
    }
}

