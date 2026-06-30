using System;
using System.Linq;
using Godot;

namespace TakobiAI.Decorators;

[Tool, GlobalClass, Icon("uid://oaw5xhqukxtt")]
public abstract partial class BTDecorator : BTNode
{
    protected BTNode Child { get; private set; }

    public override void _Ready()
    {
        if (Engine.IsEditorHint()) return;
        Child = GetChildren().OfType<BTNode>().FirstOrDefault();
    }

    public override void _Notification(int what)
    {
        if (what == NotificationChildOrderChanged)
            UpdateConfigurationWarnings();
    }

    public override string[] _GetConfigurationWarnings()
    {
        var btChildren = GetChildren().OfType<BTNode>().Count();

        if (btChildren == 0)
            return ["This decorator has no BTNode child and will do nothing."];

        if (btChildren > 1)
            return ["This decorator has more than one BTNode child. Only the first will be used."];
        return [];
    }

    protected override void OnAbort(BTContext ctx) => Child?.Abort(ctx);
}

