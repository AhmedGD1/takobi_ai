#if TOOLS
using Godot;
using TakobiAI.Conditions;

namespace TakobiAI.Editor;

public partial class BlackboardCompareInspectorPlugin : EditorInspectorPlugin
{
    public override bool _CanHandle(GodotObject obj) => obj is BlackboardCompare;

    public override bool _ParseProperty(GodotObject obj, Variant.Type type, string name,
        PropertyHint hintType, string hintString, PropertyUsageFlags usageFlags, bool wide)
    {
        if (obj is not BlackboardCompare node) return false;

        if (name == nameof(BlackboardCompare.Mode))
        {
            AddPropertyEditor(name, new BlackboardCompareEditor(node));
            return true;
        }

        // Value is rendered inside the custom editor
        if (name == nameof(BlackboardCompare.Value))
            return true;

        return false;
    }
}
#endif

