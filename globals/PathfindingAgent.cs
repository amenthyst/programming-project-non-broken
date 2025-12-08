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


public partial class PathfindingAgent : Node
{
    public TileMapLayer groundLayer;
    public TileMapLayer obstacleLayer;
    private HashSet<Vector2I> groundLayerCoords;
    private HashSet<Vector2I> obstacleLayerCoords;
    private int i = 0;

    public Vector2 parentPosition;
    [Export] public int agentRadius;

    public Vector2[] path;
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
    private static readonly Dictionary<NeighbourDirections, Vector2I> NeighbourDirectionsDict = new() {
        {NeighbourDirections.TopLeftCorner, new Vector2I(-1,-1)},
        {NeighbourDirections.Top, new Vector2I(0,-1)},
        {NeighbourDirections.TopRightCorner, new Vector2I(1,-1)},
        {NeighbourDirections.Right, new Vector2I(1,0)},
        {NeighbourDirections.BottomRightCorner, new Vector2I(1,1)},
        {NeighbourDirections.Bottom, new Vector2I(0,1)},
        {NeighbourDirections.BottomLeftCorner, new Vector2I(-1,1)},
        {NeighbourDirections.Left, new Vector2I(-1,0)}
    };
    private static readonly Dictionary<NeighbourDirections, NeighbourDirections> oppositeDirectionsDict = new()
    {
        {NeighbourDirections.TopLeftCorner, NeighbourDirections.BottomRightCorner},
        {NeighbourDirections.Top, NeighbourDirections.Bottom},
        {NeighbourDirections.TopRightCorner, NeighbourDirections.BottomLeftCorner},
        {NeighbourDirections.Right, NeighbourDirections.Left},
        {NeighbourDirections.BottomRightCorner, NeighbourDirections.TopLeftCorner},
        {NeighbourDirections.Bottom, NeighbourDirections.Top},
        {NeighbourDirections.BottomLeftCorner, NeighbourDirections.TopRightCorner},
        {NeighbourDirections.Left, NeighbourDirections.Right}
    };
    
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

    }
    
    public override void _Ready()
    {
        
        CallDeferred("DeferredSetup");
    }
    private void DeferredSetup()
    {
        // make sure the groundLayers and obstacleLayers are initialized
        groundLayerCoords = groundLayer.GetUsedCells().ToHashSet<Vector2I>();
        obstacleLayerCoords = obstacleLayer.GetUsedCells().ToHashSet<Vector2I>();
    }

    private Vector2[] TracePath(PathfindingNode current)
    {
        List<Vector2> path = new List<Vector2>();
        while (current != null)
        {
            path.Insert(0, GridCoordsToGlobalPosition(current.Position));
            current = current.parent;
        }
        Vector2[] pathWithAgentRadius = applyAgentRadius(path.ToArray());
        return pathWithAgentRadius;
    }

    private Vector2[] applyAgentRadius(Vector2[] pathGlobal) 
    {
        List<Vector2> modifiedPath = new();
        foreach (Vector2 v in pathGlobal)
        {
            Vector2I localCoord = GlobalPositionToGridCoords(v);
            Vector2I offset = new();
            foreach (NeighbourDirections dir in Enum.GetValues(typeof(NeighbourDirections)))
            {
                Vector2I potentialObstaclePos = localCoord + NeighbourDirectionsDict[dir];
                if (obstacleLayerCoords.Contains(potentialObstaclePos))
                {
                    offset += NeighbourDirectionsDict[oppositeDirectionsDict[dir]];
                }
            }
            Vector2 newVec = v + GridCoordsToGlobalPosition(offset * agentRadius);
            modifiedPath.Add(newVec);
        }
        return modifiedPath.ToArray();
    }
    private PathfindingNode ConstructNodeFromGridPosition(Vector2I gridPos, PathfindingNode currentNode, PathfindingNode targetNode)
    {
        PathfindingNode node;
        node = new PathfindingNode(gridPos);
        node.G = CalcGCost(currentNode, node);
        node.H = CalcHCost(node, targetNode);
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
    private int Dist(PathfindingNode x, PathfindingNode y)
    {
        // calculates distance between 2 nodes
        int horizontal_dist = 10;
        int diagonal_dist = 14;
        int dx = Math.Abs(x.Position.X - y.Position.X);
        int dy = Math.Abs(x.Position.Y - y.Position.Y);
        if (dx == 1 && dy == 1)
        {
            return diagonal_dist;
        }
        else
        {
            return horizontal_dist;
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
    /* private Dictionary<Vector2, PathfindingNode> GetTileMapDataAsGrid()
    {
       // prolly wont even use this one
        Dictionary<Vector2, PathfindingNode> pathfindingNodes = new Dictionary<Vector2, PathfindingNode>();
        // all this work just to cast an array from the godot array to the csharp array correctly
        Vector2I[] groundLayerCoords = groundLayer.GetUsedCells().ToArray<Vector2I>();
        Vector2I[] obstacleLayerCoords = obstacleLayer.GetUsedCells().ToArray<Vector2I>();
        

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
        //for (int i = 0; i < groundLayerCoords.Length; i++)
        //{
           // sm about getting global coords of the tiles.
        //   groundLayerCoords[i] = groundLayer.GetParent<Node2D>().ToGlobal(groundLayer.MapToLocal((Vector2I)groundLayerCoords[i]));
        //}
        //for (int i = 0; i < obstacleLayerCoords.Length; i++)
        //{
        //    obstacleLayerCoords[i] = obstacleLayer.GetParent<Node2D>().ToGlobal(obstacleLayer.MapToLocal((Vector2I)obstacleLayerCoords[i]));
        //} */
    //}
    private bool checkIfWalkable(Vector2I nodePos, Vector2I previousNodePos, PathfindingNode endNode)
    {
        // goes through all the checks to see if a node is walkable
        bool diagonalMove;
        if (obstacleLayerCoords.Contains(nodePos) && nodePos != endNode.Position)
        {
            return false;
        }
        Vector2I delta = nodePos - previousNodePos;
        if (nodePos == endNode.Position)
        {
            GD.Print(delta);
        }
        diagonalMove = Math.Abs(delta.X) == 1 && Math.Abs(delta.Y) == 1;
        if (diagonalMove) {
    
            Vector2I sideA = new Vector2I(previousNodePos.X + delta.X, previousNodePos.Y);
            Vector2I sideB = new Vector2I(previousNodePos.X, previousNodePos.Y + delta.Y);
            if (obstacleLayerCoords.Contains(sideA) && obstacleLayerCoords.Contains(sideB))
            {
                return false;
            }
        }
        return true;
    }
    public Vector2[] AStar(Vector2 startPos, Vector2 targetPos)
    {   
        // REMEMBER!!!!!!!!! ALL NODES POSITION ARE IN LOCAL GRID COORDINATES!!!!! 
        // THIS IS TO MAKE DISTANCE CALCULATING FASTER
        // ALL POSITIONS ARE IN Vector2I
        // If you don't have any errors you could intentionally forget and write it down
        // danny ho's implementation of the a* algorithm
        // start and end position should be in global coordinates
        var watch = Stopwatch.StartNew();
        int nodesExpanded = 0;
        PriorityQueue<PathfindingNode, (int, int)> openSet = new();
        Dictionary<Vector2I, PathfindingNode> nodes = new();
        HashSet<Vector2I> openSetLookup = new();
        HashSet<Vector2I> closedSet = new(); 

        PathfindingNode startNode = new PathfindingNode(GlobalPositionToGridCoords(startPos));
        PathfindingNode endNode = new PathfindingNode(GlobalPositionToGridCoords(targetPos));

        startNode.G = 0; // distance from start node to start node is 0 (shockingly)
        startNode.H = CalcHCost(startNode, endNode);
        startNode.Walkable = true;
        openSet.Enqueue(startNode, (startNode.F, startNode.H));
        openSetLookup.Add(startNode.Position);
        nodes[startNode.Position] = startNode;

        while (openSet.Count != 0)
        {
            i++;
            PathfindingNode currentNode = openSet.Dequeue();
            if (currentNode.Position == endNode.Position)
            {
                GD.Print("Path found"); 
                GD.Print("Nodes expanded: ", nodesExpanded);
                foreach (var v in TracePath(currentNode))
                {
                    GD.Print("--> ", GlobalPositionToGridCoords(v));
                } 
                Vector2[] finalArray = TracePath(currentNode);
                watch.Stop();
                var elapsedSeconds = watch.ElapsedMilliseconds;
                GD.Print("Time taken to execute pathfinding: ", elapsedSeconds, " ms");
                return finalArray;
            }
            closedSet.Add(currentNode.Position);

            /* GD.Print("Size of Open set:", openSetLookup.Count);
            foreach (Vector2I v in openSetLookup)
            {
                printNodeData(nodes[v]);
            }
            GD.Print("----------");
            GD.Print("Size of Closed set:", closedSet.Count);
            foreach (Vector2I v in closedSet)
            {
                printNodeData(nodes[v]);
            } */

            List<PathfindingNode> neighbours = new();
            foreach (NeighbourDirections dir in Enum.GetValues(typeof(NeighbourDirections)))
            {
                PathfindingNode neighbour;
                Vector2I posOffset = NeighbourDirectionsDict[dir];
                Vector2I neighbourPos = currentNode.Position + posOffset;
                if (!groundLayerCoords.Contains(neighbourPos))
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
            foreach (PathfindingNode neighbour in neighbours)
            {   
                
                neighbour.Walkable = checkIfWalkable(neighbour.Position, currentNode.Position, endNode);

                if (!neighbour.Walkable || closedSet.Contains(neighbour.Position))
                {
                    continue;
                }
                int tentativeG = currentNode.G + Dist(currentNode, neighbour);
                if (tentativeG <= neighbour.G || neighbour.Position == endNode.Position)
                {
                    // the path from the currentNode to the neighbour is a better path than the previous path
                    // to the neighbour, as the cost from this node to the neighbour is lower
             
                    neighbour.G = tentativeG;
                    neighbour.parent = currentNode;
                    if (!openSetLookup.Contains(neighbour.Position))
                    {
                        openSet.Enqueue(neighbour, (neighbour.F, neighbour.H));
                        openSetLookup.Add(neighbour.Position);
                    }
                }
            }
        }
        GD.Print("Nodes Expanded: ", nodesExpanded);
        throw new Exception("no path found");
    }

    public void setPath(Vector2 startPos, Vector2 endPos)
    {
        path = AStar(startPos, endPos);
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

