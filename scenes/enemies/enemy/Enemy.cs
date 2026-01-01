using Godot;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public partial class Enemy : CharacterBody2D, IDamageable
{
	
	public float health { get; set; } = 100f;
	[Export] private float Speed = 100f;
	private float timer = 0f;
	private Vector2[] path;
	private PathfindingAgent pathfindingAgent;
	private IEnumerator<Vector2> currentEnumerator;    
	private IEnumerator<Vector2> pendingEnumerator;
	private bool isCalculatingNewPath = false;
	public override void _Ready()
	{
		base._Ready();
		pathfindingAgent = GetNode<PathfindingAgent>("PathfindingAgent");
		pathfindingAgent.parentPosition = GlobalPosition;
		Player.Instance.onPositionChanged += OnPlayerPositionChanged;


	}
	private void OnPlayerPositionChanged(object sender, Player.SetPathEventArgs e)
	{
		if (isCalculatingNewPath)
		{
			return;
		}
		isCalculatingNewPath = true;
		pathfindingAgent.parentPosition = GlobalPosition;
		pathfindingAgent.setPath(GlobalPosition, e.EndPos);
		pendingEnumerator = pathfindingAgent.GetNextPathPosition().GetEnumerator();
	}
	
	public override void _PhysicsProcess(double delta)
	{
		if (pendingEnumerator != null)
		{
			currentEnumerator = pendingEnumerator;
			isCalculatingNewPath = false;
			pendingEnumerator = null;
			currentEnumerator.MoveNext();
		}
		MoveTestPathfinding();
		
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
		if (currentEnumerator == null)
		{
			return;
		}
		
		Vector2 targetPos = currentEnumerator.Current;
		if (GlobalPosition.DistanceTo(targetPos) < 5f)
		{
			if (currentEnumerator.MoveNext())
			{
				targetPos = currentEnumerator.Current;
			}
			
		}
		Vector2 dir = targetPos - GlobalPosition;
		dir = dir.Normalized();
		Velocity = dir * Speed;
		MoveAndSlide();
	}
}
