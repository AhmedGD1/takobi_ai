using Godot;

namespace TakobiAI.Leaves;

[Tool, GlobalClass, Icon("uid://peochc6cenjp")]
public partial class AwaitSignal : BTAction
{   
    [Export] public Node Emitter
    {
        get => emitter;
        private set
        {
            if (emitter == value) return;

            emitter = value;
            NotifyPropertyListChanged();
            UpdateConfigurationWarnings();
        }
    }

    [Export] public StringName Signal
    {
        get => signal;
        private set
        {
            signal = value;
            NotifyPropertyListChanged();
        }
    }

    private Node emitter;
    private StringName signal = "";
    private Callable onSignalCallable;

    private bool finished;

    public override string[] _GetConfigurationWarnings()
    {
        if (Emitter is null)
            return ["Signal Emitter isn't assigned."];
        return [];
    }

    protected override void OnEnter(BTContext ctx)
    {
        finished = false;
        onSignalCallable = new Callable(this, MethodName.OnSignal);
        emitter.Connect(signal, onSignalCallable, (uint)ConnectFlags.OneShot);
    }

    protected override Status OnTick(BTContext ctx)
    {
        return finished
            ? Status.Success
            : Status.Running;
    }

    protected override void OnAbort(BTContext ctx)
    {
        if (emitter.IsConnected(signal, onSignalCallable))
            emitter.Disconnect(signal, onSignalCallable);
    }

    private void OnSignal() => finished = true;
    private void OnSignal(Variant a) => finished = true;
    private void OnSignal(Variant a, Variant b) => finished = true;
    private void OnSignal(Variant a, Variant b, Variant c) => finished = true;
    private void OnSignal(Variant a, Variant b, Variant c, Variant d) => finished = true;
    private void OnSignal(Variant a, Variant b, Variant c, Variant d, Variant e) => finished = true;
    private void OnSignal(Variant a, Variant b, Variant c, Variant d, Variant e, Variant f) => finished = true;
    private void OnSignal(Variant a, Variant b, Variant c, Variant d, Variant e, Variant f, Variant g) => finished = true;
    private void OnSignal(Variant a, Variant b, Variant c, Variant d, Variant e, Variant f, Variant g, Variant h) => finished = true;
}

