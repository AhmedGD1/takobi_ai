using Godot;

namespace TakobiAI.Demo;

public partial class Player : CharacterBody2D
{
    [Export] private float speed = 100;
    [Export] private float damage = 30;

    [Export] private Enemy enemy;

    public override void _PhysicsProcess(double delta)
    {
        Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        Velocity = direction * speed;

        MoveAndSlide();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey { Pressed: true, Keycode: Key.Space })
            enemy.ReceiveDamage(damage);
    }


}
