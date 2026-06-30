using Godot;
using Godot.Collections;

namespace TakobiAI.Leaves;

[Tool, GlobalClass, Icon("uid://mf62xm6hskh7")]
public partial class CallMethod : BTAction
{
    #region Properties
    
    [Export] public Node Source { get => source; private set => SetSource(value); }
    [Export] public StringName Method { get => method; private set => SetMethodName(value); }
    [Export] public Array Args { get => args; private set => SetArgs(value); }

    [ExportGroup("Return Value")]
    [Export] public StringName StoreResultKey { get; set; } = string.Empty;
    [Export] public bool FailOnFalseReturn { get; set; } = false;

    #endregion

    #region Fields

    private Node source;
    private StringName method;
    private Array args = [];

    private ArgInfo[] argsCache;
    private readonly Array cachedResolvedArgs = [];

    #endregion

    public override void _Ready()
    {
        base._Ready(); // important
        argsCache = ArgsResolver.BuildCache(Args);
    }

    #region Leaf Tick

    protected override Status OnTick(BTContext ctx)
    {
        Variant result;

        if (args.Count == 0)
            result = source.Call(Method);
        else
        {
            ArgsResolver.ResolveArgs(ctx, argsCache, cachedResolvedArgs);
            result = source.Callv(Method, cachedResolvedArgs);
        }

        if (!string.IsNullOrEmpty(StoreResultKey))
            ctx.Blackboard.SetValue(StoreResultKey, result);

        if (FailOnFalseReturn)
        {
            if (result.VariantType == Variant.Type.Nil) return Status.Failure;
            if (result.VariantType == Variant.Type.Bool && !result.AsBool()) return Status.Failure;
        }

        return Status.Success;
    }

    #endregion

    #region Configuration Warnings    

    public override string[] _GetConfigurationWarnings()
    {
        if (Source is null)
            return ["Source is not assigned."];

        if (string.IsNullOrEmpty(Method))
            return ["Method is not assigned."];

        if (!Source.HasMethod(Method))
            return [$"'{Source.Name}' does not contain method '{Method}'."];
        
        return ArgsResolver.GetMismatchWarning("Method", Method, Source.GetMethodList(), Args);
    }

    #endregion

    #region Property Set

    private void SetSource(Node value)
    {
        if (source == value) return;

        source = value;
        NotifyPropertyListChanged();
        UpdateConfigurationWarnings();
    }

    private void SetArgs(Array value)
    {
        args = value; 
        argsCache = ArgsResolver.BuildCache(Args);
        UpdateConfigurationWarnings(); 
    }

    private void SetMethodName(StringName value)
    {
        method = value; 
        UpdateConfigurationWarnings(); 
        NotifyPropertyListChanged();
    }

    #endregion
}

