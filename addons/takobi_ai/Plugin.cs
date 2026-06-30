#if TOOLS
using Godot;

namespace TakobiAI.Editor;

[Tool]
public partial class Plugin : EditorPlugin
{
    private const string MetricsAutoloadName = "BTGlobalMetrics";
    private const string MetricsAutoloadPath = "res://addons/takobi_ai/editor/BTGlobalMetrics.cs";

    private BehaviorTreeInspectorPlugin behaviorTreePlugin;
    private MethodCallInspectorPlugin methodCallPlugin;
    private SignalEmitterInspectorPlugin signalTriggerPlugin;
    private AwaitSignalInspectorPlugin awaitSignalInspectorPlugin;
    private BlackboardCompareInspectorPlugin blackboardCompareInspectorPlugin;
    private BTDebuggerPlugin debuggerPlugin;

    public override void _EnterTree()
    {
        behaviorTreePlugin = new BehaviorTreeInspectorPlugin();
        methodCallPlugin = new MethodCallInspectorPlugin();
        signalTriggerPlugin = new SignalEmitterInspectorPlugin();
        awaitSignalInspectorPlugin = new AwaitSignalInspectorPlugin();
        blackboardCompareInspectorPlugin = new BlackboardCompareInspectorPlugin();
        debuggerPlugin = new BTDebuggerPlugin();

        AddInspectorPlugin(behaviorTreePlugin);
        AddInspectorPlugin(methodCallPlugin);
        AddInspectorPlugin(signalTriggerPlugin);
        AddInspectorPlugin(awaitSignalInspectorPlugin);
        AddInspectorPlugin(blackboardCompareInspectorPlugin);
        AddDebuggerPlugin(debuggerPlugin);

        AddAutoloadSingleton(MetricsAutoloadName, MetricsAutoloadPath);
    }

    public override void _ExitTree()
    {
        RemoveInspectorPlugin(behaviorTreePlugin);
        RemoveInspectorPlugin(methodCallPlugin);
        RemoveInspectorPlugin(signalTriggerPlugin);
        RemoveInspectorPlugin(awaitSignalInspectorPlugin);
        RemoveInspectorPlugin(blackboardCompareInspectorPlugin);
        RemoveDebuggerPlugin(debuggerPlugin);

        RemoveAutoloadSingleton(MetricsAutoloadName);
    }
}
#endif

