using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class Player : CharacterBody2D
{
    [Export]
    private int Speed = 40;
    [Export]
    private PackedScene bullet;
    [Export]
    private double shootCooldown = 0.5f;
    private double shootTimer = 0f;
    private Marker2D marker;
    private Vector2 moveVec;


    public override void _Ready()
    {
        marker = GetNode<Marker2D>("Marker2D");
    }

    public override void _PhysicsProcess(double delta)
    {
        moveVec = Input.GetVector("Left", "Right", "Up", "Down");
        Velocity = moveVec * (float)(Speed * delta * 1000);
        MoveAndSlide();
    }
    public override void _Process(double delta)
    {
        if (Input.IsActionPressed("Shoot"))
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
        Bullet b = bullet.Instantiate<Bullet>();
        b.Position = marker.GlobalPosition;
        GetParent().AddChild(b);
    }
    private void Rotate()
    {
        Vector2 mousePos = GetGlobalMousePosition();
        LookAt(mousePos);
    }
    
}