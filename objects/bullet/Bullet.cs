using Godot;
using System;

public partial class Bullet : CharacterBody2D

{
    [Export]
    public float Speed = 5f;
    private Vector2 direction;

    public override void _Ready()
    {
        direction = GetGlobalMousePosition() - Position;
        direction = direction.Normalized();
    }
    public override void _PhysicsProcess(double delta)
    {
        Velocity = direction * (float)(Speed * delta);
        MoveAndCollide(Velocity);
        
    }
    
}