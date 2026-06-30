using Godot;

namespace TakobiAI.Composites;

[Tool, GlobalClass, Icon("uid://b6o47ynleokr8")]
public abstract partial class BTRandomComposite : BTComposite
{
    protected BTNode[] ShuffledChildren { get; private set; }

    public override void _Ready()
    {
        base._Ready(); // important

        if (Engine.IsEditorHint()) return;
        ShuffledChildren = [.. Children];
    }

    protected override void OnEnter(BTContext ctx)
    {
        Shuffle(ShuffledChildren);
    }

    private static void Shuffle(BTNode[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = GD.RandRange(0, i);
            (array[j], array[i]) = (array[i], array[j]);
        }
    }
}

