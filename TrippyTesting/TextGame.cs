using System;
using System.Diagnostics;
using System.Numerics;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Common;
using TrippyGL;
using TrippyGL.ImageSharp;
using TrippyGL.FontBuilding;
using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;

namespace TrippyTesting
{
    class TextGame
    {
        Stopwatch stopwatch;

        readonly Random r = new Random();
        readonly IWindow window;

        GraphicsDevice graphicsDevice;

        SimpleShaderProgram program;
        TextureBatcher batcher;

        Texture2D whitepx;
        Texture2D jeru;
        TextureFont font;

        public TextGame()
        {
            window = CreateWindow();

            window.Load += OnWindowLoad;
            window.Update += OnWindowUpdate;
            window.Render += OnWindowRender;
            window.Resize += OnWindowResized;
            window.Closing += OnWindowClosing;
        }

        private IWindow CreateWindow()
        {
            GraphicsAPI graphicsApi = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.Debug, new APIVersion(4, 0));
            VideoMode videoMode = new VideoMode(new System.Drawing.Size(1280, 720));
            ViewOptions viewOpts = new ViewOptions(true, 60.0, 60.0, graphicsApi, VSyncMode.On, 30, false, videoMode, 0);
            return Window.Create(new WindowOptions(viewOpts));
        }

        public void OnWindowLoad()
        {
            graphicsDevice = new GraphicsDevice(GL.GetApi(window));
            graphicsDevice.DebugMessagingEnabled = true;
            graphicsDevice.DebugMessage += Program.OnDebugMessage;

            program = SimpleShaderProgram.Create<VertexColorTexture>(graphicsDevice, 0, 0, true);
            batcher = new TextureBatcher(graphicsDevice);
            batcher.SetShaderProgram(program);

            whitepx = new Texture2D(graphicsDevice, 1, 1);
            whitepx.SetData<Color4b>(new Color4b[] { Color4b.White });
            jeru = Texture2DExtensions.FromFile(graphicsDevice, "data4/jeru.png");

            Font fontFile = SystemFonts.CreateFont("Arial", 48f, FontStyle.Regular);
            TextureFontData fontData = FontBuilder.CreateFontData(new FontGlyphSource(fontFile), out SixLabors.ImageSharp.Image<Rgba32> image, SixLabors.ImageSharp.Color.Transparent);
            Texture2D fontTexture = Texture2DExtensions.FromImage(graphicsDevice, image);
            font = fontData.CreateFont(fontTexture);
            fontTexture.SaveAsImage("fontitus.png", SaveImageFormat.Png);

            stopwatch = Stopwatch.StartNew();

            OnWindowResized(window.Size);
        }

        public void Run()
        {
            window.Run();
        }

        public void OnWindowUpdate(double dtSeconds)
        {
            GLEnum c;
            while ((c = graphicsDevice.GL.GetError()) != GLEnum.NoError)
            {
                Console.WriteLine("Error found: " + c);
            }
        }

        public void OnWindowRender(double dtSeconds)
        {
            if (window.IsClosing)
                return;

            graphicsDevice.BlendingEnabled = true;
            graphicsDevice.BlendState = BlendState.NonPremultiplied;
            graphicsDevice.DepthTestingEnabled = false;
            graphicsDevice.ClearColor = new Vector4(0, 0, 0, 1);
            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit);

            batcher.Begin(BatcherBeginMode.OnTheFly);
            Vector2 position = new Vector2(50, 100);
            batcher.Draw(jeru, position, Color4b.White);

            batcher.Draw(whitepx, position, null, Color4b.Red, new Vector2(1, font.Size));
            batcher.DrawString(font, "ola ole AVAVA", position, Color4b.White);
            batcher.End();

            window.SwapBuffers();
        }

        public void OnWindowResized(System.Drawing.Size size)
        {
            if (size.Width == 0 || size.Height == 0)
                return;

            graphicsDevice.SetViewport(0, 0, (uint)size.Width, (uint)size.Height);
            program.Projection = Matrix4x4.CreateOrthographicOffCenter(0, size.Width, size.Height, 0, 0, 1);
        }

        public void OnWindowClosing()
        {
            graphicsDevice.DisposeAllResources();
            graphicsDevice.Dispose();
        }
    }
}
