using Godot;
using System;
using System.Collections.Generic;
using System.Threading;

public partial class Enemy : CharacterBody2D, IDamageable
{
    [Export]
    public float health { get; set; } = 100f;
    [Export] private float Speed = 100f;

    private Vector2[] path;
    private PathfindingAgent pathfindingAgent;
    private IEnumerator<Vector2> pathEnumerator;    
    
    private int i = 0;
    public override void _Ready()
    {
        base._Ready();
        pathfindingAgent = GetNode<PathfindingAgent>("PathfindingAgent");
        pathfindingAgent.groundLayer = GetTree().Root.GetNode<TileMapLayer>("root/TestLevel/Ground");
        pathfindingAgent.obstacleLayer = GetTree().Root.GetNode<TileMapLayer>("root/TestLevel/Obstacles");
        pathfindingAgent.parentPosition = GlobalPosition;
        CallDeferred("DeferredSetup");

    }
    private void DeferredSetup()
    {
        pathfindingAgent.setPath(GlobalPosition, Player.Instance.GlobalPosition);
        pathEnumerator = pathfindingAgent.GetNextPathPosition().GetEnumerator();
        pathEnumerator.MoveNext();
        
    }
    public override void _PhysicsProcess(double delta)
    {
        MoveTestPathfinding();
        pathfindingAgent.parentPosition = GlobalPosition;
    }

    private void Move()
    {
     
        

    }

   
    public void Die()
    {
        QueueFree();
    }

    private void MoveTestPathfinding()
    {
        if (pathEnumerator == null)
        {
            return;
        }
        Vector2 targetPos = pathEnumerator.Current;
        if (GlobalPosition.DistanceTo(targetPos) < 5f)
        {
            if (pathEnumerator.MoveNext())
            {
                targetPos = pathEnumerator.Current;
            }
            
        }
        Vector2 dir = targetPos - GlobalPosition;
        dir = dir.Normalized();
        Velocity = dir * Speed;
        MoveAndSlide();
    }
}
