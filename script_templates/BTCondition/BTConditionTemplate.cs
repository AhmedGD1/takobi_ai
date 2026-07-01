// meta-name: BT Condition
// meta-description: Creates a Takobi AI condition node.

using Godot;
using System;

namespace TakobiAI.Conditions;

[Tool, GlobalClass]
public partial class _CLASS_ : BTCondition
{
    protected override bool Check(BTContext ctx)
    {
        // Return true if condition passes.
        return true;
    }
}