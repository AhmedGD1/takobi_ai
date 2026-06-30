using System.Linq;
using Godot;

namespace TakobiAI.Composites;

[Tool, GlobalClass, Icon("uid://b6o47ynleokr8")]
public abstract partial class BTComposite : BTNode
{
    protected BTNode[] Children { get; private set; }

    public override void _Ready()
    {
        if (Engine.IsEditorHint()) return;

        Children = [.. GetChildren().OfType<BTNode>()];
    }

    public override void _Notification(int what)
    {
        if (what == NotificationChildOrderChanged)
            UpdateConfigurationWarnings();
    }

    public override string[] _GetConfigurationWarnings()
    {
        if (!GetChildren().OfType<BTNode>().Any())
            return ["This composite has no BTNode children and will do nothing."];
        return [];
    }

    protected override void OnAbort(BTContext ctx)
    {
        foreach (var child in Children)
            child.Abort(ctx);
    }
}

