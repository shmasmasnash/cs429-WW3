using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

public class Window : GameWindow
{
    public static readonly int HUDPIXELHEIGHT = 100;

    private Game game;

    private double centerX;

    private double centerY;

    // scale = pixels per world square
    private double scale;

    public Window(int width, int height, Game game)
        : base(width, height, GraphicsMode.Default, "WW3")
    {
        this.game = game;
        scale = 15;
        centerX = World.WIDTH / 2;
        centerY = World.HEIGHT / 2;
        VSync = VSyncMode.On;
    }

    public void Render(Army army)
    {
        GL.MatrixMode(MatrixMode.Modelview);
        GL.PushMatrix();
        GL.Translate(army.Position.X, army.Position.Y, 0);
        GL.Begin(PrimitiveType.Triangles);
        GL.Vertex2(0.7f, 0.3f);
        GL.Vertex2(0.5f, 0.7f);
        GL.Vertex2(0.3f, 0.3f);
        GL.End();
        GL.PopMatrix();
    }

    public void Render(City city)
    {
        Color.GREEN.Use();
        float border = (1.0f - City.SIZE) / 2;
        GL.Rect(border, border, 1.0f - border, 1.0f - border);
    }

    public void Render(Province province)
    {
        Color c = province.Owner?.Color ?? new Color(0.5f, 0.5f, 0.5f);

        c.Use();
        GL.Rect(0.0f, 0.0f, 1.0f, 1.0f);

        if (province.City != null)
        {
            Render(province.City);
        }
    }

    public void Render(World world)
    {
        for (int x = 0; x < World.WIDTH; x++)
        {
            for (int y = 0; y < World.HEIGHT; y++)
            {
                GL.MatrixMode(MatrixMode.Modelview);
                GL.PushMatrix();
                GL.Translate(x, y, 0);
                Render(world.GetProvinceAt(new Pos(x, y)));
                GL.PopMatrix();
            }
        }

        GL.Begin(PrimitiveType.Lines);
        Color.BLACK.Use();
        for (int x = 0; x < World.WIDTH; x++)
        {
            GL.Vertex2(x, 0);
            GL.Vertex2(x, World.HEIGHT);
        }

        for (int y = 0; y < World.HEIGHT; y++)
        {
            GL.Vertex2(0, y);
            GL.Vertex2(World.WIDTH, y);
        }

        GL.End();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        // GL.Enable(EnableCap.DepthTest);
    }

    protected override void OnResize(EventArgs e)
    {
        /*
        base.OnResize(e);

        GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);

        GL.MatrixMode(MatrixMode.Projection);
        GL.LoadIdentity();
        GL.Ortho(0.0, World.WIDTH, 0.0, World.HEIGHT, 1.0, -1.0);
        */
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);

        // not strictly necessary
        if (Keyboard[Key.Escape])
        {
            Exit();
        }
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);

        var left = centerX - (ClientRectangle.Width / 2 / scale);
        var right = centerX + (ClientRectangle.Width / 2 / scale);
        var bottom = centerY - ((ClientRectangle.Height - HUDPIXELHEIGHT) / 2 / scale);
        var top = centerY + ((ClientRectangle.Height - HUDPIXELHEIGHT) / 2 / scale);
        GL.Viewport(0, HUDPIXELHEIGHT, ClientRectangle.Width, ClientRectangle.Height - HUDPIXELHEIGHT);
        GL.MatrixMode(MatrixMode.Projection);
        GL.LoadIdentity();
        GL.Ortho(left, right, bottom, top, 1.0, -1.0);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        World world = game.World;
        Render(world);
        foreach (var player in game.Players)
        {
            player.Color.Use();
            foreach (var army in player.ArmyList)
            {
                Render(army);
            }

            // RenderResources(player.ResourcesString())
        }

        // render HUD
        GL.Viewport(0, 0, ClientRectangle.Width, HUDPIXELHEIGHT);
        GL.MatrixMode(MatrixMode.Projection);
        GL.LoadIdentity();
        GL.Ortho(0.0, ClientRectangle.Width, 0.0, HUDPIXELHEIGHT, 1, -1);
        GL.Color3(0.2, 0.2, 0.2);
        GL.Rect(0, 0, ClientRectangle.Width, HUDPIXELHEIGHT);

        SwapBuffers();
    }
}