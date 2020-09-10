using System;
using System.IO;
using System.Numerics;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Common;
using TrippyGL;
using TrippyGL.ImageSharp;

namespace TrippyTesting.Tests
{
    class ObjFiles
    {
        System.Diagnostics.Stopwatch stopwatch;
        public static Random r = new Random();
        public static float time, deltaTime;
        readonly IWindow window;

        GraphicsDevice graphicsDevice;

        VertexBuffer<VertexNormalTexture> cubeBuffer;

        Texture2D texture;

        ShaderProgram shaderProgram;
        BufferObject uniformBuffer;
        UniformBufferSubset<ThreeMat4> uniformSubset;
        ThreeMat4 matrices;

        public ObjFiles()
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
            ViewOptions viewOpts = new ViewOptions(true, 60.0, 60.0, graphicsApi, VSyncMode.On, 30, false, videoMode, 24);
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

            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            time = 0;

            texture = Texture2DExtensions.FromFile(graphicsDevice, "objs/stallTexture.png", true);
            texture.SetTextureFilters(TextureMinFilter.LinearMipmapLinear, TextureMagFilter.Linear);

            VertexNormalTexture[] cube = OBJLoader.FromFile<VertexNormalTexture>("objs/stall.obj");

            cubeBuffer = new VertexBuffer<VertexNormalTexture>(graphicsDevice, cube, BufferUsageARB.StaticDraw);

            ShaderProgramBuilder programBuilder = new ShaderProgramBuilder();
            programBuilder.VertexShaderCode = File.ReadAllText("objs/stall_vs.glsl");
            programBuilder.FragmentShaderCode = File.ReadAllText("objs/stall_fs.glsl");
            programBuilder.SpecifyVertexAttribs<VertexNormalTexture>(new string[] { "vPosition", "vNormal", "vTexCoords" });
            shaderProgram = programBuilder.Create(graphicsDevice, true);
            Console.WriteLine("VS Log: " + programBuilder.VertexShaderLog);
            Console.WriteLine("FS Log: " + programBuilder.FragmentShaderLog);
            Console.WriteLine("Program Log: " + programBuilder.ProgramLog);

            uniformBuffer = new BufferObject(graphicsDevice, UniformBufferSubset.CalculateRequiredSizeInBytes<ThreeMat4>(graphicsDevice, 1), BufferUsageARB.StreamDraw);
            uniformSubset = new UniformBufferSubset<ThreeMat4>(uniformBuffer);
            shaderProgram.BlockUniforms["MatrixBlock"].SetValue(uniformSubset);

            shaderProgram.Uniforms["samp"].SetValueTexture(texture);

            graphicsDevice.BlendingEnabled = false;
            graphicsDevice.DepthState = DepthState.Default;

            OnWindowResized(window.Size);
        }

        private void OnWindowUpdate(double dtSeconds)
        {
            float prevTime = time;
            time = (float)stopwatch.Elapsed.TotalSeconds;
            deltaTime = time - prevTime;
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
            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            matrices.View = Matrix4x4.CreateLookAt(new Vector3(0, 1, -2), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
            matrices.World = Matrix4x4.CreateScale(0.2f) * Matrix4x4.CreateRotationY(time * MathF.PI / 4f);
            uniformSubset.SetValue(matrices);

            graphicsDevice.ShaderProgram = shaderProgram;
            graphicsDevice.VertexArray = cubeBuffer;
            graphicsDevice.DrawArrays(PrimitiveType.Triangles, 0, cubeBuffer.StorageLength);

            window.SwapBuffers();
        }

        private void OnWindowResized(System.Drawing.Size size)
        {
            if (size.Width == 0 || size.Height == 0)
                return;

            graphicsDevice.SetViewport(0, 0, (uint)size.Width, (uint)size.Height);

            matrices.Projection = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 2f, window.Size.Width / (float)window.Size.Height, 0.1f, 50f);
        }

        private void OnWindowClosing()
        {
            cubeBuffer.Dispose();
            uniformBuffer.Dispose();
            shaderProgram.Dispose();
            texture.Dispose();
            graphicsDevice.Dispose();
        }
    }
}
