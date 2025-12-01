using Godot;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;


public partial class Pathfinding : Node
{
    private TileMapLayer groundLayer;
    private TileMapLayer obstacleLayer;
    private Vector2I[] groundLayerCoords;
    private Vector2I[] obstacleLayerCoords;

    private enum NeighbourDirections
    {
        TopLeftCorner,
        Top,
        TopRightCorner,
        Right,
        BottomRightCorner,
        Bottom,
        BottomLeftCorner,
        Left
    }
    private static readonly Dictionary<NeighbourDirections, Vector2> NeighbourDirectionsDict = new() {
        {NeighbourDirections.TopLeftCorner, new Vector2(-16,-16)},
        {NeighbourDirections.Top, new Vector2(0,-16)},
        {NeighbourDirections.TopRightCorner, new Vector2(16,-16)},
        {NeighbourDirections.Right, new Vector2(16,0)},
        {NeighbourDirections.BottomRightCorner, new Vector2(16,16)},
        {NeighbourDirections.Bottom, new Vector2(0,16)},
        {NeighbourDirections.BottomLeftCorner, new Vector2(-16,16)},
        {NeighbourDirections.Left, new Vector2(-16,0)}
    };
    
    public struct PathfindingNode
    {
        
        public PathfindingNode(bool walkable, Vector2I position)
        {
            Walkable = walkable;
            Position = position;
        }
        // nodes that the a* will work with
        public int G;
        public int H;
        public int F {get {return G + H;} }
        public bool Walkable;
        public Vector2I Position;
        public bool Visited = false;

    }
    
    public override void _Ready()
    {
        groundLayer = GetNode<TileMapLayer>("../TestLevel/Ground");
        obstacleLayer = GetNode<TileMapLayer>("../TestLevel/Obstacles");
        groundLayerCoords = groundLayer.GetUsedCells().ToArray<Vector2I>();
        obstacleLayerCoords = obstacleLayer.GetUsedCells().ToArray<Vector2I>();

    }
    public static void AStar(Vector2 startPos, Vector2 targetPos)
    {   

        // REMEMBER!!!!!!!!! ALL NODES POSITION ARE IN LOCAL GRID COORDINATES!!!!! 
        // THIS IS TO MAKE DISTANCE CALCULATING FASTER
        // ALL POSITIONS ARE IN Vector2I
        // If you don't have any errors you could intentionally forget and write it down

        // danny ho's implementation of the a* algorithm
        // this function should take the global position of objects

        
    }
    private PathfindingNode? ConstructNodeFromGridPosition(Vector2I gridPos, PathfindingNode currentNode, PathfindingNode targetNode)
    {
        PathfindingNode node;
        // this may become a bottleneck, as Array.Contains is O(n) time
        if (obstacleLayerCoords.Contains(gridPos)) {
            node = new PathfindingNode(false, gridPos);
        }
        else if (groundLayerCoords.Contains(gridPos))
        {
            node = new PathfindingNode(true, gridPos);
            node.G = CalcGCost(currentNode, node);
            node.H = CalcHCost(currentNode, targetNode);
        }
        else
        {
            return null;
        }
        return node;
    }
    private int CalcGCost(PathfindingNode currentNode, PathfindingNode neighbourNode)
    {
        int horizontal_dist = 10;
        int diagonal_dist = 14;
        int dx = Math.Abs(currentNode.Position.X - neighbourNode.Position.X);
        int dy = Math.Abs(currentNode.Position.Y - neighbourNode.Position.Y);

        if (dx == 1 && dy == 1)
        {
            return currentNode.G + diagonal_dist; // this is a diagonal
        }
        else
        {
            return currentNode.G + horizontal_dist; // this is not a diagonal
        }
    }
    private int CalcHCost(PathfindingNode currentNode, PathfindingNode targetNode)
    {
        // heuristic in A*, octile distance
        int horizontal_dist = 10;
        int diagonal_dist = 14;
        int dx = Math.Abs(currentNode.Position.X - targetNode.Position.X);
        int dy = Math.Abs(currentNode.Position.Y - targetNode.Position.Y);

        return horizontal_dist * (dx+dy) - (diagonal_dist - 2 * horizontal_dist) * Math.Min(dx, dy);
    }
    private Vector2 GridCoordsToGlobalPosition(Vector2I gridPos)
    {
        return groundLayer.GetParent<Node2D>().ToGlobal(groundLayer.MapToLocal(gridPos));
    }
    private Vector2I GlobalPositionToGridCoords(Vector2 globalPos)
    {
        return groundLayer.LocalToMap(groundLayer.ToLocal(globalPos));
    }
  //  private Dictionary<Vector2, PathfindingNode> GetTileMapDataAsGrid()
   // {
  //      // prolly wont even use this one
   //     Dictionary<Vector2, PathfindingNode> pathfindingNodes = new Dictionary<Vector2, PathfindingNode>();
        // all this work just to cast an array from the godot array to the csharp array correctly
//        Vector2[] groundLayerCoords = Array.ConvertAll<Vector2I,Vector2>(groundLayer.GetUsedCells().ToArray<Vector2I>(), new Converter<Vector2I, Vector2>(target => (Vector2I)target));
   //     Vector2[] obstacleLayerCoords = Array.ConvertAll<Vector2I,Vector2>(obstacleLayer.GetUsedCells().ToArray<Vector2I>(), new Converter<Vector2I, Vector2>(target => (Vector2I)target));
   //     for (int i = 0; i < groundLayerCoords.Length; i++)
   //     {
   //         // sm about getting global coords of the tiles.
    //       groundLayerCoords[i] = groundLayer.GetParent<Node2D>().ToGlobal(groundLayer.MapToLocal((Vector2I)groundLayerCoords[i]));
    //    }
    //    for (int i = 0; i < obstacleLayerCoords.Length; i++)
    //    {
    //       obstacleLayerCoords[i] = obstacleLayer.GetParent<Node2D>().ToGlobal(obstacleLayer.MapToLocal((Vector2I)obstacleLayerCoords[i]));
    //    }

    //    foreach (Vector2 coords in groundLayerCoords)
    //    {
            
//            PathfindingNode node;
    //        if (obstacleLayerCoords.Contains(coords))
     //       {
     //           node = new PathfindingNode(false, coords);
     //       }
     //       else {
     //           node = new PathfindingNode(true, coords);
     //       }
    //        pathfindingNodes[coords] = node;
     //   }
      //  return pathfindingNodes;
   // }
 
}
