using Godot;

namespace TakobiAI.Conditions;

[Tool, GlobalClass, Icon("uid://ck7occvnntf0s")]
public partial class BlackboardCompare : BlackboardCondition
{
    public enum CompareMode
    {
        Equal,
        NotEqual,
        Less,
        Greater,
        LessOrEqual,
        GreaterOrEqual
    }

    [Export] public CompareMode Mode 
    { 
        get => _mode; 
        set => _mode = value;
    }

    [Export] public Variant Value
    {
        get => _value;
        set
        {
            _value = value;
            resolver = BBValueResolver.From(value);
        }
    }

    #region Backend Fields

    private CompareMode _mode = CompareMode.Equal;
    private Variant _value = 0f;

    #endregion

    private BBValueResolver resolver;

    protected override bool Check(BTContext ctx)
    {
        if (!ctx.Blackboard.Has(Key))
            return false;

        Variant existing = ctx.Blackboard.GetValue(Key);
        Variant resolved = resolver.Resolve(ctx);

        return Compare(existing, resolved);
    }

    private bool Compare(Variant existing, Variant value) => existing.VariantType switch
    {
        Variant.Type.Bool       => CompareBool(existing.AsBool(), value.AsBool()),
        Variant.Type.Int        => CompareOrdered(existing.AsInt32().CompareTo(value.AsInt32())),
        Variant.Type.Float      => CompareOrdered(existing.AsDouble().CompareTo(value.AsDouble())),
        Variant.Type.String     => CompareEquatable(existing.AsString() == value.AsString()),
        Variant.Type.StringName => CompareEquatable(existing.AsStringName() == value.AsStringName()),
        Variant.Type.NodePath   => CompareEquatable(existing.AsNodePath() == value.AsNodePath()),
        Variant.Type.Vector2    => CompareEquatable(existing.AsVector2() == value.AsVector2()),
        Variant.Type.Vector3    => CompareEquatable(existing.AsVector3() == value.AsVector3()),
        Variant.Type.Vector2I   => CompareEquatable(existing.AsVector2I() == value.AsVector2I()),
        Variant.Type.Vector3I   => CompareEquatable(existing.AsVector3I() == value.AsVector3I()),
        _ => Unsupported(existing.VariantType)
    };

    private bool CompareBool(bool a, bool b)
    {
        if (Mode is not (CompareMode.Equal or CompareMode.NotEqual))
        {
            GD.PushWarning($"[BT] {Name}: Bool only supports Equal / NotEqual.");
            return false;
        }
        return Mode == CompareMode.Equal ? a == b : a != b;
    }

    private bool CompareEquatable(bool isEqual) => Mode switch
    {
        CompareMode.Equal => isEqual,
        CompareMode.NotEqual => !isEqual,
        _ => Unsupported(Value.VariantType)
    };

    private bool CompareOrdered(int cmp) => Mode switch
    {
        CompareMode.Equal          => cmp == 0,
        CompareMode.NotEqual       => cmp != 0,
        CompareMode.Less           => cmp < 0,
        CompareMode.Greater        => cmp > 0,
        CompareMode.LessOrEqual    => cmp <= 0,
        CompareMode.GreaterOrEqual => cmp >= 0,
        _ => false
    };

    private bool Unsupported(Variant.Type type)
    {
        GD.PushWarning($"[BT] {Name}: unsupported type '{type}' for comparison.");
        return false;
    }
}

