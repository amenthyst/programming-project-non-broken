using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Level : Node2D
{
    private TileMapLayer groundLayer;
    private TileMapLayer obstacleLayer;
    public HashSet<Vector2I> groundLayerCoords;
    public HashSet<Vector2I> obstacleLayerCoords;
    public bool?[,] levelIsWalkableGrid;
    public int offsetX;
    public int offsetY;
    public override void _Ready()
    {
        // can override ready as there may be layers other than ground and obstacles
        // but for pathfinding purposes all of that must be condensed into 2 layers.
        groundLayer = GetNode<TileMapLayer>("Ground");
        groundLayerCoords = groundLayer.GetUsedCells().ToHashSet<Vector2I>();
        obstacleLayer = GetNode<TileMapLayer>("Obstacles");
        obstacleLayerCoords = obstacleLayer.GetUsedCells().ToHashSet<Vector2I>();
        levelIsWalkableGrid = GetTileMapWalkableDataAsGrid();
    }
    public bool?[,] GetTileMapWalkableDataAsGrid()
    {
        // finding out how big the grid is
        var sortedByX = groundLayerCoords.OrderByDescending(v => v.X).ToArray();
		int maxX = sortedByX[0].X;
		int minX = sortedByX[^1].X;
		var sortedByY = groundLayerCoords.OrderByDescending(v => v.Y).ToArray();
		int maxY = sortedByY[0].Y;
		int minY = sortedByY[^1].Y;

        // allows for grid to have negative coordinates
        offsetX = -minX;
        offsetY = -minY;
        bool?[,] grid = new bool?[maxY - minY + 1, maxX - minX + 1];

        foreach (Vector2I v in groundLayerCoords)
        {
            if (obstacleLayerCoords.Contains(v))
            {
                grid[v.Y + offsetY, v.X + offsetX] = false;
            }
            else
            {
                grid[v.Y + offsetY, v.X + offsetX] = true;
            }
        }
        return grid;
    }
    public Vector2 GridCoordsToGlobalPosition(Vector2I gridPos)
    {
        return ToGlobal(groundLayer.MapToLocal(gridPos));
    }
    public Vector2I GlobalPositionToGridCoords(Vector2 globalPos)
	{
		return groundLayer.LocalToMap(groundLayer.ToLocal(globalPos));
	}
}
