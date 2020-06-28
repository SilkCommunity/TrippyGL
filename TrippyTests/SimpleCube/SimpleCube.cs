using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Common;
using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using TrippyGL;

namespace SimpleCube
{
    class SimpleCube
    {
        Stopwatch stopwatch;

        IWindow window;

        GraphicsDevice graphicsDevice;

        VertexBuffer<VertexColor> vertexBuffer;
        ShaderProgram shaderProgram;

        public SimpleCube()
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
            ViewOptions viewOpts = new ViewOptions(true, 60.0, 60.0, graphicsApi, VSyncMode.Adaptive, 30, false, videoMode, 8);
            return Window.Create(new WindowOptions(viewOpts));
        }

        public void Run()
        {
            window.Run();
        }

        private void OnWindowLoad()
        {
            graphicsDevice = new GraphicsDevice(GL.GetApi(window));

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

            Span<VertexColor> cubemapBufferData = stackalloc VertexColor[] {
                new VertexColor(new Vector3(-0.5f, -0.5f, -0.5f), Color4b.Red), //4
                new VertexColor(new Vector3(-0.5f, -0.5f, 0.5f), Color4b.Lime), //3
                new VertexColor(new Vector3(-0.5f, 0.5f, -0.5f), Color4b.Blue), //7
                new VertexColor(new Vector3(-0.5f, 0.5f, 0.5f), Color4b.Red), //8
                new VertexColor(new Vector3(0.5f, 0.5f, 0.5f), Color4b.Blue), //5
                new VertexColor(new Vector3(-0.5f, -0.5f, 0.5f), Color4b.Lime), //3
                new VertexColor(new Vector3(0.5f, -0.5f, 0.5f), Color4b.Red), //1
                new VertexColor(new Vector3(-0.5f, -0.5f, -0.5f), Color4b.Red), //4
                new VertexColor(new Vector3(0.5f, -0.5f, -0.5f), Color4b.Lime), //2
                new VertexColor(new Vector3(-0.5f, 0.5f, -0.5f), Color4b.Blue), //7
                new VertexColor(new Vector3(0.5f, 0.5f, -0.5f), Color4b.Red), //6
                new VertexColor(new Vector3(0.5f, 0.5f, 0.5f), Color4b.Blue), //5
                new VertexColor(new Vector3(0.5f, -0.5f, -0.5f), Color4b.Lime), //2
                new VertexColor(new Vector3(0.5f, -0.5f, 0.5f), Color4b.Red), //1
            };

            vertexBuffer = new VertexBuffer<VertexColor>(graphicsDevice, cubemapBufferData, BufferUsageARB.StaticCopy);

            ShaderProgramBuilder programBuilder = new ShaderProgramBuilder();
            programBuilder.VertexShaderCode = File.ReadAllText("vs.glsl");
            programBuilder.FragmentShaderCode = File.ReadAllText("fs.glsl");
            programBuilder.SpecifyVertexAttribs<VertexColor>(new string[] { "vPosition", "vColor" });
            shaderProgram = programBuilder.Create(graphicsDevice, true);

            shaderProgram.Uniforms["World"].SetValueMat4(Matrix4x4.Identity);
            shaderProgram.Uniforms["View"].SetValueMat4(Matrix4x4.CreateLookAt(new Vector3(0, 1.0f, -1.5f), Vector3.Zero, Vector3.UnitY));

            graphicsDevice.DepthState = DepthTestingState.Default;
            graphicsDevice.BlendState = BlendState.Opaque;

            OnWindowResized(window.Size);
            stopwatch = Stopwatch.StartNew();
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

            graphicsDevice.ClearDepth = 1f;
            graphicsDevice.ClearColor = Vector4.Zero;
            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            shaderProgram.Uniforms["World"].SetValueMat4(Matrix4x4.CreateRotationY(2 * (float)stopwatch.Elapsed.TotalSeconds));
            graphicsDevice.ShaderProgram = shaderProgram;
            graphicsDevice.VertexArray = vertexBuffer;

            graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, vertexBuffer.StorageLength);

            window.SwapBuffers();
        }

        private void OnWindowResized(System.Drawing.Size size)
        {
            if (size.Width == 0 || size.Height == 0)
                return;

            graphicsDevice.SetViewport(0, 0, (uint)size.Width, (uint)size.Height);
            shaderProgram.Uniforms["Projection"].SetValueMat4(Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI/2f, window.Size.Width / (float)window.Size.Height, 0.01f, 100f));
        }

        private void OnWindowClosing()
        {
            graphicsDevice.Dispose();
        }
    }
}
