using Godot;

namespace TakobiAI.Editor;

public partial class BTGlobalMetrics : Node
{
    public static BTGlobalMetrics Instance { get; private set; }

    private int treeCount;
    private int activeTreeCount;

    public override void _EnterTree()
    {
        Instance = this;

        Performance.AddCustomMonitor("TakobiAI/total_trees", new Callable(this, MethodName.GetTreeCount));
        Performance.AddCustomMonitor("TakobiAI/total_active_trees", new Callable(this, MethodName.GetActiveTreeCount));
    }

    public override void _ExitTree()
    {
        Performance.RemoveCustomMonitor("TakobiAI/total_trees");
        Performance.RemoveCustomMonitor("TakobiAI/total_active_trees");
    }

    public void TreeRegistered(bool isActive)
    {
        treeCount++;
        if (isActive) activeTreeCount++;
    }

    public void TreeUnregistered(bool isActive)
    {
        treeCount--;
        if (isActive) activeTreeCount--;
    }

    public void TreeActiveChanged(bool isNowActive) =>
        activeTreeCount += isNowActive ? 1 : -1;

    private double GetTreeCount() => treeCount;
    private double GetActiveTreeCount() => activeTreeCount;
}