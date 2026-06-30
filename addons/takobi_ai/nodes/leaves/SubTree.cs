using Godot;

namespace TakobiAI.Leaves;

[Tool, GlobalClass, Icon("uid://cfu162pmpo7mn")]
public partial class SubTree : BTAction
{
    private const int MAX_DEPTH = 16;

    [Export] public BehaviorTree Tree
    {
        get => tree;
        private set
        {
            tree?.SetSubTree(false, null);
            tree = value;

            RebuildContext();
            tree?.SetSubTree(true, childContext, this);

            UpdateConfigurationWarnings();
        }
    }

    [Export]
    public bool ShareBlackboard
    {
        get => shareBlackboard;
        set
        {
            shareBlackboard = value;
            RebuildContext();
            tree?.SetSubTree(true, childContext, this);
        }
    }

    private BehaviorTree tree;
    private BTContext childContext;
    private bool shareBlackboard = true;

    private void RebuildContext()
    {
        childContext = tree is not null
            ? new BTContext(null, ShareBlackboard ? null : tree.Blackboard)
            : null;
    }

    public override void _ExitTree()
    {   
        tree?.SetSubTree(false, null);
    }

    protected override Status OnTick(BTContext ctx)
    {
        if (tree is null)
        {
            GD.PushWarning($"[BT] {Name}: no subtree assigned.");
            return Status.Failure;
        }

        if (ctx.SubTreeDepth >= MAX_DEPTH)
        {
            GD.PushError($"[BT] {Name}: max subtree depth ({MAX_DEPTH}) exceeded. Possible circular reference.");
            return Status.Failure;
        }

        childContext.Agent = ctx.Agent;
        childContext.Blackboard = ShareBlackboard ? ctx.Blackboard : tree.Blackboard;
        childContext.SubTreeDepth = ctx.SubTreeDepth + 1;

        return tree.Tick(childContext);
    }

    protected override void OnAbort(BTContext ctx) => tree?.Abort(childContext);

    public override string[] _GetConfigurationWarnings()
    {
        if (tree is null)
            return ["SubTree has no BehaviorTree assigned."];

        Node current = this;
        while (current is not null)
        {
            if (current == tree)
                return ["Circular reference: SubTree points to an ancestor BehaviorTree."];
            current = current.GetParent();
        }

        return [];
    }
}