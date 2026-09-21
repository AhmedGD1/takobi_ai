using Godot;

namespace TakobiAI.Decorators;

/// <summary>
/// A reusable elapsed-time gate. Wraps the "start a clock, check if a duration
/// has passed" pattern shared by Timeout, Delayer, and Cooldown so timing
/// behavior (and any future pause/timescale support) lives in one place.
/// </summary>
public struct TimedGate
{
    private ulong startTicksMsec;

    public void Start() => startTicksMsec = Time.GetTicksMsec();

    public readonly double Elapsed => (Time.GetTicksMsec() - startTicksMsec) / 1000.0;

    public readonly bool HasElapsed(double duration) => Elapsed >= duration;
}
