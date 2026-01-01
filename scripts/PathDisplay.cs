using Godot;
using System;

public partial class PathDisplay : Node2D
{
    private Vector2[] pathToDraw;
    public void DrawPath(Vector2[] path)
    {
        pathToDraw = new Vector2[path.Length];
        for (int i = 0; i < path.Length; i++)
        {
            pathToDraw[i] = ToLocal(path[i]);
        }

        QueueRedraw();
        
    }
    public override void _Draw()
    {
        if (pathToDraw == null)
        {
            return;
        }
        GD.Print("drawin");
        for (int i = 0; i < pathToDraw.Length-1; i++)
        {
            DrawLine(pathToDraw[i], pathToDraw[i+1], Colors.Red);
        }
    }
}
