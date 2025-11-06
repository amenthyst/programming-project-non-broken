using Godot;

public interface IAttack
{
    float Damage { get; set; }
    void Attack(Area2D area)
    {
        if (area is IDamageable target)
        {
            target.TakeDamage(Damage);
        }
    }
}
