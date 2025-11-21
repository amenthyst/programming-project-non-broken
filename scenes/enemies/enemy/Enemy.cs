using Godot;
using System;

public partial class Enemy : CharacterBody2D, IDamageable
{
    [Export]
    public float health { get; set; } = 100f;
    [Export] private float Speed = 100f;

    private NavigationAgent2D navigationAgent;

    public Vector2 movementTarget
    {
        get {return movementTarget;}
        set {navigationAgent.TargetPosition = value;}
    }
    
    public override void _Ready()
    {
        base._Ready();
        navigationAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");
        
        CallDeferred("SetMovementPosition");
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        Move();
    }

    private void Move()
    {
        if (navigationAgent.IsNavigationFinished())
        {
            return;
        }
        Vector2 currentPos = GlobalTransform.Origin;
        Vector2 nextPathPosition = navigationAgent.GetNextPathPosition();
        Velocity = currentPos.DirectionTo(nextPathPosition) * Speed;
        MoveAndSlide();
        CallDeferred("SetMovementPosition");
    }

   
    public void Die()
    {
        QueueFree();
    }
    private async void SetMovementPosition()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        movementTarget = Player.Instance.GlobalPosition;
    }
}
