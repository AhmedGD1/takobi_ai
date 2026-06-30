using Godot;
using System;

namespace TakobiAI.Composites;

[Tool, GlobalClass, Icon("uid://b3k5dv4bbo4n6")]
public partial class Parallel : BTComposite
{
    public enum Policy
    {
        One,
        All
    }

    [Export] public Policy SuccessPolicy { get; set; } = Policy.All;
    [Export] public Policy FailurePolicy { get; set; } = Policy.One;

    private Status[] results;

    protected override void OnEnter(BTContext ctx)
    {
        if (results == null || results.Length != Children.Length)
            results = new Status[Children.Length];

        Array.Fill(results, Status.Running);
    }

    protected override Status OnTick(BTContext ctx)
    {
        int successCount = 0;
        int failureCount = 0;

        for (int i = 0; i < Children.Length; i++)
        {
            switch (results[i])
            {
                case Status.Success:
                    successCount++;
                    continue;

                case Status.Failure:
                    failureCount++;
                    continue;
            }

            Status result = Children[i].Tick(ctx);
            results[i] = result;

            switch (result)
            {
                case Status.Success:
                    successCount++;

                    if (SuccessPolicy == Policy.One)
                    {
                        AbortRunningChildren(ctx, i);
                        return Status.Success;
                    }
                    break;

                case Status.Failure:
                    failureCount++;

                    if (FailurePolicy == Policy.One)
                    {
                        AbortRunningChildren(ctx, i);
                        return Status.Failure;
                    }
                    break;
            }
        }

        if (SuccessPolicy == Policy.All && successCount == Children.Length)
            return Status.Success;

        if (FailurePolicy == Policy.All && failureCount == Children.Length)
            return Status.Failure;

        return Status.Running;
    }

    private void AbortRunningChildren(BTContext ctx, int except)
    {
        for (int i = 0; i < Children.Length; i++)
        {
            if (i == except)
                continue;

            if (results[i] == Status.Running)
                Children[i].Abort(ctx);
        }
    }
}

