using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Common;
using System;
using System.IO;
using System.Numerics;
using TrippyGL;

namespace TrippyTesting.Tests
{
    class DoSumShit
    {
        System.Diagnostics.Stopwatch stopwatch;
        public static Random r = new Random();
        public static float time;
        IWindow window;

        GraphicsDevice graphicsDevice;

        ShaderProgram program;

        BufferObject buffer;
        VertexDataBufferSubset<VertexColorTexture> bufferSubset;
        VertexArray vertexArray;

        Texture2D texture;

        public DoSumShit()
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


            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            time = 0;

            VertexColorTexture[] vertices = new VertexColorTexture[]
            {
                new VertexColorTexture(new Vector3(0, 0, 0), new Color4b(255, 0, 0, 255), new Vector2(0, 1)),
                new VertexColorTexture(new Vector3(1, 0, 0), new Color4b(0, 255, 0, 255), new Vector2(1, 1)),
                new VertexColorTexture(new Vector3(0, 1, 0), new Color4b(0, 0, 255, 255), new Vector2(0, 0)),
                new VertexColorTexture(new Vector3(1, 1, 0), new Color4b(255, 255, 0, 255), new Vector2(1, 0)),
            };

            buffer = new BufferObject(graphicsDevice, (uint)(vertices.Length * VertexColorTexture.SizeInBytes), BufferUsageARB.StaticDraw);
            bufferSubset = new VertexDataBufferSubset<VertexColorTexture>(buffer, vertices);
            vertexArray = VertexArray.CreateSingleBuffer<VertexColorTexture>(graphicsDevice, bufferSubset);

            program = new ShaderProgram(graphicsDevice);
            program.AddVertexShader(File.ReadAllText("sumshit/simple_vs.glsl"));
            program.AddFragmentShader(File.ReadAllText("sumshit/simple_fs.glsl"));
            program.SpecifyVertexAttribs<VertexColorTexture>(new string[] { "vPosition", "vColor", "vTexCoords" });
            program.LinkProgram();

            texture = new Texture2D(graphicsDevice, "data4/jeru.png", true);

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

            graphicsDevice.Framebuffer = null;
            graphicsDevice.SetViewport(0, 0, (uint)window.Size.Width, (uint)window.Size.Height);
            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            graphicsDevice.VertexArray = vertexArray;
            graphicsDevice.ShaderProgram = program;

            Matrix4x4 mat = Matrix4x4.Identity;
            program.Uniforms["World"].SetValueMat4(mat);
            program.Uniforms["View"].SetValueMat4(mat);
            program.Uniforms["Projection"].SetValueMat4(mat);
            program.Uniforms["tex"].SetValueTexture(texture);

            graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

            window.SwapBuffers();
        }

        private void OnWindowResized(System.Drawing.Size size)
        {
            if (size.Width == 0 || size.Height == 0)
                return;

            graphicsDevice.BlendState = BlendState.Additive;
            graphicsDevice.DepthState = DepthTestingState.None;
        }

        private void OnWindowClosing()
        {
            graphicsDevice.Dispose();
        }
    }
}
