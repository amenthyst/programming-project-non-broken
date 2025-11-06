using Godot;
using System;
public interface IDamageable

{
    float health { get; set; }
    void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }
    void Die();
}
