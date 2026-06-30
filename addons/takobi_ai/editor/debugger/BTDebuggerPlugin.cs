#if TOOLS
using Godot;
using Godot.Collections;

namespace TakobiAI.Editor;

[Tool]
public partial class BTDebuggerPlugin : EditorDebuggerPlugin
{
    private BTDebuggerPanel panel;

    public override bool _HasCapture(string prefix) => prefix == "bt_debugger";

    public override void _SetupSession(int sessionId)
    {
        panel ??= new BTDebuggerPanel { Name = "Behavior Trees" };

        var session = GetSession(sessionId);
        session.AddSessionTab(panel);
    }

    public override bool _Capture(string message, Array data, int sessionId)
    {
        if (panel is null)
            return false;

        string suffix = message["bt_debugger:".Length..];

        switch (suffix)
        {
            case "tree_snapshot":
                panel.OnTreeSnapshot(BTSnapshotParser.ParseTree(data));
                return true;

            case "blackboard_snapshot":
                panel.OnBlackboardSnapshot(BTSnapshotParser.ParseBlackboard(data));
                return true;

            default:
                return false;
        }
    }
}
#endif