#if TOOLS
using Godot;

namespace TakobiAI.Editor;

public partial class BehaviorTreeInspectorPlugin : EditorInspectorPlugin
{
    public override bool _CanHandle(GodotObject obj) => obj is BehaviorTree;

    public override void _ParseBegin(GodotObject obj)
    {
        if (obj is not BehaviorTree bt || !bt.IsSubTree)
            return;

        var label = new RichTextLabel
        {
            BbcodeEnabled = true,
            FitContent = true,
            ScrollActive = false,
            Text =
                "This tree is controlled by a SubTree node.\n" +
                $"Owner: [url=subtree_owner][color=sky_blue][u]SubTree[/u][/color][/url]"
        };

        label.MetaClicked += meta =>
        {
            if (meta.AsString() != "subtree_owner")
                return;

            EditorInterface.Singleton.GetSelection().Clear();
            EditorInterface.Singleton.GetSelection().AddNode(bt.SubTreeOwner);
        };

        AddCustomControl(label);
    }

    public override bool _ParseProperty(GodotObject obj, Variant.Type type, string name, 
        PropertyHint hintType, string hintString, PropertyUsageFlags usageFlags, bool wide)
    {
        if (obj is not BehaviorTree bt || !bt.IsSubTree) return false;

        string[] hideProps = [
            nameof(BehaviorTree.Active), nameof(BehaviorTree.TicksPerSecond), nameof(BehaviorTree.Mode),
            nameof(BehaviorTree.Services),
        ];

        foreach (var p in hideProps)
            if (name == p) return true;
        
        if (name is nameof(BehaviorTree.Blackboard) && bt.SubTreeOwner.ShareBlackboard)
            return true;
        
        return false;
    }
}
#endif

