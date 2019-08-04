using System;
using System.IO;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using TrippyGL;

namespace TrippyTesting.Tests
{
    class InstancedRendering : GameWindow
    {
        const int MaxParticles = 4096;

        System.Diagnostics.Stopwatch stopwatch;
        public static Random r = new Random();
        public static float time, deltaTime;

        BufferObject ptcBuffer;
        VertexDataBufferSubset<Matrix4> matSubset;
        VertexDataBufferSubset<VertexColor> vertexSubset;
        VertexArray ptcArray;

        Particle[] particles;

        ShaderProgram ptcProgram;

        GraphicsDevice graphicsDevice;

        Vector2 mouseWindowPosition;
        MouseState ms, oldMs;
        KeyboardState ks, oldKs;

        public InstancedRendering() : base(1280, 720, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 32, 0, 0, ColorFormat.Empty, 2), "haha yes", GameWindowFlags.Default, DisplayDevice.Default, 4, 0, GraphicsContextFlags.Default)
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

            const int maxvert = 128;
            VertexColor[] vertices = new VertexColor[maxvert+1];
            vertices[0] = new VertexColor(new Vector3(0, 0, 0), randomCol());
            for (int i = 0; i < maxvert; i++)
            {
                float rot = i * MathHelper.TwoPi / maxvert;
                float scale = (i * 10f / maxvert) % 0.5f + 0.5f;
                scale = 3f * scale * scale - 2f * scale * scale * scale;
                vertices[i+1] = new VertexColor(new Vector3((float)Math.Cos(rot) * scale, (float)Math.Sin(rot) * scale, 0f), Color4b.Multiply(randomCol(), 0.1f));
            }

            ptcBuffer = new BufferObject(graphicsDevice, MaxParticles * 64 + VertexColor.SizeInBytes * vertices.Length, BufferUsageHint.StaticDraw);
            matSubset = new VertexDataBufferSubset<Matrix4>(ptcBuffer, 0, MaxParticles);
            vertexSubset = new VertexDataBufferSubset<VertexColor>(ptcBuffer, vertices, 0, matSubset.StorageNextInBytes, vertices.Length);

            ptcArray = new VertexArray(graphicsDevice, new VertexAttribSource[]
            {
                new VertexAttribSource(matSubset, ActiveAttribType.FloatMat4, 1),
                new VertexAttribSource(vertexSubset, ActiveAttribType.FloatVec3),
                new VertexAttribSource(vertexSubset, ActiveAttribType.FloatVec4, true, VertexAttribPointerType.UnsignedByte)
            });

            ptcProgram = new ShaderProgram(graphicsDevice);
            ptcProgram.AddVertexShader(File.ReadAllText("instanced/vs.glsl"));
            ptcProgram.AddFragmentShader(File.ReadAllText("instanced/fs.glsl"));
            ptcProgram.SpecifyVertexAttribs(ptcArray.AttribSources, new string[] { "World", "vPosition", "vColor" });
            ptcProgram.LinkProgram();

            particles = new Particle[MaxParticles];
            for (int i = 0; i < MaxParticles; i++)
                particles[i] = new Particle();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            oldMs = ms;
            ms = Mouse.GetState();
            oldKs = ks;
            ks = Keyboard.GetState();

            float prevTime = time;
            time = (float)stopwatch.Elapsed.TotalSeconds;
            deltaTime = time - prevTime;
            ErrorCode c;
            while ((c = GL.GetError()) != ErrorCode.NoError)
            {
                Console.WriteLine("Error found: " + c);
            }

            Vector2 mousePos = new Vector2(mouseWindowPosition.X, Height - mouseWindowPosition.Y) * 128f / (float)Width;

            if (ks.IsKeyDown(Key.Space) && oldKs.IsKeyUp(Key.Space))
            {
                for (int i = 0; i < particles.Length; i++)
                    particles[i].Reset(mousePos);
            }

            for (int i = 0; i < particles.Length; i++)
                particles[i].Update(mousePos);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            graphicsDevice.BlendState = BlendState.Additive;
            graphicsDevice.DepthTestingEnabled = false;
            graphicsDevice.ClearColor = new Color4(0f, 0f, 0f, 1f);
            graphicsDevice.Framebuffer = null;

            Matrix4[] mats = new Matrix4[particles.Length]; // this line makes the GC cry :(
            for (int i = 0; i < particles.Length; i++)
                mats[i] = particles[i].GenerateMatrix();
            matSubset.SetData(mats);

            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            graphicsDevice.VertexArray = ptcArray;
            ptcProgram.Uniforms["time"].SetValue1(time);
            graphicsDevice.ShaderProgram = ptcProgram;
            //matSubset.SetData(new Matrix4[] { Matrix4.CreateScale(24f) * Matrix4.CreateRotationZ(time) *  Matrix4.CreateTranslation(64, 24, 0) });
            //graphicsDevice.DrawArrays(PrimitiveType.TriangleFan, 0, vertexSubset.StorageLength);
            graphicsDevice.DrawArraysInstanced(PrimitiveType.TriangleFan, 0, vertexSubset.StorageLength, particles.Length);

            SwapBuffers();
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            mouseWindowPosition.X = e.Mouse.X;
            mouseWindowPosition.Y = e.Mouse.Y;
        }

        protected override void OnResize(EventArgs e)
        {
            graphicsDevice.Viewport = new Rectangle(0, 0, Width, Height);

            float ratio = (float)Width / (float)Height;

            Matrix4 view = Matrix4.Identity;
            ptcProgram.Uniforms["View"].SetValueMat4(ref view);

            Matrix4 proj = Matrix4.CreateOrthographicOffCenter(0, 128, 0, 128f / ratio, 0, 1);
            ptcProgram.Uniforms["Projection"].SetValueMat4(ref proj);
        }

        protected override void OnUnload(EventArgs e)
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
                    if (position.X < 0 || position.X > 128-scale)
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

            public Matrix4 GenerateMatrix()
            {
                return Matrix4.CreateScale(scale) * Matrix4.CreateRotationZ(rotation) * Matrix4.CreateTranslation(position.X, position.Y, 0);
            }
        }
    }
}
