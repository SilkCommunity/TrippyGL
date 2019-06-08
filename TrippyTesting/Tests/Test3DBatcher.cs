using System;
using System.IO;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using System.Drawing.Imaging;
using TrippyGL;

namespace TrippyTesting.Tests
{
    class Test3DBatcher : GameWindow
    {
        const float CameraMoveSpeed = 5;

        System.Diagnostics.Stopwatch stopwatch;
        public static Random r = new Random();
        public static float time, deltaTime;

        GraphicsDevice graphicsDevice;

        ShaderProgram program;

        VertexDataBufferObject<VertexColor> triangleBuffer, lineBuffer;
        VertexArray triangleArray, lineArray;

        UniformBufferObject<ThreeMat4> ubo;
        ThreeMat4 uniformVal;

        bool isMouseDown;
        Vector3 cameraPos;
        float rotY, rotX;

        PrimitiveBatcher<VertexColor> batcher;

        System.Collections.Generic.List<Fuckable> fuckables;

        MouseState ms, oldMs;
        KeyboardState ks, oldKs;

        public Test3DBatcher() : base(1280, 720, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 0, 0, ColorFormat.Empty, 2), "3D FUCKSAAA LO PIBE", GameWindowFlags.Default, DisplayDevice.Default, 4, 0, GraphicsContextFlags.Default)
        {
            VSync = VSyncMode.On;
            graphicsDevice = new GraphicsDevice(this.Context);

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
            fuckables = new System.Collections.Generic.List<Fuckable>(32);

            cameraPos = new Vector3(2, 1.3f, 2);
            rotY = 0;
            rotX = 0;

            triangleBuffer = new VertexDataBufferObject<VertexColor>(graphicsDevice, 512, BufferUsageHint.StreamDraw);
            lineBuffer = new VertexDataBufferObject<VertexColor>(graphicsDevice, 512, BufferUsageHint.StreamDraw);
            triangleArray = VertexArray.CreateSingleBuffer<VertexColor>(graphicsDevice, triangleBuffer);
            lineArray = VertexArray.CreateSingleBuffer<VertexColor>(graphicsDevice, lineBuffer);

            program = new ShaderProgram(graphicsDevice);
            program.AddVertexShader(File.ReadAllText("data3/vs.glsl"));
            program.AddFragmentShader(File.ReadAllText("data3/fs.glsl"));
            program.SpecifyVertexAttribs<VertexColor>(new string[] { "vPosition", "vColor" });
            program.LinkProgram();
            Console.WriteLine(GL.GetError());

            Matrix4 mat = Matrix4.Identity;
            uniformVal.World = mat;
            uniformVal.View = mat;
            uniformVal.Projection = mat;

            ubo = new UniformBufferObject<ThreeMat4>(graphicsDevice, uniformVal, BufferUsageHint.StreamDraw);

            batcher = new PrimitiveBatcher<VertexColor>(512, 512);
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

            if (ks.IsKeyDown(Key.Left))
                cameraPos.X -= CameraMoveSpeed * deltaTime;
            if (ks.IsKeyDown(Key.Right))
                cameraPos.X += CameraMoveSpeed * deltaTime;
            if (ks.IsKeyDown(Key.LShift) || ks.IsKeyDown(Key.RShift))
            {
                if (ks.IsKeyDown(Key.Up))
                    cameraPos.Z += CameraMoveSpeed * deltaTime;
                if (ks.IsKeyDown(Key.Down))
                    cameraPos.Z -= CameraMoveSpeed * deltaTime;
            }
            else
            {
                if (ks.IsKeyDown(Key.Up))
                    cameraPos.Y += CameraMoveSpeed * deltaTime;
                if (ks.IsKeyDown(Key.Down))
                    cameraPos.Y -= CameraMoveSpeed * deltaTime;
            }

            if (ks.IsKeyDown(Key.R))
                cameraPos.Y += CameraMoveSpeed * deltaTime;
            if (ks.IsKeyDown(Key.F))
                cameraPos.Y -= CameraMoveSpeed * deltaTime;

            float jejeX = ks.IsKeyDown(Key.LShift) || ks.IsKeyDown(Key.RShift) ? rotX : 0;
            if (ks.IsKeyDown(Key.W))
                cameraPos += new Vector3((float)(Math.Cos(rotY) * Math.Cos(jejeX)), (float)Math.Sin(jejeX), (float)(Math.Sin(rotY) * Math.Cos(jejeX))) * CameraMoveSpeed * deltaTime;
            if (ks.IsKeyDown(Key.S))
                cameraPos -= new Vector3((float)(Math.Cos(rotY) * Math.Cos(jejeX)), (float)Math.Sin(jejeX), (float)(Math.Sin(rotY) * Math.Cos(jejeX))) * CameraMoveSpeed * deltaTime;

            if (ks.IsKeyDown(Key.A))
                cameraPos += new Vector3((float)(Math.Sin(rotY) * Math.Cos(jejeX)), (float)Math.Sin(jejeX), (float)(Math.Cos(rotY) * -Math.Cos(jejeX))) * CameraMoveSpeed * deltaTime;
            if (ks.IsKeyDown(Key.D))
                cameraPos -= new Vector3((float)(Math.Sin(rotY) * Math.Cos(jejeX)), -(float)Math.Sin(jejeX), (float)(Math.Cos(rotY) * -Math.Cos(jejeX))) * CameraMoveSpeed * deltaTime;

            if (this.WindowState != WindowState.Minimized && isMouseDown)
            {
                rotY += (ms.X - oldMs.X) * 0.005f;
                rotX += (ms.Y - oldMs.Y) * -0.005f;
                rotX = MathHelper.Clamp(rotX, -1.57f, 1.57f);
                Mouse.SetPosition(this.Width / 2f + this.X, this.Height / 2f + this.Y);

            }

            //Console.WriteLine("rotX= " + rotX + "; rotY=" + rotY);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.ClearDepth(1f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            uniformVal.View = Matrix4.LookAt(cameraPos, cameraPos + new Vector3((float)Math.Cos(rotY), (float)Math.Tan(rotX), (float)Math.Sin(rotY)), Vector3.UnitY);
            /*VertexColor[] cube = new VertexColor[]{
                new VertexColor(new Vector3(0,0,0), Color4b.LightBlue),//4
                new VertexColor(new Vector3(0,0,1), Color4b.Lime),//3
                new VertexColor(new Vector3(0,1,0), Color4b.White),//7
                new VertexColor(new Vector3(0,1,1), Color4b.Black),//8
                new VertexColor(new Vector3(1,1,1), Color4b.Blue),//5
                new VertexColor(new Vector3(0,0,1), Color4b.Lime),//3
                new VertexColor(new Vector3(1,0,1), Color4b.Red),//1
                new VertexColor(new Vector3(0,0,0), Color4b.LightBlue),//4
                new VertexColor(new Vector3(1,0,0), Color4b.Yellow),//2
                new VertexColor(new Vector3(0,1,0), Color4b.White),//7
                new VertexColor(new Vector3(1,1,0), Color4b.Pink),//6
                new VertexColor(new Vector3(1,1,1), Color4b.Blue),//5
                new VertexColor(new Vector3(1,0,0), Color4b.Yellow),//2
                new VertexColor(new Vector3(1,0,1), Color4b.Red),//1
            };*/
            VertexColor[] cube = new VertexColor[]{
                new VertexColor(new Vector3(-0.5f,-0.5f,-0.5f), Color4b.LightBlue),//4
                new VertexColor(new Vector3(-0.5f,-0.5f,0.5f), Color4b.Lime),//3
                new VertexColor(new Vector3(-0.5f,0.5f,-0.5f), Color4b.White),//7
                new VertexColor(new Vector3(-0.5f,0.5f,0.5f), Color4b.Black),//8
                new VertexColor(new Vector3(0.5f,0.5f,0.5f), Color4b.Blue),//5
                new VertexColor(new Vector3(-0.5f,-0.5f,0.5f), Color4b.Lime),//3
                new VertexColor(new Vector3(0.5f,-0.5f,0.5f), Color4b.Red),//1
                new VertexColor(new Vector3(-0.5f,-0.5f,-0.5f), Color4b.LightBlue),//4
                new VertexColor(new Vector3(0.5f,-0.5f,-0.5f), Color4b.Yellow),//2
                new VertexColor(new Vector3(-0.5f,0.5f,-0.5f), Color4b.White),//7
                new VertexColor(new Vector3(0.5f,0.5f,-0.5f), Color4b.Pink),//6
                new VertexColor(new Vector3(0.5f,0.5f,0.5f), Color4b.Blue),//5
                new VertexColor(new Vector3(0.5f,-0.5f,-0.5f), Color4b.Yellow),//2
                new VertexColor(new Vector3(0.5f,-0.5f,0.5f), Color4b.Red),//1
            };
            Matrix4 mat = Matrix4.CreateRotationY(time * MathHelper.Pi);
            batcher.AddTriangleStrip(MultiplyAllToNew(cube, ref mat));

            VertexColor[] circleFan = new VertexColor[12];
            circleFan[0] = new VertexColor(new Vector3(0, 0, 0), new Color4b(0, 0, 0, 255));
            for (int i = 1; i < circleFan.Length - 1; i++)
            {
                float rot = (i - 1) * MathHelper.TwoPi / (circleFan.Length - 2);
                circleFan[i] = new VertexColor(new Vector3((float)Math.Cos(rot), 0, (float)Math.Sin(rot)), randomCol());
            }
            circleFan[circleFan.Length - 1] = circleFan[1];
            mat = Matrix4.CreateScale(1.5f) * Matrix4.CreateTranslation(0, -1f, 0);
            batcher.AddTriangleFan(MultiplyAllToNew(circleFan, ref mat));

            VertexColor[] cone = new VertexColor[]
            {
                new VertexColor(new Vector3(-1, 0, -1), new Color4b(255, 0, 0, 255)),
                new VertexColor(new Vector3(-1, 0, 1), new Color4b(0, 255, 0, 255)),
                new VertexColor(new Vector3(0, 1, 0), new Color4b(0, 0, 255, 255)),
                new VertexColor(new Vector3(1, 0, 1), new Color4b(255, 0, 0, 255)),
                new VertexColor(new Vector3(1, 0, -1), new Color4b(0, 255, 0, 255)),
                new VertexColor(new Vector3(-1, 0, -1), new Color4b(0, 0, 255, 255)),
                new VertexColor(new Vector3(0, 1, 0), new Color4b(0, 0, 255, 255)),
            };

            mat = Matrix4.CreateRotationY(time * MathHelper.PiOver2) * Matrix4.CreateScale(0.6f, 1.5f, 0.6f) * Matrix4.CreateTranslation(2, 0, -1.4f);
            batcher.AddTriangleStrip(MultiplyAllToNew(cone, ref mat));

            mat = Matrix4.CreateRotationY(-time * MathHelper.PiOver2) * Matrix4.CreateScale(0.6f, 1.5f, 0.6f) * Matrix4.CreateTranslation(-1.4f, 0, 2);
            batcher.AddTriangleStrip(MultiplyAllToNew(cone, ref mat));

            batcher.AddLine(new VertexColor(new Vector3(-9999, 0, 0), new Color4b(255, 0, 0, 255)), new VertexColor(new Vector3(9999, 0, 0), new Color4b(255, 0, 0, 255)));
            batcher.AddLine(new VertexColor(new Vector3(0, -9999, 0), new Color4b(0, 255, 0, 255)), new VertexColor(new Vector3(0, 9999, 0), new Color4b(0, 255, 0, 255)));
            batcher.AddLine(new VertexColor(new Vector3(0, 0, -9999), new Color4b(0, 0, 255, 255)), new VertexColor(new Vector3(0, 0, 9999), new Color4b(0, 0, 255, 255)));

            for (int i = 0; i < fuckables.Count; i++)
                fuckables[i].Draw(batcher);

            for (int i = -10; i < 11; i++)
            {
                bool hahayes = Math.Abs(cameraPos.X) > Math.Abs(cameraPos.Y);
                float meh = (int)(cameraPos.Z + 0.5f) + i;
                batcher.AddLine(new VertexColor(new Vector3(-9999, 0, meh), new Color4b(32, 0, 0, 255)), new VertexColor(new Vector3(9999, 0, meh), new Color4b(32, 0, 0, 255)));
                if (!hahayes)
                    batcher.AddLine(new VertexColor(new Vector3(0, -9999, meh), new Color4b(0, 32, 0, 255)), new VertexColor(new Vector3(0, 9999, meh), new Color4b(0, 32, 0, 255)));

                meh = (int)(cameraPos.X + 0.5f) + i;
                batcher.AddLine(new VertexColor(new Vector3(meh, 0, -9999), new Color4b(0, 0, 32, 255)), new VertexColor(new Vector3(meh, 0, 9999), new Color4b(0, 0, 32, 255)));
                if (hahayes)
                    batcher.AddLine(new VertexColor(new Vector3(meh, -9999, 0), new Color4b(0, 32, 0, 255)), new VertexColor(new Vector3(meh, 9999, 0), new Color4b(0, 32, 0, 255)));
            }

            Vector3 forward = new Vector3((float)Math.Cos(rotY) * (float)Math.Cos(rotX), (float)Math.Sin(rotX), (float)Math.Sin(rotY) * (float)Math.Cos(rotX));
            Vector3 center = cameraPos + forward * MathHelper.Clamp(ms.Scroll.Y * 0.1f, 0.1f, 50f);
            batcher.AddLine(new VertexColor(center, new Color4b(255, 0, 0, 255)), new VertexColor(new Vector3(1, 0, 0) + center, new Color4b(255, 0, 0, 255)));
            batcher.AddLine(new VertexColor(center, new Color4b(0, 255, 0, 255)), new VertexColor(new Vector3(0, 1, 0) + center, new Color4b(0, 255, 0, 255)));
            batcher.AddLine(new VertexColor(center, new Color4b(0, 0, 255, 255)), new VertexColor(new Vector3(0, 0, 1) + center, new Color4b(0, 0, 255, 255)));

            mat = Matrix4.CreateTranslation(2f, 2f, 2f);
            batcher.AddQuads(MultiplyAllToNew(new VertexColor[]{
                new VertexColor(Vector3.Zero, Color4b.White),
                new VertexColor(Vector3.Zero, Color4b.White),
                new VertexColor(Vector3.Zero, Color4b.White),

                new VertexColor(new Vector3(-1, -1, 0), new Color4b(255, 0, 0, 255)),
                new VertexColor(new Vector3(-1, 1, 0), new Color4b(0, 255, 0, 255)),
                new VertexColor(new Vector3(1, 1, 0), new Color4b(0, 0, 255, 255)),
                new VertexColor(new Vector3(1, -1, 0), new Color4b(255, 255, 0, 255)),

                new VertexColor(new Vector3(-1, -1, 1), new Color4b(255, 0, 0, 255)),
                new VertexColor(new Vector3(-1, 1, 1), new Color4b(0, 255, 0, 255)),
                new VertexColor(new Vector3(1, 1, 1), new Color4b(0, 0, 255, 255)),
                new VertexColor(new Vector3(1, -1, 1), new Color4b(255, 255, 0, 255)),

                new VertexColor(Vector3.Zero, Color4b.White),
                new VertexColor(Vector3.Zero, Color4b.White),
            }, ref mat), 3, 8);

            batcher.AddTriangleStrip(new VertexColor[]
            {
                new VertexColor(Vector3.Zero, Color4b.White),
                new VertexColor(Vector3.Zero, Color4b.White),

                new VertexColor(new Vector3(3, -3, -3), new Color4b(255, 0, 0, 255)),
                new VertexColor(new Vector3(3, -2, -3.5f), new Color4b(255, 255, 0, 255)),
                new VertexColor(new Vector3(2, -3, -3), new Color4b(255, 0, 255, 255)),
                new VertexColor(new Vector3(2, -2, -3), new Color4b(255, 0, 0, 255)),
                new VertexColor(new Vector3(1, -3, -3.5f), new Color4b(255, 255, 0, 255)),
                new VertexColor(new Vector3(1, -2, -3), new Color4b(255, 0, 255, 255)),
                new VertexColor(new Vector3(0, -3, -3), new Color4b(255, 255, 0, 255)),
                new VertexColor(new Vector3(0, -2, -3.5f), new Color4b(255, 0, 255, 255)),
                new VertexColor(new Vector3(-1, -3, -3), new Color4b(255, 255, 0, 255)),
                new VertexColor(new Vector3(-1, -2, -3), new Color4b(255, 0, 255, 255)),

                new VertexColor(Vector3.Zero, Color4b.White),
                new VertexColor(Vector3.Zero, Color4b.White),
            }, 2, 10);

            batcher.WriteTrianglesTo(triangleBuffer);
            batcher.WriteLinesTo(lineBuffer);
            ubo.SetValue(uniformVal);
            program.BlockUniforms["MatrixBlock"].SetValue(ubo);

            program.Uniforms["time"].SetValue1(time * 10f);
            program.Uniforms["amp"].SetValue1(0.2f);

            graphicsDevice.EnsureVertexArrayBound(lineArray);
            program.EnsurePreDrawStates();
            GL.DrawArrays(PrimitiveType.Lines, 0, batcher.LineVertexCount);

            graphicsDevice.EnsureVertexArrayBound(triangleArray);
            program.EnsurePreDrawStates();
            GL.DrawArrays(PrimitiveType.Triangles, 0, batcher.TriangleVertexCount);

            batcher.ClearTriangles();
            batcher.ClearLines();

            SwapBuffers();

            int slp = (int)(15f - (stopwatch.Elapsed.TotalSeconds - time) * 1000f);
            if (slp >= 0)
                System.Threading.Thread.Sleep(slp);
        }

        protected override void OnUnload(EventArgs e)
        {
            program.Dispose();
            triangleArray.Dispose();
            lineArray.Dispose();
            triangleBuffer.Dispose();
            lineBuffer.Dispose();

            graphicsDevice.Dispose();
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, this.Width, this.Height);

            float wid = this.Width / (float)this.Height;
            wid *= 0.5f;
            //Matrix4 mat = Matrix4.CreateOrthographicOffCenter(-wid, wid, 0, 1, 0, 100);
            Matrix4 mat = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver2, this.Width / (float)this.Height, 0.001f, 10f);
            //projUniform.SetValueMat4(ref mat);
            uniformVal.Projection = mat;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Left)
                isMouseDown = true;
            else if (e.Button == MouseButton.Right)
            {
                Vector3 forward = new Vector3((float)Math.Cos(rotY) * (float)Math.Cos(rotX), (float)Math.Sin(rotX), (float)Math.Sin(rotY) * (float)Math.Cos(rotX));
                Vector3 center = cameraPos + forward * MathHelper.Clamp(ms.Scroll.Y * 0.1f, 0.1f, 50f);
                fuckables.Add(new Fuckable(getRandMesh(), center));
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Left)
                isMouseDown = false;
        }

        public static VertexColor[] getRandMesh()
        {
            switch (r.Next(5))
            {
                case 0:
                    return new VertexColor[]
                    {
                        new VertexColor(new Vector3(-0.5f,-0.5f,-0.5f), Color4b.LightBlue),//4
                        new VertexColor(new Vector3(-0.5f,-0.5f,0.5f), Color4b.Lime),//3
                        new VertexColor(new Vector3(-0.5f,0.5f,-0.5f), Color4b.White),//7
                        new VertexColor(new Vector3(-0.5f,0.5f,0.5f), Color4b.Black),//8
                        new VertexColor(new Vector3(0.5f,0.5f,0.5f), Color4b.Blue),//5
                        new VertexColor(new Vector3(-0.5f,-0.5f,0.5f), Color4b.Lime),//3
                        new VertexColor(new Vector3(0.5f,-0.5f,0.5f), Color4b.Red),//1
                        new VertexColor(new Vector3(-0.5f,-0.5f,-0.5f), Color4b.LightBlue),//4
                        new VertexColor(new Vector3(0.5f,-0.5f,-0.5f), Color4b.Yellow),//2
                        new VertexColor(new Vector3(-0.5f,0.5f,-0.5f), Color4b.White),//7
                        new VertexColor(new Vector3(0.5f,0.5f,-0.5f), Color4b.Pink),//6
                        new VertexColor(new Vector3(0.5f,0.5f,0.5f), Color4b.Blue),//5
                        new VertexColor(new Vector3(0.5f,-0.5f,-0.5f), Color4b.Yellow),//2
                        new VertexColor(new Vector3(0.5f,-0.5f,0.5f), Color4b.Red),//1
                    };

                case 1:
                    return new VertexColor[]
                    {
                        new VertexColor(new Vector3(-1, 0, -1), new Color4b(255, 0, 0, 255)),
                        new VertexColor(new Vector3(-1, 0, 1), new Color4b(0, 255, 0, 255)),
                        new VertexColor(new Vector3(0, 1, 0), new Color4b(0, 0, 255, 255)),
                        new VertexColor(new Vector3(1, 0, 1), new Color4b(255, 0, 0, 255)),
                        new VertexColor(new Vector3(1, 0, -1), new Color4b(0, 255, 0, 255)),
                        new VertexColor(new Vector3(-1, 0, -1), new Color4b(0, 0, 255, 255)),
                        new VertexColor(new Vector3(0, 1, 0), new Color4b(0, 0, 255, 255)),
                    };
            }

            VertexColor[] arr = new VertexColor[r.Next(30) + 3];
            for (int i = 0; i < arr.Length; i++)
                arr[i] = new VertexColor(new Vector3(randomf(-1f, 1f), randomf(-1f, 1f), randomf(-1f, 1f)), randomCol());
            return arr;
        }

        public static VertexColor[] MultiplyAllToNew(VertexColor[] vertex, ref Matrix4 mat)
        {
            VertexColor[] arr = new VertexColor[vertex.Length];
            for (int i = 0; i < vertex.Length; i++)
            {
                Vector4 t = new Vector4(vertex[i].Position, 1f);
                Vector4.Transform(ref t, ref mat, out t);
                arr[i].Position = t.Xyz;
                arr[i].Color = vertex[i].Color;
            }
            return arr;
        }

        public static VertexColor[] SetColorAllToNew(VertexColor[] vertex, Color4b color)
        {
            VertexColor[] arr = new VertexColor[vertex.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i].Color = color;
                arr[i].Position = vertex[i].Position;
            }
            return arr;
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

        private class Fuckable
        {
            private VertexColor[] mesh;
            private Vector3 pos;
            private float inittime;

            public Fuckable(VertexColor[] mesh, Vector3 pos)
            {
                this.mesh = mesh;
                this.pos = pos;
                inittime = Test3DBatcher.time;
            }

            public void Draw(PrimitiveBatcher<VertexColor> batcher)
            {
                Matrix4 mat = Matrix4.CreateRotationY(Test3DBatcher.time) * Matrix4.CreateScale((float)Math.Sin(Test3DBatcher.time - inittime) * 0.3f + 1f) * Matrix4.CreateTranslation(pos);
                batcher.AddTriangleStrip(Test3DBatcher.MultiplyAllToNew(mesh, ref mat));
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

}
