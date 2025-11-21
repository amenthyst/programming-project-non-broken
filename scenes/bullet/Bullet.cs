using Godot;
using System;

public partial class Bullet : Area2D

{
    [Export]
    private float Speed = 5f;
    [Export]
    private float Damage = 10f;
    private Vector2 direction;

    public override void _Ready()
    {
        direction = GetGlobalMousePosition() - Position;
        direction = direction.Normalized();
    }
    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = direction * (float)(Speed * delta);
        Position += velocity;
        
    }
    private void _on_body_entered(Node body)
    {
        if (body is IDamageable target)
        {
            target.TakeDamage(Damage);
        }
        QueueFree();
    }
}