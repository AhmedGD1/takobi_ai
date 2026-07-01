// meta-name: BT Service
// meta-description: Creates a Takobi AI service node (periodic logic update).

using Godot;
using System;

namespace TakobiAI.Services;

[Tool, GlobalClass]
public partial class _CLASS_ : BTService
{
    protected override void OnTick(BTContext ctx)
    {
        // Runs periodically while tree is active.
    }
}