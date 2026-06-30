#if TOOLS
using Godot.Collections;

namespace TakobiAI.Editor;

public static class BTSnapshotParser
{
    public static BTTreeSnapshot ParseTree(Array data)
    {
        long treeId = data[0].AsInt64();
        string treeName = data[1].AsString();
        double timestamp = data[2].AsDouble();
        var nodeArray = data[3].AsGodotArray();
        long blackboardId = data[4].AsInt64();

        var nodes = new BTNodeSnapshot[nodeArray.Count];
        for (int i = 0; i < nodeArray.Count; i++)
        {
            var n = nodeArray[i].AsGodotArray();
            nodes[i] = new BTNodeSnapshot
            {
                NodeId = n[0].AsInt64(),
                ParentId = n[1].AsInt64(),
                TypeName = n[2].AsString(),
                Status = (Status)n[3].AsInt32(),
                IsRunning = n[4].AsBool(),
                TickCount = n[5].AsInt32()
            };
        }

        return new BTTreeSnapshot
        {
            TreeId = treeId,
            TreeName = treeName,
            Timestamp = timestamp,
            Nodes = nodes,
            BlackboardId = blackboardId
        };
    }

    public static BTBlackboardSnapshot ParseBlackboard(Array data)
    {
        long blackboardId = data[0].AsInt64();
        var entryArray = data[1].AsGodotArray();

        var entries = new (string, string)[entryArray.Count];
        for (int i = 0; i < entryArray.Count; i++)
        {
            var e = entryArray[i].AsGodotArray();
            entries[i] = (e[0].AsString(), e[1].AsString());
        }

        return new BTBlackboardSnapshot
        {
            BlackboardId = blackboardId,
            Entries = entries
        };
    }
}
#endif