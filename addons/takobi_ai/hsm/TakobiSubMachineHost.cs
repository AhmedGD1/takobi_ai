using System;
using Godot;

namespace TakobiAI.HSM;

/// <summary>
/// What a <see cref="TakobiState{T}"/> needs from a sub-machine it hosts, independent of the sub-machine's
/// state enum. The state calls these at fixed points, so the order never depends on how it was configured.
/// </summary>
internal interface ISubMachine
{
    /// <summary>The parent state was entered. Runs after the parent's own Enter callbacks.</summary>
    void ParentEntered();

    /// <summary>The parent state is being exited. Runs before the parent's own Exit callbacks.</summary>
    void ParentExited();

    /// <summary>The parent state is updating. Runs after the parent's own update callbacks.</summary>
    void Update(double delta);

    /// <summary>Offers an event from the parent. Returns <see langword="true"/> if it fired a transition and was consumed.</summary>
    bool TryHandleEvent(StringName eventName);
}

/// <summary>
/// Connects a <see cref="TakobiHSM{T}"/> to the parent state that hosts it, created by
/// <see cref="TakobiState{T}.WithSubMachine"/>.
/// </summary>
/// <typeparam name="TChild">The enum type of the sub-machine's states.</typeparam>
/// <param name="machine">The hosted sub-machine.</param>
/// <param name="restartOnEnter">
/// <see langword="true"/>: restart the sub-machine on every parent entry and disable it on exit, so its current
/// state's Exit runs when the parent leaves. <see langword="false"/>: pause it on exit and resume it on entry,
/// keeping its state and running no Exit or Enter.
/// </param>
internal sealed class SubMachineHost<TChild>(TakobiHSM<TChild> machine, bool restartOnEnter) : ISubMachine where TChild : Enum
{
    public void ParentEntered()
    {
        if (restartOnEnter)
            machine.Restart();

        machine.Paused = false;
    }

    public void ParentExited()
    {
        if (restartOnEnter)
            machine.Disable();
        else
            machine.Paused = true;
    }

    public void Update(double delta) => machine.Update(delta);

    public bool TryHandleEvent(StringName eventName) => machine.HandleEventFromParent(eventName);
}
