using Silk.NET.Input;
using Silk.NET.Input.Common;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Common;
using System;
using System.IO;
using System.Numerics;
using TrippyGL;

namespace TrippyTesting.Tests
{
    class InstancedRendering
    {
        const int MaxParticles = 4096;

        System.Diagnostics.Stopwatch stopwatch;
        public static Random r = new Random();
        public static float time, deltaTime;
        readonly IWindow window;
        IInputContext inputContext;

        GraphicsDevice graphicsDevice;

        BufferObject ptcBuffer;
        VertexDataBufferSubset<Matrix4x4> matSubset;
        VertexDataBufferSubset<VertexColor> vertexSubset;
        VertexArray ptcArray;

        Vector2 mousePos;
        Particle[] particles;

        ShaderProgram ptcProgram;

        public InstancedRendering()
        {
            window = CreateWindow();

            window.Load += OnWindowLoad;
            window.Update += OnWindowUpdate;
            window.Render += OnWindowRender;
            window.Resize += OnWindowResized;
            window.Closing += OnWindowClosing;
        }

        private void InstancedRendering_MouseMove(IMouse sender, System.Drawing.PointF location)
        {
            //location = window.PointToClient(location);
            mousePos = new Vector2(location.X, window.Size.Height - location.Y) * 128f / window.Size.Width;
        }

        private void InstancedRendering_KeyDown(IKeyboard sender, Key key, int idk)
        {
            if (key == Key.Space)
            {
                for (int i = 0; i < particles.Length; i++)
                    particles[i].Reset(mousePos);
            }
        }

        private IWindow CreateWindow()
        {
            GraphicsAPI graphicsApi = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.Debug, new APIVersion(3, 3));
            VideoMode videoMode = new VideoMode(new System.Drawing.Size(1280, 720));
            ViewOptions viewOpts = new ViewOptions(true, 60.0, 60.0, graphicsApi, VSyncMode.On, 30, false, videoMode, 0);
            return Window.Create(new WindowOptions(viewOpts));
        }

        public void Run()
        {
            window.Run();
        }

        private void OnWindowLoad()
        {
            inputContext = window.CreateInput();

            inputContext.Mice[0].MouseMove += InstancedRendering_MouseMove;
            inputContext.Keyboards[0].KeyDown += InstancedRendering_KeyDown;

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

            const int maxvert = 128;
            VertexColor[] vertices = new VertexColor[maxvert + 1];
            vertices[0] = new VertexColor(new Vector3(0, 0, 0), randomCol());
            for (int i = 0; i < maxvert; i++)
            {
                float rot = i * MathF.PI * 2f / maxvert;
                float scale = i * 10f / maxvert % 0.5f + 0.5f;
                scale = 3f * scale * scale - 2f * scale * scale * scale;
                vertices[i + 1] = new VertexColor(new Vector3(MathF.Cos(rot) * scale, MathF.Sin(rot) * scale, 0f), Color4b.Multiply(randomCol(), 0.1f));
            }

            ptcBuffer = new BufferObject(graphicsDevice, (uint)(MaxParticles * 64 + VertexColor.SizeInBytes * vertices.Length), BufferUsageARB.StaticDraw);
            matSubset = new VertexDataBufferSubset<Matrix4x4>(ptcBuffer, 0, MaxParticles);
            vertexSubset = new VertexDataBufferSubset<VertexColor>(ptcBuffer, matSubset.StorageEndInBytes, (uint)vertices.Length, vertices);

            ptcArray = new VertexArray(graphicsDevice, new VertexAttribSource[]
            {
                new VertexAttribSource(matSubset, AttributeType.FloatMat4, 1),
                new VertexAttribSource(vertexSubset, AttributeType.FloatVec3),
                new VertexAttribSource(vertexSubset, AttributeType.FloatVec4, true, VertexAttribPointerType.UnsignedByte)
            });

            ShaderProgramBuilder programBuilder = new ShaderProgramBuilder();
            programBuilder.VertexShaderCode = File.ReadAllText("instanced/vs.glsl");
            programBuilder.FragmentShaderCode = File.ReadAllText("instanced/fs.glsl");
            programBuilder.SpecifyVertexAttribs(ptcArray.AttribSources, new string[] { "World", "vPosition", "vColor" });
            ptcProgram = programBuilder.Create(graphicsDevice, true);

            particles = new Particle[MaxParticles];
            for (int i = 0; i < MaxParticles; i++)
                particles[i] = new Particle();

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

            for (int i = 0; i < particles.Length; i++)
                particles[i].Update(mousePos);
        }

        private void OnWindowRender(double dtSeconds)
        {
            if (window.IsClosing)
                return;

            graphicsDevice.BlendState = BlendState.Additive;
            graphicsDevice.DepthTestingEnabled = false;
            graphicsDevice.ClearColor = new Vector4(0f, 0f, 0f, 1f);
            graphicsDevice.Framebuffer = null;

            Matrix4x4[] mats = new Matrix4x4[particles.Length]; // this line makes the GC cry :(
            for (int i = 0; i < particles.Length; i++)
                mats[i] = particles[i].GenerateMatrix();
            matSubset.SetData(mats);

            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            graphicsDevice.VertexArray = ptcArray;
            ptcProgram.Uniforms["time"].SetValueFloat(time);
            graphicsDevice.ShaderProgram = ptcProgram;
            //matSubset.SetData(new Matrix4[] { Matrix4.CreateScale(24f) * Matrix4.CreateRotationZ(time) *  Matrix4.CreateTranslation(64, 24, 0) });
            //graphicsDevice.DrawArrays(PrimitiveType.TriangleFan, 0, vertexSubset.StorageLength);
            graphicsDevice.DrawArraysInstanced(PrimitiveType.TriangleFan, 0, vertexSubset.StorageLength, (uint)particles.Length);


            window.SwapBuffers();
        }

        private void OnWindowResized(System.Drawing.Size size)
        {
            if (size.Width == 0 || size.Height == 0)
                return;

            graphicsDevice.SetViewport(0, 0, (uint)size.Width, (uint)size.Height);

            float ratio = size.Width / (float)size.Height;

            Matrix4x4 view = Matrix4x4.Identity;
            ptcProgram.Uniforms["View"].SetValueMat4(view);

            Matrix4x4 proj = Matrix4x4.CreateOrthographicOffCenter(0, 128, 0, 128f / ratio, 0, 1);
            ptcProgram.Uniforms["Projection"].SetValueMat4(proj);
        }

        private void OnWindowClosing()
        {
            graphicsDevice.Dispose();
        }

        public static Color4b randomCol()
        {
            return new Color4b((byte)r.Next(256), (byte)r.Next(256), (byte)r.Next(256), 255);
        }

        public static float randomf(float max)
        {
            return (float)r.NextDouble() * max;
        }

        public static float randomf(float min, float max)
        {
            return (float)r.NextDouble() * (max - min) + min;
        }

        private class Particle
        {
            Vector2 position;
            Vector2 direction;

            float rotation;
            float rotSpeed;

            float scale;
            float scaleSpeed;

            public Particle()
            {
                position = new Vector2(0, randomf(100));
                rotation = 0;
                scale = 1;
            }

            public void Update(Vector2 mousePos)
            {
                if (position.Y < -scale)
                {
                    Reset(mousePos);
                }
                else
                {
                    position += direction * deltaTime;
                    if (position.X < 0 || position.X > 128 - scale)
                        direction.X = -direction.X;
                    //if (position.Y < scale)
                    //    direction.Y *= -1f;

                    direction.Y -= deltaTime * 64f;
                    scale += scaleSpeed * deltaTime;
                    rotation += rotSpeed * deltaTime;
                }
            }

            public void Reset(Vector2 mousePos)
            {
                position = mousePos;
                direction.X = randomf(-40f, 40f);
                direction.Y = randomf(-6f, 70f);
                scale = randomf(1.3f, 2.2f);
                scaleSpeed = randomf(-0.75f, 0.5f);
                rotSpeed = randomf(-3f, 3f);
            }

            public Matrix4x4 GenerateMatrix()
            {
                return Matrix4x4.CreateScale(scale) * Matrix4x4.CreateRotationZ(rotation) * Matrix4x4.CreateTranslation(position.X, position.Y, 0);
            }
        }
    }
}
