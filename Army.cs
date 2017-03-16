using System;
using OpenTK.Graphics.OpenGL;

public class Army
{
    public const int DefaultMoveRange = 20;
    public const int DefaultRange = 50;

    private readonly int maxHealth = 100;

    public Army(Pos position, int health)
    {
        Position = position;
        Health = health;
        Range = DefaultRange;
        MoveRange = DefaultMoveRange;
    }

    public Pos Position { get; set; }

    public int Range { get; private set; }

    public int MoveRange { get; private set; }

    public int Health { get; private set; }

    public void FeedArmy(int food)
    {
        this.Health += food;
        this.Health = Math.Min(this.Health, maxHealth);
    }

    public int DistanceTo(Pos target)
    {
        return Math.Abs(target.X - Position.X) + Math.Abs(target.Y - Position.Y);
    }

    public void Render()
    {
        GL.MatrixMode(MatrixMode.Modelview);
        GL.PushMatrix();
        GL.Translate(Position.X, Position.Y, 0);
        GL.Begin(PrimitiveType.Triangles);
        GL.Vertex2(0.7f, 0.3f);
        GL.Vertex2(0.5f, 0.7f);
        GL.Vertex2(0.3f, 0.3f);
        GL.End();
        GL.PopMatrix();
    }
}
