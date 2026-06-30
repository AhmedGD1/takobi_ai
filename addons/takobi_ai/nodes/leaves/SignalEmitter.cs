using Godot;
using System;
using Array = Godot.Collections.Array;

namespace TakobiAI.Leaves;

[Tool, GlobalClass, Icon("uid://peochc6cenjp")]
public partial class SignalEmitter : BTAction
{
    #region Properties 

    [Export] public Node Emitter { get => emitter; private set => SetEmitter(value); }
    [Export] public StringName Signal { get => signal; private set => SetSignalName(value); }
    [Export] public Array Args { get => args; private set => SetArgs(value); }

    #endregion

    #region Fields

    private Node emitter;
    private StringName signal;
    private Array args = [];

    private ArgInfo[] argsCache;
    private Variant[] resolvedArgs;

    #endregion

    public override void _Ready()
    {
        base._Ready(); // important
        argsCache = ArgsResolver.BuildCache(Args);
        resolvedArgs = new Variant[argsCache.Length];
    }

    #region Leaf Tick

    protected override Status OnTick(BTContext ctx)
    {
        if (args.Count == 0)
            emitter.EmitSignal(Signal);
        else
        {
            ArgsResolver.ResolveArgs(ctx, argsCache, resolvedArgs);
            emitter.EmitSignal(Signal, resolvedArgs.AsSpan());
        }

        return Status.Success;
    }

    #endregion

    #region Configuration Warnings

    public override string[] _GetConfigurationWarnings()
    {
        if (Emitter is null)
            return ["Emitter is not assigned."];

        if (string.IsNullOrEmpty(Signal))
            return ["Signal is not assigned."];

        if (!Emitter.HasSignal(Signal))
            return [$"'{Emitter.Name}' does not have a signal named '{Signal}'."];

        return ArgsResolver.GetMismatchWarning("Signal", Signal, Emitter.GetSignalList(), Args);
    }

    #endregion

    #region Property Set

    private void SetEmitter(Node value)
    {
        if (emitter == value) return;

        emitter = value;
        NotifyPropertyListChanged();
        UpdateConfigurationWarnings();
    }

    private void SetSignalName(StringName value)
    {
        signal = value; 
        UpdateConfigurationWarnings(); 
        NotifyPropertyListChanged();
    }

    private void SetArgs(Array value)
    {
        args = value; 
        argsCache = ArgsResolver.BuildCache(Args);
        UpdateConfigurationWarnings(); 
    }

    #endregion
}

