#if TOOLS
using Godot;
using TakobiAI.Leaves;

namespace TakobiAI.Editor;

public partial class SignalEmitterInspectorPlugin : EditorInspectorPlugin
{
    public override bool _CanHandle(GodotObject obj) => obj is SignalEmitter;

    public override bool _ParseProperty(GodotObject obj, Variant.Type type, string name, 
        PropertyHint hintType, string hintString, PropertyUsageFlags usageFlags, bool wide)
    {
        if (obj is not SignalEmitter st)
            return false;

        if (name == nameof(SignalEmitter.Signal))
        {
            if (st.Emitter is null) 
                return true;

            AddPropertyEditor(name, new SignalSelectorEditorProperty(st.Emitter));
            return true;
        }

        if (name == nameof(SignalEmitter.Args))
        {
            if (st.Emitter is null) 
                return true;
            
            return ArgsResolver.GetArgCount(st.Signal, st.Emitter.GetSignalList()) == 0;
        }

        return false;
    }
}
#endif

