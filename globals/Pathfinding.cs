using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Pathfinding : Node
{
    private TileMapLayer groundLayer;
    private TileMapLayer obstacleLayer;

    public struct PathfindingNode
    {
        
        public PathfindingNode(bool walkable, Vector2 coordinates)
        {
            Walkable = walkable;
            Coordinates = coordinates;
        }
        // nodes that the a* will work with
        int G;
        int H;
        int F {get {return G + H;} }
        public bool Walkable;
        Vector2 Coordinates;
    }
    private Dictionary<Vector2I, PathfindingNode> pathfindingNodes;
    public override void _Ready()
    {
        groundLayer = GetNode<TileMapLayer>("../TestLevel/Ground");
        obstacleLayer = GetNode<TileMapLayer>("../TestLevel/Obstacles");
        pathfindingNodes = GetTileMapDataAsGrid();
        printTileMapData();

    }

    public override void _Process(double delta)
    {
        
    }
    public static void AStar(Vector2 startPos, Vector2 targetPos)
    {
        // danny ho's implementation of the a* algorithm
        // this function should take the global position of objects

        

    }
    private Dictionary<Vector2I, PathfindingNode> GetTileMapDataAsGrid()
    {
        Dictionary<Vector2I, PathfindingNode> pathfindingNodes = new Dictionary<Vector2I, PathfindingNode>();
        Godot.Collections.Array<Vector2I> groundLayerCoords = groundLayer.GetUsedCells();
        Godot.Collections.Array<Vector2I> obstacleLayerCoords = obstacleLayer.GetUsedCells();
        foreach (Vector2I coords in groundLayerCoords)
        {
            PathfindingNode node;
            if (obstacleLayerCoords.Contains(coords))
            {
                node = new PathfindingNode(false, coords);
            }
            else {
                node = new PathfindingNode(true, coords);
            }
            pathfindingNodes[coords] = node;
        }
        return pathfindingNodes;
    }
    private void printTileMapData()
    {
        string[,] tileMapDataArray = new string[8,8];
        string arrayString = "";
        foreach (Vector2I coords in pathfindingNodes.Keys)
        {
            GD.Print(coords);
            if (!pathfindingNodes[coords].Walkable)
            {
                tileMapDataArray[coords.Y, coords.X] = " # ";
            }
            else
            {
                tileMapDataArray[coords.Y, coords.X] = " . ";
            }
        }
        for (int k = 0; k < tileMapDataArray.GetLength(0); k++) {
            for (int l = 0; l < tileMapDataArray.GetLength(1); l++) {
                arrayString += tileMapDataArray[k,l];
            }
            arrayString += '\n';
        }
        GD.Print(arrayString);
    }
}
