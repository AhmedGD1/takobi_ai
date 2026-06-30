#if TOOLS
using Godot;
using TakobiAI.Leaves;

namespace TakobiAI.Editor;

public partial class MethodCallInspectorPlugin : EditorInspectorPlugin
{
    public override bool _CanHandle(GodotObject obj) => obj is CallMethod;

    public override bool _ParseProperty(GodotObject obj, Variant.Type type, string name, 
        PropertyHint hintType, string hintString, PropertyUsageFlags usageFlags, bool wide)
    {
        if (obj is not CallMethod cm) 
            return false;

        if (name == nameof(CallMethod.Method))
        {
            if (cm.Source is null)
                return true;

            AddPropertyEditor(name, new MethodSelectorEditorProperty(cm));
            return true; 
        }

        if (name == nameof(CallMethod.Args))
        {
            if (cm.Source is null) 
                return true;

            return ArgsResolver.GetArgCount(cm.Method, cm.Source.GetMethodList()) == 0;
        }

        return false;
    }
}
#endif

