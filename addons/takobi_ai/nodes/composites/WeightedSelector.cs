using System;
using System.Linq;
using Godot;
using TakobiAI.Decorators;

namespace TakobiAI.Composites;

[Tool, GlobalClass, Icon("uid://cvlicgbm21ebg")]
public partial class WeightedSelector : BTComposite
{
    private RandomNumberGenerator rng = new();

    private int runningIndex = -1;

    protected override void OnEnter(BTContext ctx) => runningIndex = -1;

    protected override Status OnTick(BTContext ctx)
    {
        if (runningIndex != -1)
        {
            Status s = Children[runningIndex].Tick(ctx);
            if (s != Status.Running)
                runningIndex = -1;
            return s;
        }

        int index = PickWeightedIndex(ctx);
        if (index == -1)
            return Status.Failure;

        Status status = Children[index].Tick(ctx);
        runningIndex = status == Status.Running ? index : -1;
        return status;
    }

    private int PickWeightedIndex(BTContext ctx)
    {
        float total = 0f;
        Span<float> weights = stackalloc float[Children.Length];

        for (int i = 0; i < Children.Length; i++)
        {
            float w = Mathf.Max(0f, Children[i].GetWeight(ctx));
            weights[i] = w;
            total += w;
        }

        if (total <= 0f)
            return -1;

        float roll = rng.Randf() * total;
        float acc = 0f;

        for (int i = 0; i < weights.Length; i++)
        {
            acc += weights[i];
            if (roll < acc)
                return i;
        }

        return weights.Length - 1;
    }

    public override string[] _GetConfigurationWarnings()
    {
        bool invalid = GetChildren()
            .OfType<BTNode>()
            .Any(child => child is not Weight);

        if (invalid)
            return ["WeightedSelector children should use a Weight decorator."];
        return base._GetConfigurationWarnings();
    }
}
