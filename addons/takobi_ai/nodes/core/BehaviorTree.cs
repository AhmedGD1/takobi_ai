using Godot;
using System.Linq;
using TakobiAI.Leaves;
using TakobiAI.Editor;

namespace TakobiAI;

[Tool, GlobalClass, Icon("uid://dbn01u56b8ph0")]
public partial class BehaviorTree : Node
{
    public enum TickMode { Idle, Physics }

    [Signal] public delegate void TreeActiveChangedEventHandler(bool active);

    [Export] public Node Agent { get; set; }

    [Export] public TickMode Mode
    {
        get => mode;
        set
        {
            mode = value;
            SetPhysicsProcess(mode == TickMode.Physics);
            SetProcess(mode == TickMode.Idle);
        }
    }

    [Export] public bool Active
    {
        get => active;
        set
        {
            if (active == value) 
                return;

            active = value;
            SetPhysicsProcess(value && mode == TickMode.Physics);
            SetProcess(value && mode == TickMode.Idle);

            if (!Engine.IsEditorHint() && IsNodeReady())
                BTGlobalMetrics.Instance?.TreeActiveChanged(value);
            EmitSignalTreeActiveChanged(value);
        }
    }

    [ExportGroup("Settings")]
    [Export(PropertyHint.Range, "1, 165")]
    public int TicksPerSecond { get; private set; } = 60;

    [Export] public Blackboard Blackboard { get; private set; }
    [Export] public BTService[] Services { get; private set; } = [];

    [ExportGroup("Debug")]
    [Export] public bool CustomMonitor { get; set; }

    #region Fields

    public SubTree SubTreeOwner { get; private set; }
    public bool IsSubTree { get; private set; }

    private TickMode mode = TickMode.Physics;
    private bool active = true;

    private BTNode root;

    private BTContext originContext;
    private BTContext subContext;

    private double elapsed;

    private string processTimeMetricName;
    private double processTimeMetricValue;

    #endregion

    public BTContext Context => IsSubTree ? subContext : originContext;

    #region Editor

    public override void _Notification(int what)
    {
        if (Engine.IsEditorHint()) return;

        if (what == NotificationChildOrderChanged)
            UpdateConfigurationWarnings();
    }

    public override string[] _GetConfigurationWarnings()
    {
        if (!GetChildren().Any(child => child is BTNode))
            return ["Behavior Tree has no Behavior Tree Node child and will do nothing."];
        return [];
    }

    #endregion

    #region Life Cycle

    public override void _Ready()
    {
        if (Engine.IsEditorHint()) return;

        if (Blackboard is null)
        {
            GD.PushWarning($"[BT] {Name}: Blackboard isn't assigned - new backup created.");
            Blackboard = new();
        }

        Mode = mode;
        Active = active;

        root = GetChildren().OfType<BTNode>().FirstOrDefault();
        RegisterMetrics();
        originContext = new BTContext(Agent, Blackboard);

#if DEBUG
        BTDebuggerCollector.RegisterTree(this);
#endif
    }

    public override void _ExitTree()
    {
        if (Engine.IsEditorHint()) 
            return;
        UnregisterMetrics();

#if DEBUG
        BTDebuggerCollector.UnregisterTree(this);
#endif
    }

    public override void _Process(double delta) => TryTick(delta);
    public override void _PhysicsProcess(double delta) => TryTick(delta);

    #endregion

    #region Tick

    private void TryTick(double delta)
    {
        if (Engine.IsEditorHint() || !active)
            return;

        foreach (var service in Services)
            service.Tick(delta, Context);

        double tickInterval = 1.0 / TicksPerSecond;

        elapsed += delta;
        elapsed = Mathf.Min(elapsed, tickInterval * 2);

        if (elapsed < tickInterval)
            return;

        elapsed -= tickInterval;
        Tick(Context);
    }

    public Status Tick(BTContext ctx)
    {
        if (root is null) 
            return Status.Failure;
        return TimedTick(ctx);
    }

    private Status TimedTick(BTContext ctx)
    {
        if (!CustomMonitor) return root.Tick(ctx);

        ulong start = Time.GetTicksUsec();
        Status status = root.Tick(ctx);
        processTimeMetricValue = Time.GetTicksUsec() - start;
        return status;
    }

    #endregion

    #region Metrics

    private void RegisterMetrics()
    {
#if DEBUG
        BTGlobalMetrics.Instance?.TreeRegistered(active);

        if (!CustomMonitor) return;

        processTimeMetricName = $"TakobiAI/process_time_us_{Name}-{GetInstanceId()}";
        Performance.AddCustomMonitor(processTimeMetricName, Callable.From(GetProcessTimeMetricValue));
#endif
    }

    private void UnregisterMetrics()
    {
        BTGlobalMetrics.Instance?.TreeUnregistered(active);

        if (processTimeMetricName is not null)
            Performance.RemoveCustomMonitor(processTimeMetricName);
    }

    private double GetProcessTimeMetricValue() => processTimeMetricValue;

    #endregion

    #region Utilities

    public void Abort()
    {
        root?.Abort(Context);
    }

    public void Abort(BTContext ctx) => root?.Abort(ctx);

    public void SetSubTree(bool value, BTContext ctx, SubTree owner = null)
    {
        IsSubTree = value;
        SubTreeOwner = owner;
        Active = !value;
        subContext = ctx;

        UpdateConfigurationWarnings();
    }

    #endregion
}

