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

    [Export(PropertyHint.Range, "0.05, 10, suffix:s")]
    public float Duration { get; set; } = 1f;

    [Export] public TriggerOn Trigger { get; set; } = TriggerOn.Success;

    private ulong startTime;
    private bool isOnCooldown;

    private double Elapsed => (Time.GetTicksMsec() - startTime) / 1000.0;

    protected override Status OnTick(BTContext ctx)
    {
        if (Child is null) return Status.Failure;

        if (isOnCooldown)
        {
            if (Elapsed < Duration)
                return Status.Failure;

            isOnCooldown = false;
        }

        Status status = Child.Tick(ctx);

        bool shouldTrigger = Trigger switch
        {
            TriggerOn.Success => status == Status.Success,
            TriggerOn.Failure => status == Status.Failure,
            TriggerOn.Both    => status != Status.Running,
            _                 => false
        };

        if (shouldTrigger)
        {
            isOnCooldown = true;
            startTime = Time.GetTicksMsec();
        }

        return status;
    }

    public void Reset() => isOnCooldown = false;
}

