using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Common;
using System;
using System.IO;
using System.Numerics;
using TrippyGL;

namespace TrippyTesting
{
    class Game4
    {
        System.Diagnostics.Stopwatch stopwatch;
        float time;
        Random r = new Random();

        IWindow window;

        ShaderProgram program;
        BufferObject buffer;
        UniformBufferSubset<ThreeMat4> uniformSubset;
        VertexDataBufferSubset<Vector3> positionSubset;
        VertexDataBufferSubset<Color4b> colorSubset;
        VertexDataBufferSubset<Vector2> texcoordSubset;

        BufferObject indexbuffer;
        IndexBufferSubset indexsubset;

        VertexArray array;

        VertexBuffer<VertexColorTexture> vertexbuffer;

        Texture2D texture, whitepx;

        GraphicsDevice graphicsDevice;

        public Game4()
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

            texture = new Texture2D(graphicsDevice, "data4/jeru.png");
            whitepx = new Texture2D(graphicsDevice, 1, 1);
            whitepx.SetData((ReadOnlySpan<Color4b>)new Color4b[] { Color4b.White });

            program = new ShaderProgram(graphicsDevice);
            program.AddVertexShader(File.ReadAllText("data4/vs.glsl"));
            program.AddFragmentShader(File.ReadAllText("data4/fs.glsl"));
            program.SpecifyVertexAttribs<VertexColorTexture>(new string[] { "vPosition", "vColor", "vTexCoords" });
            program.LinkProgram();

            Vector3[] vertexPositions = new Vector3[]
            {
                new Vector3(),
                new Vector3(-0.5f, -0.5f, 0),
                new Vector3(0, 0.5f, 0),
                new Vector3(0.5f, -0.5f, 0),
                new Vector3()
            };

            Color4b[] vertexColors = new Color4b[]
            {
                new Color4b(),
                new Color4b(255, 0, 0, 255),
                new Color4b(0, 255, 0, 255),
                new Color4b(0, 0, 255, 255),
                new Color4b()
            };

            Vector2[] vertexTexCoords = new Vector2[]
            {
                new Vector2(),
                new Vector2(0, 1),
                new Vector2(0.5f, 0),
                new Vector2(1, 1),
                new Vector2()
            };

            uint uboSizeBytes = UniformBufferSubset.CalculateRequiredSizeInBytes<ThreeMat4>(graphicsDevice, 1);
            buffer = new BufferObject(graphicsDevice, uboSizeBytes + (uint)(vertexPositions.Length * VertexColorTexture.SizeInBytes), BufferUsageARB.DynamicDraw);
            positionSubset = new VertexDataBufferSubset<Vector3>(buffer, uboSizeBytes, (uint)vertexPositions.Length, vertexPositions);
            colorSubset = new VertexDataBufferSubset<Color4b>(buffer, positionSubset.NextByteInBuffer, (uint)vertexColors.Length, vertexColors);
            texcoordSubset = new VertexDataBufferSubset<Vector2>(buffer, colorSubset.NextByteInBuffer, (uint)vertexTexCoords.Length, vertexTexCoords);

            const uint JEJJEJJEJJ = 523152;

            Span<ushort> indices = stackalloc ushort[] { 1, 2, 3, 1, 2, 3, 1, 2, 3, 1, 2, 3 };
            indexbuffer = new BufferObject(graphicsDevice, (uint)indices.Length * sizeof(ushort) + JEJJEJJEJJ, BufferUsageARB.DynamicDraw);
            indexsubset = new IndexBufferSubset(indexbuffer, JEJJEJJEJJ, (uint)indices.Length, indices);

            array = new VertexArray(graphicsDevice, new VertexAttribSource[]
            {
                new VertexAttribSource(positionSubset, AttributeType.FloatVec3),
                new VertexAttribSource(colorSubset, AttributeType.FloatVec4, true, VertexAttribPointerType.UnsignedByte),
                new VertexAttribSource(texcoordSubset, AttributeType.FloatVec2)
            }, indexsubset);

            uniformSubset = new UniformBufferSubset<ThreeMat4>(buffer, 0, 1);
            ThreeMat4 m = new ThreeMat4();
            m.World = Matrix4x4.Identity;
            m.View = Matrix4x4.Identity;
            m.Projection = Matrix4x4.Identity;
            uniformSubset.SetValue(m);
            program.BlockUniforms["MatrixBlock"].SetValue(uniformSubset);

            VertexColorTexture[] vertex = new VertexColorTexture[]
            {
                new VertexColorTexture(new Vector3(-0.8f, -0.8f, 0), new Color4b(255, 0, 0, 255), new Vector2(0, 0)),
                new VertexColorTexture(new Vector3(0.8f, -0.8f, 0), new Color4b(0, 255, 0, 255), new Vector2(0, 0)),
                new VertexColorTexture(new Vector3(-0.8f, 0.8f, 0), new Color4b(0, 0, 255, 255), new Vector2(0, 0)),
                new VertexColorTexture(new Vector3(0.8f, 0.8f, 0), new Color4b(255, 0, 255, 255), new Vector2(0, 0))
            };

            vertexbuffer = new VertexBuffer<VertexColorTexture>(graphicsDevice, vertex, BufferUsageARB.DynamicDraw);

            OnWindowResized(window.Size);
        }

        private void OnWindowUpdate(double dtSeconds)
        {
            time = (float)stopwatch.Elapsed.TotalSeconds;
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
            graphicsDevice.BlendState = BlendState.AlphaBlend;
            graphicsDevice.DepthTestingEnabled = false;

            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit);

            graphicsDevice.VertexArray = vertexbuffer.VertexArray;
            program.Uniforms["samp"].SetValueTexture(whitepx);
            graphicsDevice.ShaderProgram = program;
            graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, vertexbuffer.StorageLength);

            graphicsDevice.VertexArray = array;
            program.Uniforms["samp"].SetValueTexture(texture);
            graphicsDevice.ShaderProgram = program;
            graphicsDevice.DrawElements(PrimitiveType.TriangleStrip, 0, 3);

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
            program.Dispose();
            buffer.Dispose();
            array.Dispose();
            vertexbuffer.Dispose();
            texture.Dispose();
            whitepx.Dispose();
            indexbuffer.Dispose();

            graphicsDevice.Dispose();
        }

        float wave(float spd, float amp, float offset = 0f)
        {
            return MathF.Sin(time * spd + offset) * amp;
        }
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    struct ThreeMat4
    {
        public Matrix4x4 World;
        public Matrix4x4 View;
        public Matrix4x4 Projection;
    }
}
