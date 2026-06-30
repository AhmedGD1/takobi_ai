using Godot;
using Godot.Collections;

namespace TakobiAI.Decorators;

[Tool, GlobalClass, Icon("uid://c24vbxn7i5v7")]
public partial class Weight : BTDecorator
{
    public enum WeightSource { Constant, Blackboard }

    [Export] 
    public WeightSource Source
    {
        get => _source;
        set
        {
            if (_source == value)
                return;

            _source = value;
            NotifyPropertyListChanged();
        }
    }

    [Export(PropertyHint.Range, "0,100,1,suffix:%")] 
    public float Amount { get; set; } = 50f;

    [Export(PropertyHint.PlaceholderText, "key name")]
    public StringName BlackboardKey { get; set; } = "";

    private WeightSource _source = WeightSource.Constant;

    public override float GetWeight(BTContext ctx)
    {
        return Source == WeightSource.Constant
            ? Amount
            : ctx.Blackboard.GetValue<float>(BlackboardKey);
    }

    protected override Status OnTick(BTContext ctx) =>
        Child?.Tick(ctx) ?? Status.Failure;

    public override void _ValidateProperty(Dictionary property)
    {
        string name = property["name"].AsString();

        if (name == nameof(BlackboardKey) && Source == WeightSource.Constant)
            property["usage"] = (int)PropertyUsageFlags.None;

        if (name == nameof(Amount) && Source == WeightSource.Blackboard)
            property["usage"] = (int)PropertyUsageFlags.None;
    }
}

