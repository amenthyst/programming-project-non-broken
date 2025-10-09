using Godot;
using System;

public partial class Enemy : Area2D, IDamageable
{
    [Export]
    private float health = 100;

    
    public void TakeDamage(float damage)
    {
        health -= damage;
        GD.Print($"Damaged by {damage}");
        GD.Print($"Remaining health: {health}");
        if (health <= 0)
        {
            Die();
        }
    }
    public void Die()
    {
        QueueFree();
    }
}
