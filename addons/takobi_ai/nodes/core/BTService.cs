using Godot;

namespace TakobiAI;

[Tool, GlobalClass, Icon("uid://ccqhaoqo7ffg5")]
public abstract partial class BTService : Resource
{
    [Export] public float Interval { get; set; } = 0.2f;
    
    private double timer;

    protected abstract void OnTick(BTContext ctx);

    public void Tick(double delta, BTContext ctx)
    {
        timer += delta;

        while (timer >= Interval)
        {
            timer -= Interval;
            OnTick(ctx);
        }
    }

    public void Reset() => timer = 0.0;
}

