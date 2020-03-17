using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Common;
using System;
using System.IO;
using System.Numerics;
using TrippyGL;

namespace TrippyTesting.Tests
{
    class IndexBufferTest
    {
        System.Diagnostics.Stopwatch stopwatch;
        static Random r = new Random();

        IWindow window;

        GraphicsDevice graphicsDevice;

        VertexColor[] vertexData;
        VertexBuffer<VertexColor> vertexBuffer;

        PrimitiveBatcher<VertexColor> extraLinesBatcher;
        VertexBuffer<VertexColor> extraLinesBuffer;

        ShaderProgram shaderProgram;

        public IndexBufferTest()
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
            ViewOptions viewOpts = new ViewOptions(true, 3.0, 3.0, graphicsApi, VSyncMode.Adaptive, 30, false, videoMode, 8);
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

            shaderProgram = new ShaderProgram(graphicsDevice);
            shaderProgram.AddVertexShader(File.ReadAllText("indextest/vs.glsl"));
            shaderProgram.AddFragmentShader(File.ReadAllText("indextest/fs.glsl"));
            shaderProgram.SpecifyVertexAttribs<VertexColor>(new string[] { "vPosition", "vColor" });
            shaderProgram.LinkProgram();
            Matrix4x4 mat = Matrix4x4.CreateScale(0.9f);
            shaderProgram.Uniforms["mat"].SetValueMat4(ref mat);

            const int w = 5, h = 5;
            vertexData = new VertexColor[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    vertexData[x + y * w] = new VertexColor(new Vector3(x / (float)w * 2f - 1 + randomf(-0.2f, 0.2f), y / (float)h * 2f - 1 + randomf(-0.1f, 0.1f), 0), randomCol());

            vertexBuffer = new VertexBuffer<VertexColor>(graphicsDevice, (uint)vertexData.Length, 128, DrawElementsType.UnsignedByte, BufferUsageARB.DynamicDraw, vertexData);

            extraLinesBatcher = new PrimitiveBatcher<VertexColor>(0, 32);
            extraLinesBuffer = new VertexBuffer<VertexColor>(graphicsDevice, (uint)extraLinesBatcher.LineVertexCapacity, BufferUsageARB.StreamDraw);

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

            graphicsDevice.ClearColor = new Vector4(0f, 0f, 0f, 1f);
            graphicsDevice.BlendingEnabled = false;
            graphicsDevice.DepthTestingEnabled = false;

            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit);

            graphicsDevice.VertexArray = vertexBuffer;
            graphicsDevice.ShaderProgram = shaderProgram;
            Span<byte> indices = stackalloc byte[4]
            {
                (byte)r.Next((int)vertexBuffer.StorageLength),
                14,
                (byte)r.Next((int)vertexBuffer.StorageLength),
                15
            };

            vertexBuffer.IndexSubset.SetData(indices);
            graphicsDevice.DrawElements(PrimitiveType.TriangleStrip, 1, 3);
            graphicsDevice.DrawArrays(PrimitiveType.Lines, 0, vertexBuffer.StorageLength);

            for (int i = 1; i < indices.Length; i++)
            {
                Vector3 p = vertexData[indices[i]].Position;
                extraLinesBatcher.AddLine(new VertexColor(p, Color4b.Red), new VertexColor(new Vector3(p.X, p.Y + 0.5f, p.Z), Color4b.Red));
            }

            if (extraLinesBatcher.LineVertexCount > extraLinesBuffer.StorageLength)
                extraLinesBuffer.RecreateStorage((uint)extraLinesBatcher.LineVertexCapacity);
            extraLinesBatcher.WriteLinesTo(extraLinesBuffer.DataSubset);
            graphicsDevice.VertexArray = extraLinesBuffer.VertexArray;
            graphicsDevice.DrawArrays(PrimitiveType.Lines, 0, (uint)extraLinesBatcher.LineVertexCount);
            extraLinesBatcher.ClearLines();

            window.SwapBuffers();
        }

        private void OnWindowResized(System.Drawing.Size size)
        {
            graphicsDevice.SetViewport(0, 0, size.Width, size.Height);
        }

        private void OnWindowClosing()
        {
            graphicsDevice.Dispose();
        }


        public static float randomf(float max)
        {
            return (float)r.NextDouble() * max;
        }
        public static float randomf(float min, float max)
        {
            return (float)r.NextDouble() * (max - min) + min;
        }
        public static Color4b randomCol()
        {
            return new Color4b((byte)r.Next(256), (byte)r.Next(256), (byte)r.Next(256), 255);
        }
    }
}
