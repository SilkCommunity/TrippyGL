using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Common;
using SixLabors.ImageSharp;
using TrippyGL;
using TrippyGL.ImageSharp;

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

            stopwatch = Stopwatch.StartNew();
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

            graphicsDevice.ClearColor = new Vector4(0, 0, 0, 1);
            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit);

            window.SwapBuffers();
        }

        public void OnWindowResized(System.Drawing.Size size)
        {
            if (size.Width == 0 || size.Height == 0)
                return;

            graphicsDevice.SetViewport(0, 0, (uint)size.Width, (uint)size.Height);
        }

        public void OnWindowClosing()
        {
            graphicsDevice.DisposeAllResources();
            graphicsDevice.Dispose();
        }
    }
}
