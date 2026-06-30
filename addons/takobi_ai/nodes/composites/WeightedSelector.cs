using System.Linq;
using Godot;
using TakobiAI.Decorators;

namespace TakobiAI.Composites;

[Tool, GlobalClass, Icon("uid://cvlicgbm21ebg")]
public partial class WeightedSelector : BTComposite
{
    private int runningIndex = -1;

    protected override void OnEnter(BTContext ctx) => runningIndex = -1;

    protected override Status OnTick(BTContext ctx)
    {
        int bestIndex = -1;
        float bestScore = float.MinValue;

        for (int i = 0; i < Children.Length; i++)
        {
            float score = Children[i].GetWeight(ctx);

            if (score > bestScore)
            {
                bestScore = score;
                bestIndex = i;
            }
        }

        if (bestIndex == -1)
            return Status.Failure;

        if (runningIndex != -1 && runningIndex != bestIndex)
            Children[runningIndex].Abort(ctx);

        Status status = Children[bestIndex].Tick(ctx);

        runningIndex = status == Status.Running ? bestIndex : -1;

        return status;
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

