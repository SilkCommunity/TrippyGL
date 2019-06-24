using System;
using System.IO;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using TrippyGL;


namespace TrippyTesting
{
    class Game4 : GameWindow
    {
        System.Diagnostics.Stopwatch stopwatch;
        float time;
        Random r = new Random();

        ShaderProgram program;
        BufferObject buffer;
        VertexDataBufferSubset<VertexColor> vertexSubset;
        UniformBufferSubset<ThreeMat4> uniformSubset;

        VertexArray array;

        GraphicsDevice graphicsDevice;

        public Game4() : base(1280, 720, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 0, 0, 8, ColorFormat.Empty, 2), "haha yes", GameWindowFlags.Default, DisplayDevice.Default, 4, 0, GraphicsContextFlags.Default)
        {
            VSync = VSyncMode.On;
            graphicsDevice = new GraphicsDevice(this.Context);
            graphicsDevice.DebugMessagingEnabled = true;
            graphicsDevice.DebugMessage += Program.OnDebugMessage;

            Console.WriteLine(String.Concat("GL Version: ", graphicsDevice.GLMajorVersion, ".", graphicsDevice.GLMinorVersion));
            Console.WriteLine("GL Version String: " + graphicsDevice.GLVersion);
            Console.WriteLine("GL Vendor: " + graphicsDevice.GLVendor);
            Console.WriteLine("GL Renderer: " + graphicsDevice.GLRenderer);
            Console.WriteLine("GL ShadingLanguageVersion: " + graphicsDevice.GLShadingLanguageVersion);
            Console.WriteLine("GL TextureUnits: " + graphicsDevice.MaxTextureImageUnits);
            Console.WriteLine("GL MaxTextureSize: " + graphicsDevice.MaxTextureSize);
            Console.WriteLine("GL MaxSamples:" + graphicsDevice.MaxSamples);
        }

        protected override void OnLoad(EventArgs e)
        {
            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            time = 0;

            program = new ShaderProgram(graphicsDevice);
            program.AddVertexShader(File.ReadAllText("data4/vs.glsl"));
            program.AddFragmentShader(File.ReadAllText("data4/fs.glsl"));
            program.SpecifyVertexAttribs<VertexColor>(new string[] { "vPosition", "vColor" });
            program.LinkProgram();

            VertexColor[] vertex = new VertexColor[]
            {
                new VertexColor(new Vector3(-0.5f, -0.5f, 0), new Color4b(255, 0, 0, 255)),
                new VertexColor(new Vector3(0, 0.5f, 0), new Color4b(0, 255, 0, 255)),
                new VertexColor(new Vector3(0.5f, -0.5f, 0), new Color4b(0, 0, 255, 255)),
            };

            int uboSizeBytes = UniformBufferSubset<ThreeMat4>.CalculateRequiredSizeInBytes(graphicsDevice, 1);
            buffer = new BufferObject(graphicsDevice, vertex.Length * VertexColor.SizeInBytes + uboSizeBytes, BufferUsageHint.StaticDraw);

            uniformSubset = new UniformBufferSubset<ThreeMat4>(buffer, 0, 1);
            ThreeMat4 m = new ThreeMat4();
            m.World = Matrix4.Identity;
            m.View = Matrix4.Identity;
            m.Projection = Matrix4.Identity;
            uniformSubset.SetValue(ref m);
            program.BlockUniforms["MatrixBlock"].SetValue(uniformSubset);

            vertexSubset = new VertexDataBufferSubset<VertexColor>(buffer, uboSizeBytes, vertex.Length);
            vertexSubset.SetData(vertex);

            array = VertexArray.CreateSingleBuffer<VertexColor>(graphicsDevice, vertexSubset);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            time = (float)stopwatch.Elapsed.TotalSeconds;
            ErrorCode c;
            while ((c = GL.GetError()) != ErrorCode.NoError)
            {
                Console.WriteLine("Error found: " + c);
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            graphicsDevice.ClearColor = new Color4(0, 0, 0, 1);
            graphicsDevice.BlendingEnabled = false;
            graphicsDevice.DepthTestingEnabled = false;

            GL.Clear(ClearBufferMask.ColorBufferBit);


            VertexColor[] vertex = new VertexColor[]
            {
                new VertexColor(new Vector3(-0.5f, -0.5f, 0), new Color4b(255, 0, 0, 255)),
                new VertexColor(new Vector3(wave(3.14f, 0.5f, 0f), 0.5f, 0), new Color4b(0, 255, 0, 255)),
                new VertexColor(new Vector3(0.5f+wave(3f, 0.1f), -0.5f+wave(3f, 0.1f), 0), new Color4b(0, 0, 255, 255)),
            };
            vertexSubset.SetData(vertex);

            graphicsDevice.BindVertexArray(array);
            program.EnsurePreDrawStates();
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 3);

            SwapBuffers();
        }

        protected override void OnUnload(EventArgs e)
        {
            program.Dispose();
            buffer.Dispose();

            graphicsDevice.Dispose();
        }

        protected override void OnResize(EventArgs e)
        {
            graphicsDevice.Viewport = new Rectangle(0, 0, this.Width, this.Height);
        }

        float wave(float spd, float amp, float offset = 0f)
        {
            return (float)Math.Sin(time * spd + offset) * amp;
        }
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    struct ThreeMat4
    {
        public Matrix4 World;
        public Matrix4 View;
        public Matrix4 Projection;
    }
}
