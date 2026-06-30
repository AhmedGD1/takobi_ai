using Godot;

namespace TakobiAI;

public readonly struct BBValueResolver
{
    private enum ResolveMode { Constant, Blackboard, }

    private readonly StringName key;
    
    private readonly Variant constant;
    private readonly ResolveMode mode;

    public static BBValueResolver From(Variant value)
    {
        return new BBValueResolver(value);
    }

    private BBValueResolver(Variant value)
    {
        constant = value;
        
        if (value.VariantType is not (Variant.Type.String or Variant.Type.StringName))
            return;
        
        string str = value.AsString();
        if (!str.StartsWith('$'))
            return;
        
        string body = str[1..];

        key = body;
        mode = ResolveMode.Blackboard;
        return;
    }

    public Variant Resolve(BTContext ctx)
    {
        switch (mode)
        {
            case ResolveMode.Constant:
                return constant;
            
            case ResolveMode.Blackboard:
                if (!ctx.Blackboard.Has(key))
                {
                    GD.PushWarning(nameof(BBValueResolver), ": missing key: ", key);
                    return default;
                }
                return ctx.Blackboard.GetValue(key);
            
            default:
                return default;
        }
    }
}

