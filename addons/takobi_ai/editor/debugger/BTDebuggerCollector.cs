#if DEBUG
using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TakobiAI.Editor;

public static class BTDebuggerCollector
{
    private const string MessagePrefix = "bt_debugger";
    private const double SnapshotInterval = 0.1; // 10 Hz

    private static readonly HashSet<BehaviorTree> trees = [];
    private static double elapsed;

    public static void RegisterTree(BehaviorTree tree)
    {
        EnsureHooked();
        trees.Add(tree);
    }

    public static void UnregisterTree(BehaviorTree tree) => trees.Remove(tree);

    private static void EnsureHooked()
    {
        var tree = (SceneTree)Engine.GetMainLoop();

        if (tree.IsConnected(SceneTree.SignalName.ProcessFrame, Callable.From(OnProcessFrame)))
            return;

        tree.ProcessFrame += OnProcessFrame;
    }

    private static void OnProcessFrame()
    {
        if (!EngineDebugger.IsActive())
            return;

        elapsed += ((SceneTree)Engine.GetMainLoop()).Root.GetProcessDeltaTime();
        if (elapsed < SnapshotInterval)
            return;
        elapsed = 0;

        BroadcastSnapshots();
    }

    private static void BroadcastSnapshots()
    {
        var seenBlackboards = new HashSet<ulong>();

        foreach (var tree in trees)
        {
            if (!GodotObject.IsInstanceValid(tree))
                continue;

            EngineDebugger.SendMessage($"{MessagePrefix}:tree_snapshot", BuildTreeSnapshot(tree));

            var bb = tree.Blackboard;
            if (bb is null || !seenBlackboards.Add(bb.GetInstanceId()))
                continue;

            EngineDebugger.SendMessage($"{MessagePrefix}:blackboard_snapshot", BuildBlackboardSnapshot(bb));
        }
    }

    private static Array BuildTreeSnapshot(BehaviorTree tree)
    {
        var nodeData = new Array();
        foreach (var node in tree.GetChildren().OfType<BTNode>())
            CollectNode(node, 0, nodeData);
            
        return
        [
            tree.GetInstanceId(),
            tree.Name.ToString(), // Extract the name as a string
            Time.GetTicksMsec() / 1000.0,
            nodeData,
            tree.Blackboard?.GetInstanceId() ?? 0
        ];
    }
    private static void CollectNode(BTNode node, long parentId, Array nodeData)
    {
        nodeData.Add(new Array
        {
            node.GetInstanceId(),
            parentId,
            node.GetType().Name,
            (int)node.LastStatus,
            node.IsRunning,
            node.TickCount
        });

        long thisId = (long)node.GetInstanceId();
        foreach (var child in node.GetChildren().OfType<BTNode>())
            CollectNode(child, thisId, nodeData);
    }

    private static Array BuildBlackboardSnapshot(Blackboard bb)
    {
        var entries = new Array();
        foreach (var kvp in bb.Data)
            entries.Add(new Array { kvp.Key.ToString(), kvp.Value.ToString() });

        return new Array { bb.GetInstanceId(), entries };
    }
}
#endif