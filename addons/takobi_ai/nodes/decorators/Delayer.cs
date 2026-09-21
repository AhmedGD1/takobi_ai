using Godot;
using Godot.Collections;

namespace TakobiAI.Decorators;

[Tool, GlobalClass, Icon("uid://cf0nvueuq3hhw")]
public partial class Delayer : BTDecorator
{
    public enum DelaySource { Constant, Random, Blackboard }

    [Export] 
    public DelaySource Source
    {
        get => _source;
        private set
        {
            _source = value;
            NotifyPropertyListChanged();
        }
    }

    [Export(PropertyHint.Range, "0.05,10,suffix:s")] 
    public float Duration { get; set; } = 1f;

    [Export(PropertyHint.Range, "0.01,10,suffix:s")]
    public float MinDuration { get; set; } = 0.5f;

    [Export(PropertyHint.PlaceholderText, "key name")] 
    public StringName BlackboardKey { get; set; } = "";

    private TimedGate gate = new();
    
    private double finalDuration;
    private bool hasElapsed;

    #region Backend Fields

    private DelaySource _source;

    #endregion

    protected override void OnEnter(BTContext ctx)
    {
        gate.Start();
        hasElapsed = false;

        finalDuration = Source switch
        {
            DelaySource.Random => ctx.Rng.RandfRange(MinDuration, Duration),
            DelaySource.Constant => Duration,
            DelaySource.Blackboard => ctx.Blackboard.GetValue<double>(BlackboardKey, -1),
            _ => 0
        };
    }

    protected override Status OnTick(BTContext ctx)
    {
        if (!hasElapsed)
        {
            if (!gate.HasElapsed(finalDuration))
                return Status.Running;
            hasElapsed = true;
        }

        return Child?.Tick(ctx) ?? Status.Success;
    }

    public override void _ValidateProperty(Dictionary property)
    {
        string name = property["name"].AsString();

        if (name == nameof(BlackboardKey) && Source != DelaySource.Blackboard)
            property["usage"] = (int)PropertyUsageFlags.None;

        if (name == nameof(Duration) && Source == DelaySource.Blackboard)
            property["usage"] = (int)PropertyUsageFlags.None;

        if (name == nameof(MinDuration) && Source != DelaySource.Random)
            property["usage"] = (int)PropertyUsageFlags.None;
    }
}
