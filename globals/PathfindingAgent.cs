using Godot;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Godot.NativeInterop;
using System.Threading.Tasks;




public partial class PathfindingAgent : Node
{
	public TileMapLayer groundLayer;
	public TileMapLayer obstacleLayer;

	private int i = 0;
	


	public Vector2 parentPosition;
	[Export] public float agentRadius;
	public float agentRadiusSquared;
	[Export] public int numberOfPointsInCurve = 5;
	[Export] public float distanceAwayFromPointInCurve = 50;
	[Export] public float controlPointDistanceFromCurve = 30;

	private Level level;
	public Vector2[] path;
	private enum NeighbourDirections
	{
		Top,
		Right,
		Bottom,
		Left
	}
	private static readonly Dictionary<NeighbourDirections, Vector2I> NeighbourDirectionsDict = new() {
		{NeighbourDirections.Top, new Vector2I(0,-1)}, 
		{NeighbourDirections.Right, new Vector2I(1,0)},
		{NeighbourDirections.Bottom, new Vector2I(0,1)},
		{NeighbourDirections.Left, new Vector2I(-1,0)}
	};
	private static readonly Dictionary<NeighbourDirections, NeighbourDirections> oppositeDirectionsDict = new()
	{
		{NeighbourDirections.Top, NeighbourDirections.Bottom},
		{NeighbourDirections.Right, NeighbourDirections.Left},
		{NeighbourDirections.Bottom, NeighbourDirections.Top},
		{NeighbourDirections.Left, NeighbourDirections.Right}
	};
	private Array dirIterable = Enum.GetValues(typeof(NeighbourDirections));
	public class PathfindingNode
	{
		public PathfindingNode(Vector2I position)
		{
			Position = position;
		}
		// nodes that the a* will work with
		public int G;
		public int H;
		public int F {get {return G + H;} }
		public bool Walkable;
		public Vector2I Position;
		public PathfindingNode? parent = null;
		public bool? Visited = null;

	}
	
	public override void _Ready()
	{
		agentRadiusSquared = agentRadius * agentRadius;
		CallDeferred("DeferredSetup");
	}
	private void DeferredSetup()
	{
		level = GetTree().Root.GetNode<Level>("root/TestLevel"); 
	}
	
	
	private Vector2[] TracePath(PathfindingNode current)
	{
		List<Vector2> path = new List<Vector2>();
		while (current != null)
		{
			path.Insert(0, level.GridCoordsToGlobalPosition(current.Position));
			current = current.parent;
		}
		return pathPostProcess(path.ToArray());
	}

	private Vector2[] applyAgentRadius(Vector2[] pathGlobal) 
	{
		List<Vector2> modifiedPath = new();
		foreach (Vector2 v in pathGlobal)
		{
			Vector2 offset = Vector2.Zero;
			foreach (Vector2 vec in level.obstacleGlobalCoords)
			{
				float dist = v.DistanceSquaredTo(vec);
				if (dist < agentRadiusSquared)
				{
					offset = -(vec - v).Normalized() * agentRadius;
				}
			}
			modifiedPath.Add(v + offset);
			
		}
		return modifiedPath.ToArray();
	}
	private Vector2[] LineOfSightPathSmoothing(Vector2[] pathGlobal)
	{
		List<Vector2> newPath = new();
		newPath.Add(pathGlobal[0]);
		int apexIndex = 0;
		int i = 1;
		int j = 0;
		var finalPoint = pathGlobal[^1];

		while (i < pathGlobal.Length && j < 1000)
		{
			Vector2 apex = pathGlobal[apexIndex];
			// origin of raycast
			Vector2 target = pathGlobal[i];
			/* GD.Print("Apex: ", level.GlobalPositionToGridCoords(apex));
			GD.Print("Target: ", level.GlobalPositionToGridCoords(target)); */
			// target of raycast
			bool hitObstacle = level.LiangBarskyEntersRectangle(apex, target);
			if (!hitObstacle)
            {
                i += 1;
				continue;
            }
			else
			{	
				/* GD.Print("Collision found between ", level.GlobalPositionToGridCoords(apex), " and ", level.GlobalPositionToGridCoords(target));
				GD.Print("Adding ",level.GlobalPositionToGridCoords(pathGlobal[i-1])," to path"); */
				apexIndex = i - 1; // new apex at this location
				newPath.Add(pathGlobal[apexIndex]);// new target
				i = apexIndex + 1;
			}
			j += 1;
		}
		if (newPath[^1] != finalPoint)
        {
			// if last item of newpath is not the final point of the original path
            newPath.Add(finalPoint);
        }
		
		return newPath.ToArray();
	}
	private Vector2 SampleBezierPoint(Vector2 p0, Vector2 p1, Vector2 p2, float t)
    {
        Vector2 q0 = p0.Lerp(p1, t);
		Vector2 q1 = p1.Lerp(p2, t);
		Vector2 r = q0.Lerp(q1, t);
		return r;
    } 
	private float DetermineScalarOfDistance(Vector2 dir, Vector2 prevDir)
	{
		dir = dir.Normalized();
		prevDir = prevDir.Normalized();
		float scalarFactorControlPoint = Mathf.Sin((float)prevDir.AngleTo(dir));
		return scalarFactorControlPoint;
	}
	private Vector2[] AddBezierCurving(Vector2[] pathGlobal)
    {
        List<Vector2> newPath = new();
		newPath.Add(pathGlobal[0]);

		for (int i = 1; i < pathGlobal.Length - 1; i++)
		{
			Vector2 dirFromCurrentVectorToPrevious = (pathGlobal[i-1]-pathGlobal[i]).Normalized();
			Vector2 dirFromCurrentVectorToNext = (pathGlobal[i+1]-pathGlobal[i]).Normalized();
			Vector2 p0 = pathGlobal[i] + dirFromCurrentVectorToPrevious * distanceAwayFromPointInCurve; // start point
			Vector2 p2 = pathGlobal[i] + dirFromCurrentVectorToNext * distanceAwayFromPointInCurve; // end point
			Vector2 dir = p2-p0;
			Vector2 perp = dir.Orthogonal().Normalized();
			Vector2 midpoint = (p0 + p2) / 2;
			float scalar = DetermineScalarOfDistance(dir, -dirFromCurrentVectorToPrevious);
			Vector2 p1 = midpoint + perp * scalar * controlPointDistanceFromCurve;
			for (int j = 0; j <= numberOfPointsInCurve; j++)
			{	

				Vector2 sampledPoint = SampleBezierPoint(p0, p1, p2, (float)j / numberOfPointsInCurve);
				
				newPath.Add(sampledPoint);
			}
		}
		newPath.Add(pathGlobal[^1]);
		return newPath.ToArray();
    }
	private Vector2[] pathPostProcess(Vector2[] pathGlobal)
	{
		Vector2[] newPath;
		
		newPath = LineOfSightPathSmoothing(pathGlobal);
		newPath = AddBezierCurving(newPath);
		newPath = applyAgentRadius(newPath);
		return newPath;
	}
	private PathfindingNode ConstructNodeFromGridPosition(Vector2I gridPos, PathfindingNode currentNode, PathfindingNode targetNode)
	{
		PathfindingNode node;
		node = new PathfindingNode(gridPos);
		node.Walkable = (bool)level.levelIsWalkableGrid[gridPos.Y + level.offsetY, gridPos.X + level.offsetX];
		node.G = CalcGCost(currentNode, node);
		node.H = CalcHCost(node, targetNode);
		return node;
	}
	
	private int CalcGCost(PathfindingNode currentNode, PathfindingNode neighbourNode)
	{
		int horizontal_dist = 1;
		return currentNode.G + horizontal_dist; // since there are only 4 cardinal directions, the distance between neighbouring nodes
		// is always 1 (or 10 in this case)
		
	}
	private int CalcHCost(PathfindingNode currentNode, PathfindingNode targetNode)
	{
		// heuristic in A*, Manhattan distance
		int dx = Math.Abs(currentNode.Position.X - targetNode.Position.X);
		int dy = Math.Abs(currentNode.Position.Y - targetNode.Position.Y);
		return dx + dy;

	}

	public Vector2[] AStar(Vector2 startPos, Vector2 targetPos)
	{   
		int startPosIslandID = 0;
		int endPosIslandID = 0;
		for (int i = 0; i < level.islands.Length; i++)
		{
			if (level.islands[i].Contains(level.GlobalPositionToGridCoords(startPos)))
			{
				startPosIslandID = i;
			}
			else if (level.islands[i].Contains(level.GlobalPositionToGridCoords(targetPos)))
			{
				endPosIslandID = i;
			}
		}
		if (startPosIslandID != endPosIslandID)
		{
			Vector2[] returnArray = {startPos};
			GD.Print("no path found soz");
			return returnArray;
		}
		// REMEMBER!!!!!!!!! ALL NODES POSITION ARE IN LOCAL GRID COORDINATES!!!!! 
		// THIS IS TO MAKE DISTANCE CALCULATING FASTER
		// ALL POSITIONS ARE IN Vector2I
		// If you don't have any errors you could intentionally forget and write it down
		// danny ho's implementation of the a* algorithm
		// start and end position should be in global coordinates
		var watch = Stopwatch.StartNew();
		List<PathfindingNode> openSet = new();
		Dictionary<Vector2I, PathfindingNode> nodes = new();

		List<PathfindingNode> neighbours = new();

		int nodesExpanded = 0;
		int horizontal_dist = 1;
		int arrayHeight = level.levelIsWalkableGrid.GetLength(0);
		int arrayWidth = level.levelIsWalkableGrid.GetLength(1);
		
		PathfindingNode startNode = new PathfindingNode(level.GlobalPositionToGridCoords(startPos));
		PathfindingNode endNode = new PathfindingNode(level.GlobalPositionToGridCoords(targetPos));
		
		
		startNode.G = 0; // distance from start node to start node is 0 (shockingly)
		startNode.H = CalcHCost(startNode, endNode);
		startNode.Walkable = true;
		openSet.Add(startNode);
		nodes[startNode.Position] = startNode;

		Vector2I posOffset;
		Vector2I neighbourPos;
		PathfindingNode currentNode;
		PathfindingNode neighbour;
		
		while (openSet.Count != 0)
		{
			currentNode = openSet[0];

			foreach (var v in openSet)
			{
				v.Visited = false;
				if (v.F < currentNode.F)
				{
					currentNode = v;
				}
			}
			openSet.Remove(currentNode);
			if (currentNode.Position == endNode.Position)
			{
			/* 	watch.Stop();
				long elapsedTicks = watch.ElapsedTicks; */
				/* GD.Print("Time taken to execute pathfinding before post-processing: ", Mathf.Round((double)elapsedTicks / Stopwatch.Frequency * 1000000), " microseconds");
				
				GD.Print("Nodes expanded: ", nodesExpanded); */
				
/* 				var watch2 = Stopwatch.StartNew(); */
				Vector2[] finalArray = TracePath(currentNode); 
/* 				watch2.Stop();
				elapsedTicks = watch2.ElapsedTicks;
				GD.Print("Time taken to execute pathfinding after post-processing: ", Mathf.Round((double)elapsedTicks / Stopwatch.Frequency * 1000000), " microseconds"); */
				/* 	GD.Print("Total time taken to execute pathfinding? ", Mathf.Round((double)elapsedTicks / Stopwatch.Frequency * 1000000), " microseconds"); */
			/* 	long elapsedTicks = watch.ElapsedTicks; */
			
				return finalArray;
			}
			currentNode.Visited = true;
			neighbours.Clear();
			foreach (NeighbourDirections dir in dirIterable)
			{
				
				posOffset = NeighbourDirectionsDict[dir];
				
				neighbourPos = currentNode.Position + posOffset;
				
				int neighbourPositionInGridX = neighbourPos.X + level.offsetX;
				int neighbourPositionInGridY = neighbourPos.Y + level.offsetY;
				bool isInTileMap = (neighbourPositionInGridX >= 0 && neighbourPositionInGridX <= arrayWidth - 1) &&
								   (neighbourPositionInGridY >= 0 && neighbourPositionInGridY <= arrayHeight - 1) &&
								   (level.levelIsWalkableGrid[neighbourPositionInGridY, neighbourPositionInGridX] != null);
								   
				if (!isInTileMap)
				{
					continue;
					// if there is no tile at that coordinate skip
				}
				if (nodes.ContainsKey(neighbourPos))
				{
					neighbour = nodes[neighbourPos]; // node already exists in the openSet
				}
				else
				{
					neighbour = ConstructNodeFromGridPosition(neighbourPos, currentNode, endNode); // node doesn't exist
					nodes[neighbourPos] = neighbour;
					nodesExpanded++;
				}
				neighbours.Add(neighbour);
				// get neighbours in 8 directions of currentNode
			}
			foreach (PathfindingNode neighbourLocal in neighbours)
			{   
				
				if (!neighbourLocal.Walkable || neighbourLocal.Visited == true)
				{
					continue;
				}
				
				int tentativeG = currentNode.G + horizontal_dist;
				if (tentativeG <= neighbourLocal.G)
				{
					// the path from the currentNode to the neighbour is a better path than the previous path
					// to the neighbour, as the cost from this node to the neighbour is lower
			 
					neighbourLocal.G = tentativeG;
					neighbourLocal.parent = currentNode;
					if (neighbourLocal.Visited == null)
					{
						openSet.Add(neighbourLocal);
					}
				}
			}
		}
		GD.Print("Nodes expanded: ", nodesExpanded);
		throw new Exception("no path found");
	}

	public void setPath(Vector2 startPos, Vector2 endPos)
	{
		path = AStar(startPos, endPos);
		level.pathDisplay.DrawPath(path);
	}
	public Vector2[] getPath()
	{
		return path;
	}
	
	public IEnumerable<Vector2> GetNextPathPosition()
	{
		if (path == null)
		{
			GD.Print("Path null");
		}
		foreach (Vector2 v in path)
		{
			if (level.GlobalPositionToGridCoords(parentPosition) == level.GlobalPositionToGridCoords(v))
			{
				continue;
			}
			yield return v;
		}
	}
	private void printNodeData(PathfindingNode node)
	{
		GD.Print("------NODE DATA AT ", node.Position, " ------");
		GD.Print("G score: ", node.G);
		GD.Print("H score: ", node.H);
		GD.Print("F score: ", node.F);
		GD.Print("Walkable: ", node.Walkable);
		if (node.parent != null)
		{
			GD.Print("Parent Position: ", node.parent.Position);
		} 
	}
}
