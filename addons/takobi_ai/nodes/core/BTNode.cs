using Godot;

namespace TakobiAI;

[Tool, GlobalClass]
public abstract partial class BTNode : Node
{
    public bool IsRunning { get; private set; }

    protected abstract Status OnTick(BTContext ctx);

    protected virtual void OnEnter(BTContext ctx) { }
    protected virtual void OnExit(BTContext ctx, Status status) { }
    protected virtual void OnAbort(BTContext ctx) { }
    
    public virtual float GetWeight(BTContext ctx) => 0f;

    public Status LastStatus { get; private set; } = Status.Failure;
    public int TickCount { get; private set; }

    public Status Tick(BTContext ctx)
    {
        if (!IsRunning)
        {
            IsRunning = true;
            OnEnter(ctx);
        }

        Status status = OnTick(ctx);

        LastStatus = status;
        TickCount++;

        if (status != Status.Running)
        {
            IsRunning = false;
            OnExit(ctx, status);
        }

        return status;
    }

    public void Abort(BTContext ctx)
    {
        if (!IsRunning) return;
        IsRunning = false;
        OnAbort(ctx);
        OnExit(ctx, Status.Failure);
    }
}



