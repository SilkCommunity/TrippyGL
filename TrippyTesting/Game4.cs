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
        UniformBufferSubset<ThreeMat4> uniformSubset;
        VertexDataBufferSubset<Vector3> positionSubset;
        VertexDataBufferSubset<Color4b> colorSubset;
        VertexDataBufferSubset<Vector2> texcoordSubset;

        VertexArray array;

        VertexBuffer<VertexColorTexture> vertexbuffer;

        Texture2D texture, whitepx;

        GraphicsDevice graphicsDevice;

        public Game4() : base(1280, 720, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 0, 0, 8, ColorFormat.Empty, 2), "haha yes", GameWindowFlags.Default, DisplayDevice.Default, 4, 0, GraphicsContextFlags.Default)
        {
            VSync = VSyncMode.On;
            graphicsDevice = new GraphicsDevice(Context);
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

            texture = new Texture2D(graphicsDevice, "data4/jeru.png");
            whitepx = new Texture2D(graphicsDevice, 1, 1);
            whitepx.SetData(new Color4b[] { Color4b.White });

            program = new ShaderProgram(graphicsDevice);
            program.AddVertexShader(File.ReadAllText("data4/vs.glsl"));
            program.AddFragmentShader(File.ReadAllText("data4/fs.glsl"));
            program.SpecifyVertexAttribs<VertexColorTexture>(new string[] { "vPosition", "vColor", "vTexCoords" });
            program.LinkProgram();

            Vector3[] vertexPositions = new Vector3[]
            {
                new Vector3(-0.5f, -0.5f, 0),
                new Vector3(0, 0.5f, 0),
                new Vector3(0.5f, -0.5f, 0)
            };

            Color4b[] vertexColors = new Color4b[]
            {
                new Color4b(255, 0, 0, 255),
                new Color4b(0, 255, 0, 255),
                new Color4b(0, 0, 255, 255)
            };

            Vector2[] vertexTexCoords = new Vector2[]
            {
                new Vector2(0, 1),
                new Vector2(0.5f, 0),
                new Vector2(1, 1),
            };

            int uboSizeBytes = UniformBufferSubset<ThreeMat4>.CalculateRequiredSizeInBytes(graphicsDevice, 1);
            buffer = new BufferObject(graphicsDevice, uboSizeBytes + vertexPositions.Length * VertexColorTexture.SizeInBytes, BufferUsageHint.StaticDraw);
            positionSubset = new VertexDataBufferSubset<Vector3>(buffer, vertexPositions, 0, uboSizeBytes, vertexPositions.Length);
            colorSubset = new VertexDataBufferSubset<Color4b>(buffer, vertexColors, 0, positionSubset.StorageNextInBytes, vertexColors.Length);
            texcoordSubset = new VertexDataBufferSubset<Vector2>(buffer, vertexTexCoords, 0, colorSubset.StorageNextInBytes, vertexTexCoords.Length);

            array = new VertexArray(graphicsDevice, new VertexAttribSource[]
            {
                new VertexAttribSource(positionSubset, ActiveAttribType.FloatVec3),
                new VertexAttribSource(colorSubset, ActiveAttribType.FloatVec4, true, VertexAttribPointerType.UnsignedByte),
                new VertexAttribSource(texcoordSubset, ActiveAttribType.FloatVec2)
            });

            uniformSubset = new UniformBufferSubset<ThreeMat4>(buffer, 0, 1);
            ThreeMat4 m = new ThreeMat4();
            m.World = Matrix4.Identity;
            m.View = Matrix4.Identity;
            m.Projection = Matrix4.Identity;
            uniformSubset.SetValue(ref m);
            program.BlockUniforms["MatrixBlock"].SetValue(uniformSubset);

            VertexColorTexture[] vertex = new VertexColorTexture[]
            {
                new VertexColorTexture(new Vector3(-0.8f, -0.8f, 0), new Color4b(255, 0, 0, 255), new Vector2(0, 0)),
                new VertexColorTexture(new Vector3(0.8f, -0.8f, 0), new Color4b(0, 255, 0, 255), new Vector2(0, 0)),
                new VertexColorTexture(new Vector3(-0.8f, 0.8f, 0), new Color4b(0, 0, 255, 255), new Vector2(0, 0)),
                new VertexColorTexture(new Vector3(0.8f, 0.8f, 0), new Color4b(255, 0, 255, 255), new Vector2(0, 0))
            };

            vertexbuffer = new VertexBuffer<VertexColorTexture>(graphicsDevice, vertex, BufferUsageHint.DynamicDraw);

            VertexDataBufferSubset<float> allVertexSubset = new VertexDataBufferSubset<float>(buffer, uboSizeBytes, VertexColorTexture.SizeInBytes * vertexPositions.Length / 4);

            float[] data = new float[allVertexSubset.StorageLength];
            allVertexSubset.GetData(data);
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
            graphicsDevice.BlendState = BlendState.AlphaBlend;
            graphicsDevice.DepthTestingEnabled = false;

            GL.Clear(ClearBufferMask.ColorBufferBit);

            graphicsDevice.BindVertexArray(vertexbuffer.VertexArray);
            program.Uniforms["samp"].SetValueTexture(whitepx);
            program.EnsurePreDrawStates();
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, vertexbuffer.StorageLength);

            graphicsDevice.BindVertexArray(array);
            program.Uniforms["samp"].SetValueTexture(texture);
            program.EnsurePreDrawStates();
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, positionSubset.StorageLength);

            SwapBuffers();
        }

        protected override void OnUnload(EventArgs e)
        {
            program.Dispose();
            buffer.Dispose();
            array.Dispose();
            vertexbuffer.Dispose();
            texture.Dispose();
            whitepx.Dispose();

            graphicsDevice.Dispose();
        }

        protected override void OnResize(EventArgs e)
        {
            graphicsDevice.Viewport = new Rectangle(0, 0, Width, Height);
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
