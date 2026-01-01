using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


public partial class Level : Node2D
{
    [Export] private float padding = 10;
    private struct Window
    {
        public float xmin;
        public float xmax;
        public float ymin;
        public float ymax;
    }
    public Vector2[] pathToDraw;
    private TileMapLayer groundLayer;
    private TileMapLayer obstacleLayer;
    public PathDisplay pathDisplay;
    public HashSet<Vector2I> groundLayerCoords;
    public HashSet<Vector2I> obstacleLayerCoords;
    public HashSet<Vector2> obstacleGlobalCoords = new();
    public bool?[,] levelIsWalkableGrid;
    public int offsetX;
    public int offsetY;
    public HashSet<Vector2I>[] islands;
    private Window[] windows;
    public override void _Ready()
    {
        // can override ready as there may be layers other than ground and obstacleIslands
        // but for pathfinding purposes all of that must be condensed into 2 layers.
        groundLayer = GetNode<TileMapLayer>("Ground");
        groundLayerCoords = groundLayer.GetUsedCells().ToHashSet<Vector2I>();
        obstacleLayer = GetNode<TileMapLayer>("Obstacles");
        obstacleLayerCoords = obstacleLayer.GetUsedCells().ToHashSet<Vector2I>();
        foreach (var v in obstacleLayerCoords)
        {
            obstacleGlobalCoords.Add(GridCoordsToGlobalPosition(v));
        }
        levelIsWalkableGrid = GetTileMapWalkableDataAsGrid();
        pathDisplay = GetNode<PathDisplay>("PathDisplay");
        islands = ParseGridIntoIslands(false);
        windows = ParseGridIntoObstacles();
        
    }
    private bool?[,] GetTileMapWalkableDataAsGrid()
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
    private HashSet<Vector2I>[] ParseGridIntoIslands(bool searchingForObstacles)
    {
        bool target = true;
        if (searchingForObstacles)
        {
            target = false;
        }
        int rows = levelIsWalkableGrid.GetLength(0);

        int cols = levelIsWalkableGrid.GetLength(1);
        bool?[,] gridLocal = new bool?[rows, cols];
        Array.Copy(levelIsWalkableGrid, gridLocal, levelIsWalkableGrid.Length);
        Vector2I coord;
        List<HashSet<Vector2I>> islands = new();

        // now it is by value, so i can make changes to grid local without messing up the other grid the pathfinding
        // actually looks at
        List<Vector2I> dirs = new List<Vector2I>();
        if (!searchingForObstacles)
        {
            Vector2I[] allDirs = {new Vector2I(0,1), new Vector2I(1,0), new Vector2I(0,-1), new Vector2I(-1,0)};
            dirs.AddRange(allDirs);
        }
        else
        {
            Vector2I[] horizontalDirs = {new Vector2I(1,0), new Vector2I(-1,0)};
            dirs.AddRange(horizontalDirs);
        }
        
        void depthFirstSearch(Vector2I coord, HashSet<Vector2I> hashset)
        {

            hashset.Add(coord);
            gridLocal[coord.Y, coord.X] = !target;
             
            foreach (Vector2I dir in dirs)
            {
                Vector2I offsetted = coord + dir;
                bool valid = (offsetted.X >= 0 && offsetted.X < cols) && (offsetted.Y >= 0 && offsetted.Y < rows) &&
                              (gridLocal[offsetted.Y, offsetted.X] == target);
                if (valid)
                {
                    depthFirstSearch(offsetted, hashset);
                }
            }
        }
        
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (gridLocal[row, col] == target)
                {
                    HashSet<Vector2I> hashset = new();
                    coord = new Vector2I(col, row);
                    depthFirstSearch(coord, hashset);
                    islands.Add(hashset);

                }
            }
        }
        
        return islands.ToArray();
    }
    private void PrintIslandsInfo()
    {
        for (int i = 0; i < islands.Length; i++)
        {
            var currentHashSet = islands[i];
            GD.Print("CURRENT ISLAND ID: ", i);
            foreach (var v in currentHashSet)
            {
                GD.Print(v);
            }
        }
    }
    private Window[] ParseGridIntoObstacles()
    {
        Vector2 scale = Transform.Scale;
        HashSet<Vector2I>[] obstacleCoords = ParseGridIntoIslands(true);
        List<Window> windows = new();
        List<Vector2I[]> obstacleIslands = new();
        foreach (var v in obstacleCoords)
        {
            obstacleIslands.Add(v.ToArray());
        }
        foreach (var island in obstacleIslands)
        {
            Window window = new();
            Vector2I startCoord = island[0];
            Vector2I endCoord = island[^1];

            var offset = 16 * scale.X;
            TileData startTileData = obstacleLayer.GetCellTileData(startCoord);
            Vector2 globalStartCoord = GridCoordsToGlobalPosition(startCoord);
            Vector2 globalEndCoord = GridCoordsToGlobalPosition(endCoord);
            window.xmin = globalStartCoord.X - offset - padding;
            window.xmax = globalEndCoord.X + offset + padding;
            window.ymin = globalStartCoord.Y - offset - padding;
            window.ymax = globalEndCoord.Y + offset + padding;
            windows.Add(window);
        }
        return windows.ToArray();
    }
    public bool LiangBarskyEntersRectangle(Vector2 startGlobal, Vector2 endGlobal)
    {
        var x1 = startGlobal.X;
        var x2 = endGlobal.X;
        var y1 = startGlobal.Y;
        var y2 = endGlobal.Y;
        var dx = x2-x1;
        var dy = y2-y1;
        float[] p = {-dx, dx, -dy, dy};
        foreach (Window window in windows)
        {   
            bool parallel = false;
            float[] q = {x1 - window.xmin, window.xmax - x1, y1 - window.ymin, window.ymax - y1};
            float t_enter = 0;
            float t_exit = 1;
            for (int i = 0; i < 4; i++)
            {
                if (p[i] == 0)
                {
                    // the line is parallel - either horizontal or vertical
                    if (q[i] < 0)
                    {
                        // line outside rectangle 
                        parallel = true;
                        break;
                    }
                }
                else
                {
                    float t = q[i] / p[i];
                    if (p[i] < 0)
                    {
                        t_enter = Math.Max(t, t_enter);
                    }
                    else
                    {
                        t_exit = Math.Min(t, t_exit);
                    }
                }
            }           
            if (parallel)
            {
                continue;
            }
            if (t_enter > t_exit)
            {
                continue;
            }
            else
            {
                return true;
            }

        }
        return false;
    }
    
}
