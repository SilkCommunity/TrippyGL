using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Common;
using System;
using System.IO;
using System.Numerics;
using TrippyGL;

namespace TrippyTesting
{
    class SimpleTriangle
    {
        IWindow window;

        GraphicsDevice graphicsDevice;

        VertexBuffer<VertexColor> vertexBuffer;
        ShaderProgram program;

        public SimpleTriangle()
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
            GraphicsAPI graphicsApi = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.Debug, new APIVersion(3, 3));
            VideoMode videoMode = new VideoMode(new System.Drawing.Size(1280, 720));
            ViewOptions viewOpts = new ViewOptions(true, 60.0, 60.0, graphicsApi, VSyncMode.Adaptive, 30, false, videoMode, 0);
            return Window.Create(new WindowOptions(viewOpts));
        }

        public void Run()
        {
            window.Run();
        }

        private void OnWindowLoad()
        {
            graphicsDevice = new GraphicsDevice(GL.GetApi());
            graphicsDevice.DebugMessagingEnabled = true;
            graphicsDevice.DebugMessage += Program.OnDebugMessage;

            Console.WriteLine(string.Concat("GL Version: ", graphicsDevice.GLMajorVersion, ".", graphicsDevice.GLMinorVersion));
            Console.WriteLine("GL Version String: " + graphicsDevice.GLVersion);
            Console.WriteLine("GL Vendor: " + graphicsDevice.GLVendor);
            Console.WriteLine("GL Renderer: " + graphicsDevice.GLRenderer);
            Console.WriteLine("GL ShadingLanguageVersion: " + graphicsDevice.GLShadingLanguageVersion);
            Console.WriteLine("GL TextureUnits: " + graphicsDevice.MaxTextureImageUnits);
            Console.WriteLine("GL MaxTextureSize: " + graphicsDevice.MaxTextureSize);
            Console.WriteLine("GL MaxSamples: " + graphicsDevice.MaxSamples);

            Span<VertexColor> vertexData = stackalloc VertexColor[]
            {
                new VertexColor(new Vector3(-0.5f, -0.5f, 0f), new Color4b(255, 0, 0, 255)),
                new VertexColor(new Vector3(0.0f, 0.5f, 0f), new Color4b(0, 255, 0, 255)),
                new VertexColor(new Vector3(0.5f, -0.5f, 0f), new Color4b(0, 0, 255, 255))
            };

            vertexBuffer = new VertexBuffer<VertexColor>(graphicsDevice, vertexData, BufferUsageARB.StaticDraw);

            ShaderProgramBuilder programBuilder = new ShaderProgramBuilder();
            programBuilder.VertexShaderCode = File.ReadAllText("triangle/vs.glsl");
            programBuilder.FragmentShaderCode = File.ReadAllText("triangle/fs.glsl");
            programBuilder.SpecifyVertexAttribs<VertexColor>(new string[] { "vPosition", "vColor" });
            program = programBuilder.Create(graphicsDevice, true);

            OnWindowResized(window.Size);
        }

        private void OnWindowUpdate(double dtSeconds)
        {
            GLEnum c;
            while ((c = graphicsDevice.GL.GetError()) != GLEnum.NoError)
            {
                Console.WriteLine("Error found: " + c);
            }
        }

        private void OnWindowRender(double dtSeconds)
        {
            if (window.IsClosing)
                return;

            graphicsDevice.ClearColor = new Vector4(0, 0, 0, 1);
            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit);

            graphicsDevice.VertexArray = vertexBuffer;
            graphicsDevice.ShaderProgram = program;
            graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, 3);

            window.SwapBuffers();
        }

        private void OnWindowResized(System.Drawing.Size size)
        {
            if (size.Width == 0 || size.Height == 0)
                return;

            graphicsDevice.SetViewport(0, 0, (uint)size.Width, (uint)size.Height);
        }

        private void OnWindowClosing()
        {
            graphicsDevice.Dispose();
        }
    }
}
