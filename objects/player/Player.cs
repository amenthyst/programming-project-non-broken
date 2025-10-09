using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class Player : CharacterBody2D, IDamageable
{
    [Export]
    private int speed = 40;
    [Export] private PackedScene bullet;
    [Export] private PackedScene meleeSwing;
    [Export] private float health = 100f;
    private double shootCooldown = 0.5f;
    private double shootTimer = 0f;
    private Marker2D marker;
    private Area2D noMouseArea;
    private Vector2 moveVec;

    public static Player Instance { get; private set; } // singleton pattern
    private bool canAttack = true;
    
    public override void _Ready()
    {
        if (Instance != null)
        {
            throw new Exception("More than one player instance!!!!!!!! FIX your code.");
        }
        Instance = this;
        marker = GetNode<Marker2D>("Marker2D");
        noMouseArea = GetNode<Area2D>("NoMouseArea");
    }

    public override void _PhysicsProcess(double delta)
    {
        moveVec = Input.GetVector("Left", "Right", "Up", "Down");
        Velocity = moveVec * (float)(speed * delta * 1000);
        MoveAndSlide();
    }
    public override void _Input(InputEvent @event)
    {   if (canAttack)
        {
            if (Input.IsActionJustPressed("MeleeAttack"))
            {
                MeleeAttack();
            }
        }
    }
    public override void _Process(double delta)
    {   // for continuous input events and continuous functions, as continuous input events 
        // e.g. shooting do not work in _Input, as _Input is called by input event.
        // sorry if this is bad practice but it works anyway and all the input should be recieved
        // here anyway in Player.cs...
        if (Input.IsActionPressed("Shoot") && canAttack)
        {
            shootTimer += delta;
            if (shootTimer >= shootCooldown)
            {
                Shoot();
                shootTimer = 0f;
            }
        }
        Rotate();
    }

    private void Shoot()
    {
        if (bullet is null)
        {
            throw new Exception("Bullet scene is empty!");
        }
        Bullet b = bullet.Instantiate<Bullet>();
        b.Position = marker.GlobalPosition;
        GetParent().AddChild(b);
    }
    private void Rotate()
    {
        Vector2 mousePos = GetGlobalMousePosition();
        LookAt(mousePos);
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }
    public void Die()
    {
        QueueFree();
    }
    private void MeleeAttack()
    {
        if (meleeSwing is null)
        {
            throw new Exception("Melee swing scene is empty!");
        }
        MeleeSwing m = meleeSwing.Instantiate<MeleeSwing>();
        m.Position = marker.Position;
        AddChild(m);
    }

    // these subroutines make it so no bullets/swings can fire if the mouse is in the no mouse zone,
    // as it does not make sense to aim inward towards the player
    private void _on_no_mouse_area_mouse_entered()
    {
        canAttack = false;
    }
    private void _on_no_mouse_area_mouse_exited()
    {
        canAttack = true;
    }
}