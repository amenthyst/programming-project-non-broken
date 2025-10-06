using Godot;
using Godot.NativeInterop;
using System;
using System.Linq;

public partial class MeleeSwing : Area2D
{
    private CollisionPolygon2D collisionPolygon2D;
    [Export] private float angle = 90;

    [Export] private int amount_of_segments = 5;
    [Export] private float radius = 100f;
    [Export] private double lifetime = 1f;
    private double lifetimeTimer = 0f;

    public override void _Ready()
        
    {
        
        collisionPolygon2D = GetNode<CollisionPolygon2D>("CollisionShape2D");
        Vector2 directionVec = (GetGlobalMousePosition() - Position);
        float rotation = directionVec.Angle();
        collisionPolygon2D.Polygon = make_sector_shape(Mathf.DegToRad(angle), amount_of_segments, radius, rotation);
    }

    public override void _Process(double delta)
    {
        lifetimeTimer += delta;
        if (lifetimeTimer > lifetime)
        {
            QueueFree();
        }
    }
    private Vector2[] make_sector_shape(float angle, int amount_of_segments, float radius, float rotation)
    {
        // this function returns a vector2 array that when passed into the collisionpolygon2d node,
        // makes a sector shape, as sector shapes can model melee swings.
        GD.Print(Mathf.RadToDeg(rotation));
        Vector2[] vectorArray = new Vector2[amount_of_segments + 1]; // sets size to amount of segments + space for initial point
        vectorArray[0] = Vector2.Zero;
        float halfAngle = angle / 2; // starts at minus half angle.
        for (int i = 1; i <= amount_of_segments; i++)
        {

            float angle_of_segment = rotation - halfAngle + (i / (float)amount_of_segments * angle);
            double x = radius * Mathf.Cos(angle_of_segment); // can write x coordinate as cos(theta), as cos = a/h and in the unit circle h = 1, adjacent = x
            double y = radius * Mathf.Sin(angle_of_segment); // same with y, sin(theta) = o/h and h = 1, so sin(theta) = o
            Vector2 coordinate_of_point = new Vector2((float)x, (float)y);
            vectorArray[i] = coordinate_of_point;
        }
        return vectorArray;
    }


}
