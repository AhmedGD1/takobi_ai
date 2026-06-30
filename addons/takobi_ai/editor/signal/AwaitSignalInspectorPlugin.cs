#if TOOLS
using Godot;
using TakobiAI.Leaves;

namespace TakobiAI.Editor;

public partial class AwaitSignalInspectorPlugin : EditorInspectorPlugin
{
    public override bool _CanHandle(GodotObject obj) => obj is AwaitSignal;

    public override bool _ParseProperty(GodotObject obj, Variant.Type type, string name, 
        PropertyHint hintType, string hintString, PropertyUsageFlags usageFlags, bool wide)
    {
        if (obj is not AwaitSignal awaiter)
            return false;

        if (name == nameof(AwaitSignal.Signal))
        {
            if (awaiter.Emitter is null) 
                return true;

            AddPropertyEditor(name, new SignalSelectorEditorProperty(awaiter.Emitter));
            return true;
        }

        return false;
    }
}
#endif