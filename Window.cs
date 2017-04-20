using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

public class Window : GameWindow
{
    public const int HUDPIXELHEIGHT = 100;
    public static readonly Color BACKGROUND = new Color(0.5f, 0.5f, 0.5f);
    public static readonly Color HUDBACKGROUND = new Color(0.09375f, 0.27f, 0.4f);
    public static readonly int BORDER = 2;
    private static string fontBitmapFilename = "font.bmp";
    private static int glyphsPerLine = 16;

    // private static int GlyphLineCount = 16;
    private static int glyphWidth = 11;
    private static int glyphHeight = 22;

    private static int charXSpacing = 11;
    private Game game;
    private int playerID;
    private Army army;
    private int clickFlag = 0; // 0: initial state, 1: army clicked, 2: Confirmation step, clicking on the same spot will decrement it
    private Pos pos;
    private float centerX;
    private float centerY;
    private float scale; // scale = pixels per world square
    private int fontTextureID;
    private int textureWidth;
    private int textureHeight;

    // private static string Text = "GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);";

    // Used to offset rendering glyphs to bitmap
    // private static int AtlasOffsetX = -3, AtlassOffsetY = -1;
    // private static int FontSize = 14;
    // private static bool BitmapFont = false;
    // private static string FromFile; //= "joystix monospace.ttf";
    // private static string FontName = "Consolas";
    public Window(int width, int height, Game game)
        : base(width, height, GraphicsMode.Default, "WW3")
    {
        this.game = game;
        scale = 15;
        centerX = World.WIDTH / 2;
        centerY = World.HEIGHT / 2;
        VSync = VSyncMode.On;
    }

    public void DrawText(int x, int y, string text)
    {
        GL.Enable(EnableCap.Texture2D);
        GL.Begin(PrimitiveType.Quads);

        float u_step = (float)glyphWidth / (float)textureWidth;
        float v_step = (float)glyphHeight / (float)textureHeight;

        for (int n = 0; n < text.Length; n++)
        {
            char idx = text[n];
            float u = (float)(idx % glyphsPerLine) * u_step;
            float v = (float)(idx / glyphsPerLine) * v_step;

            GL.TexCoord2(u, v + v_step);
            GL.Vertex2(x, y);
            GL.TexCoord2(u + u_step, v + v_step);
            GL.Vertex2(x + glyphWidth, y);
            GL.TexCoord2(u + u_step, v);
            GL.Vertex2(x + glyphWidth, y + glyphHeight);
            GL.TexCoord2(u, v);
            GL.Vertex2(x, y + glyphHeight);

            x += charXSpacing;
        }

        GL.End();
        GL.Disable(EnableCap.Texture2D);
    }

    public void Blt(double x, double y, double width, double height)
    {
        GL.Begin(PrimitiveType.Quads);
        GL.TexCoord2(0, 0);
        GL.Vertex2(x, y);
        GL.TexCoord2(1, 0);
        GL.Vertex2(x + width, y);
        GL.TexCoord2(1, 1);
        GL.Vertex2(x + width, y + height);
        GL.TexCoord2(0, 1);
        GL.Vertex2(x, y + height);
        GL.End();
    }

    public void Render(Army army)
    {
        var pos = game.Manager.ArmyPosition(army);
        GL.MatrixMode(MatrixMode.Modelview);
        GL.PushMatrix();
        GL.Translate(pos.X, pos.Y, 0);
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
        // render provinces
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

        // render gridlines
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

        // render selected province
        GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
        GL.Color3(1.0, 0.5, 0.0);
        GL.Rect(pos.X, pos.Y, pos.X + 1, pos.Y + 1);
        GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
    }

    public void RenderHealth()
    {
        if (army != null)
        {
            var x = 30;
            var y = 30;
            var size = 45;
            var thickness = 10;
            Color.RED.Use();
            GL.Rect(x + (size / 2) - (thickness / 2), y, x + (size / 2) + (thickness / 2), y + size);
            GL.Rect(x, y + (size / 2) - (thickness / 2), x + size, y + (size / 2) + (thickness / 2));
            Color.BLACK.Use();

            var barX = 100;
            var barLength = 100;
            var barFilled = army.Health;
            GL.Rect(barX - BORDER, y - BORDER, barX + barLength + BORDER, y + size + BORDER);
            HUDBACKGROUND.Use();
            GL.Rect(barX, y, barX + barLength, y + size);
            Color.GREEN.Use();
            GL.Rect(barX, y, barX + barFilled, y + size);
        }
    }

    public void RenderHUD(World world)
    {
        // render HUD background
        GL.Viewport(0, 0, Width, HUDPIXELHEIGHT);
        GL.MatrixMode(MatrixMode.Projection);
        GL.LoadIdentity();
        GL.Ortho(0.0, Width, 0.0, HUDPIXELHEIGHT, 1, -1);
        HUDBACKGROUND.Use();
        GL.Rect(0, 0, Width, HUDPIXELHEIGHT);

        RenderHealth();

        // render province info
        world.GetProvinceAt(pos);
        int food = 5;
        int weapons = 12;
        var right = Width;
        var foodX = right - 100;
        var gunX = right - 200;
        float squareSize = 40;
        var squareY = (HUDPIXELHEIGHT / 2) - (squareSize / 2);
        int foodSideNum = (int)Math.Sqrt(food) + 1;
        int weaponsSideNum = (int)Math.Sqrt(weapons) + 1;
        float foodSize = squareSize / foodSideNum;
        float weaponSize = squareSize / weaponsSideNum;

        // food
        Color.BLACK.Use();
        GL.Rect(foodX - BORDER, squareY - BORDER, foodX + squareSize + BORDER, squareY + squareSize + BORDER);
        HUDBACKGROUND.Use();
        GL.Rect(foodX, squareY, foodX + squareSize, squareY + squareSize);

        Color.RED.Use();
        for (int i = 0; i < food; i++)
        {
            var x2 = foodX + ((i % foodSideNum) * foodSize);
            var y2 = squareY + ((i / foodSideNum) * foodSize);
            GL.Rect(x2 + 1, y2 + 1, x2 + foodSize - 1, y2 + foodSize - 1);
        }

        // weapons
        Color.BLACK.Use();
        GL.Rect(gunX - BORDER, squareY - BORDER, gunX + squareSize + BORDER, squareY + squareSize + BORDER);
        HUDBACKGROUND.Use();
        GL.Rect(gunX, squareY, gunX + squareSize, squareY + squareSize);
        new Color(0.8f, 0.6f, 0.0f).Use();
        for (int i = 0; i < weapons; i++)
        {
            var x2 = gunX + ((i % weaponsSideNum) * weaponSize);
            var y2 = squareY + ((i / weaponsSideNum) * weaponSize);
            GL.Rect(x2 + 1, y2 + 1, x2 + weaponSize - 1, y2 + weaponSize - 1);
        }
    }

    public float GetLeft()
    {
        return centerX - (Width / 2 / scale);
    }

    public float GetRight()
    {
        return centerX + (Width / 2 / scale);
    }

    public float GetBottom()
    {
        return centerY - ((Height - HUDPIXELHEIGHT) / 2 / scale);
    }

    public float GetTop()
    {
        return centerY + ((Height - HUDPIXELHEIGHT) / 2 / scale);
    }

    /*private void RenderBitmap()
    {
        //GL.ClearColor(Color.MidnightBlue);
        GL.Enable(EnableCap.Texture2D);

        GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

        GL.GenTextures(1, out texture);
        GL.BindTexture(TextureTarget.Texture2D, texture);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        int width = 800;
        int height = 600;
        byte[, ,] data = new byte[height, width, 3];

        for (int y = 0; y < height; y++)
           {
           for (int x = 0; x < width; x++)
              {
              for(int channel = 0; channel < 3; channel++)
                 {
                 data[y, x, channel] = 254;
                 }
              }
           }

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, width, height, 0,
            OpenTK.Graphics.OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, data);
    }*/

    /*private void InitializeTextRendering()
    {
        //GL.Ortho(0, controlWidth, 0, controlHeight, -1000, 1000);
        //GL.Scale(1, -1, 1); // I work with a top/left image and openGL is bottom/left
        //GL.Viewport(0, 0, controlWidth, controlHeight);
        //GL.ClearColor(Color.White);
        GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);
        GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
        GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
        GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
        GL.Enable(EnableCap.PointSmooth);
        GL.Enable(EnableCap.LineSmooth);
        GL.Enable(EnableCap.Blend);
        GL.Enable(EnableCap.DepthTest);
        GL.ShadeModel(ShadingModel.Smooth);
        GL.Enable(EnableCap.AutoNormal);

        var bmp = new Bitmap(width, height);
        var gfx = Graphics.FromImage(bmp);
        gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

        var texture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, texture);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp.Width, bmp.Height, 0,
        OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
    }*/

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        fontTextureID = LoadTexture(fontBitmapFilename);
        GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
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

        var left = GetLeft();
        var right = GetRight();
        var bottom = GetBottom();
        var top = GetTop();

        GL.Viewport(0, HUDPIXELHEIGHT, Width, Height - HUDPIXELHEIGHT);
        GL.MatrixMode(MatrixMode.Projection);
        GL.LoadIdentity();
        GL.Ortho(left, right, bottom, top, 1.0, -1.0);

        GL.Clear(ClearBufferMask.ColorBufferBit);

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
        RenderHUD(world);

        // lol
        // DrawText(0, 0, "HELLO WORLD");
        SwapBuffers();
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);
        var a = Mouse.GetCursorState();
        int mouseX = e.Position.X;
        int mouseY = Height - e.Position.Y - 1;

        float left = GetLeft();
        float right = GetRight();
        float bottom = GetBottom();
        float top = GetTop();

        float worldX = left + (((float)mouseX / Width) * (right - left));
        float worldY = bottom + (((float)(mouseY - HUDPIXELHEIGHT) / (Height - HUDPIXELHEIGHT)) * (top - bottom));

        int x = (int)worldX;
        int y = (int)worldY;
        Console.WriteLine("x is: " + x + " y is: " + y);
        Player player = game.CurrentPlayer;
        if (clickFlag == 0)
        {
            playerID = game.CurrentPlayerIndex;
            pos = new Pos(x, y);
            army = game.Manager.ArmyAt(pos);
            if (army != null)
            {
                Console.WriteLine("Army clicked.");
                clickFlag = 1;
            }
            else
            {
                Console.WriteLine("Invalid click, not an army. Try again.");
            }
        }
        else if (clickFlag == 1 || clickFlag == 2)
        {
            pos = new Pos(x, y);
            if (game.Manager.CanMoveTo(army, pos) == true)
            {
                Console.WriteLine("Press 'y' now to confirm move.");
                clickFlag = 2;
            }
            else
            {
                Console.WriteLine("Invalid move. Try again.");
            }
        }
    }

    protected override void OnKeyPress(OpenTK.KeyPressEventArgs e)
    {
        if (e.KeyChar == 'y' && clickFlag == 2)
        {
            game.Manager.MoveArmy(army, pos);
            Console.WriteLine("Army has moved.");
            clickFlag = 0;
        }

        if (e.KeyChar == '+')
        {
            scale *= 1.1f;
        }

        if (e.KeyChar == '-')
        {
            scale /= 1.1f;
        }

        if (e.KeyChar == 'w')
        {
            centerY += 1;
        }

        if (e.KeyChar == 'a')
        {
            centerX -= 1;
        }

        if (e.KeyChar == 'd')
        {
            centerX += 1;
        }

        if (e.KeyChar == 's')
        {
            centerY -= 1;
        }

        if (e.KeyChar == 'n')
        {
            game.AdvancePlayer();
            Console.WriteLine("Ended turn");
        }
    }

    private int LoadTexture(string filename)
    {
        using (var bitmap = new System.Drawing.Bitmap(filename))
        {
            var texId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, fontTextureID);
            System.Drawing.Imaging.BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmap.Width, bitmap.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            bitmap.UnlockBits(data);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            textureWidth = bitmap.Width;
            textureHeight = bitmap.Height;
            return texId;
        }
    }
}