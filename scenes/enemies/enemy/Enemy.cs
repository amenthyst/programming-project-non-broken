using Godot;
using System;

public partial class Enemy : Area2D, IDamageable
{
    [Export]
    public float health { get; set; } = 100f;

    public void Die()
    {
        QueueFree();
    }
}
