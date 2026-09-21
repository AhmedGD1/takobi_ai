using Godot;

namespace TakobiAI.Decorators;

[Tool, GlobalClass, Icon("uid://cvutxaywpij07")]
public partial class Cooldown : BTDecorator
{
    public enum TriggerOn
    {
        Success,
        Failure,
        Both
    }

    [Export(PropertyHint.Range, "0.05,50,0.05,suffix:s")]
    public float Duration { get; set; } = 1f;

    [Export] public TriggerOn Trigger { get; set; } = TriggerOn.Success;

    private TimedGate gate = new();
    
    private bool isOnCooldown;

    protected override Status OnTick(BTContext ctx)
    {
        if (Child is null) 
            return Status.Failure;
    
        if (isOnCooldown)
        {
            if (!gate.HasElapsed(Duration))
                return Status.Failure;
            isOnCooldown = false;
        }
    
        Status status = Child.Tick(ctx);
    
        bool shouldTrigger = Trigger switch
        {
            TriggerOn.Success => status == Status.Success,
            TriggerOn.Failure => status == Status.Failure,
            TriggerOn.Both => status != Status.Running,
            _ => false
        };
    
        if (shouldTrigger)
        {
            isOnCooldown = true;
            gate.Start();
        }
    
        return status;
    }

    public void Reset() => isOnCooldown = false;
}
