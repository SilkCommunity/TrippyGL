using System;
using System.IO;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using TrippyGL;

namespace TrippyTesting.Tests
{
    class Test3DBatcher : GameWindow
    {
        System.Diagnostics.Stopwatch stopwatch;
        public static Random r = new Random();
        public static float time, deltaTime;

        ShaderProgram program;

        PrimitiveBatcher<VertexColor> batcher;
        VertexBuffer<VertexColor> triangleBuffer, lineBuffer;

        TextureCubemap cubemap;
        ShaderProgram cubemapProgram;
        VertexBuffer<VertexPosition> cubemapBuffer;

        VertexBuffer<VertexColorTexture> texBuffer;
        ShaderProgram texProgram;

        FramebufferObject fbo1, fbo2;
        Texture2D tex1, tex2;

        GraphicsDevice graphicsDevice;

        bool isMouseDown;
        Vector3 cameraPos;
        float rotY, rotX;

        MouseState ms, oldMs;
        KeyboardState ks, oldKs;

        public Test3DBatcher() : base(1280, 720, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 0, 0, 0, ColorFormat.Empty, 2), "haha yes", GameWindowFlags.Default, DisplayDevice.Default, 4, 0, GraphicsContextFlags.Default)
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

            cameraPos = new Vector3(-3.6f, 2.4f, -3f);
            rotY = 0.5f;
            rotX = -0.4f;

            program = new ShaderProgram(graphicsDevice);
            program.AddVertexShader(File.ReadAllText("3dbatcher/3dvs.glsl"));
            program.AddFragmentShader(File.ReadAllText("3dbatcher/3dfs.glsl"));
            program.SpecifyVertexAttribs<VertexColor>(new string[] { "vPosition", "vColor" });
            program.LinkProgram();

            Matrix4 id = Matrix4.Identity;
            program.Uniforms["World"].SetValueMat4(ref id);
            program.Uniforms["View"].SetValueMat4(ref id);
            program.Uniforms["Projection"].SetValueMat4(ref id);

            batcher = new PrimitiveBatcher<VertexColor>(512, 128);
            triangleBuffer = new VertexBuffer<VertexColor>(graphicsDevice, batcher.TriangleVertexCapacity, BufferUsageHint.StreamDraw);
            lineBuffer = new VertexBuffer<VertexColor>(graphicsDevice, batcher.LineVertexCapacity, BufferUsageHint.StreamDraw);

            cubemapProgram = new ShaderProgram(graphicsDevice);
            cubemapProgram.AddVertexShader(File.ReadAllText("cubemap/vs.glsl"));
            cubemapProgram.AddFragmentShader(File.ReadAllText("cubemap/fs.glsl"));
            cubemapProgram.SpecifyVertexAttribs<VertexPosition>(new string[] { "vPosition" });
            cubemapProgram.LinkProgram();
            
            cubemapBuffer = new VertexBuffer<VertexPosition>(graphicsDevice, new VertexPosition[]{
                new VertexPosition(new Vector3(-0.5f,-0.5f,-0.5f)),//4
                new VertexPosition(new Vector3(-0.5f,-0.5f,0.5f)),//3
                new VertexPosition(new Vector3(-0.5f,0.5f,-0.5f)),//7
                new VertexPosition(new Vector3(-0.5f,0.5f,0.5f)),//8
                new VertexPosition(new Vector3(0.5f,0.5f,0.5f)),//5
                new VertexPosition(new Vector3(-0.5f,-0.5f,0.5f)),//3
                new VertexPosition(new Vector3(0.5f,-0.5f,0.5f)),//1
                new VertexPosition(new Vector3(-0.5f,-0.5f,-0.5f)),//4
                new VertexPosition(new Vector3(0.5f,-0.5f,-0.5f)),//2
                new VertexPosition(new Vector3(-0.5f,0.5f,-0.5f)),//7
                new VertexPosition(new Vector3(0.5f,0.5f,-0.5f)),//6
                new VertexPosition(new Vector3(0.5f,0.5f,0.5f)),//5
                new VertexPosition(new Vector3(0.5f,-0.5f,-0.5f)),//2
                new VertexPosition(new Vector3(0.5f,-0.5f,0.5f)),//1
            }, BufferUsageHint.StaticDraw);

            cubemap = new TextureCubemap(graphicsDevice, 800);
            cubemap.SetData(CubeMapFace.PositiveX, "cubemap/cubemap1_front.png");
            cubemap.SetData(CubeMapFace.NegativeX, "cubemap/cubemap1_back.png");
            cubemap.SetData(CubeMapFace.NegativeZ, "cubemap/cubemap1_left.png");
            cubemap.SetData(CubeMapFace.PositiveZ, "cubemap/cubemap1_right.png");
            cubemap.SetData(CubeMapFace.PositiveY, "cubemap/cubemap1_top.png");
            cubemap.SetData(CubeMapFace.NegativeY, "cubemap/cubemap1_bottom.png");
            cubemap.SetTextureFilters(TextureMinFilter.Linear, TextureMagFilter.Linear);
            cubemapProgram.Uniforms["samp"].SetValueTexture(cubemap);

            texProgram = new ShaderProgram(graphicsDevice);
            texProgram.AddVertexShader(File.ReadAllText("3dbatcher/simplevs.glsl"));
            texProgram.AddFragmentShader(File.ReadAllText("3dbatcher/simplefs.glsl"));
            texProgram.SpecifyVertexAttribs<VertexColorTexture>(new string[] { "vPosition", "vColor", "vTexCoords" });
            texProgram.LinkProgram();
            id = Matrix4.Identity;
            texProgram.Uniforms["World"].SetValueMat4(ref id);

            texBuffer = new VertexBuffer<VertexColorTexture>(graphicsDevice, new VertexColorTexture[] {
                new VertexColorTexture(new Vector3(-0.5f, -0.5f, 0), new Color4b(255, 255, 255, 255), new Vector2(0, 0)),
                new VertexColorTexture(new Vector3(-0.5f, 0.5f, 0), new Color4b(255, 255, 255, 255), new Vector2(0, 1)),
                new VertexColorTexture(new Vector3(0.5f, -0.5f, 0), new Color4b(255, 255, 255, 255), new Vector2(1, 0)),
                new VertexColorTexture(new Vector3(0.5f, 0.5f, 0), new Color4b(255, 255, 255, 255), new Vector2(1, 1)),
            }, BufferUsageHint.StaticDraw);

            tex1 = null;
            tex2 = null;
            fbo1 = FramebufferObject.Create2D(ref tex1, graphicsDevice, this.Width, this.Height, DepthStencilFormat.Depth24Stencil8);
            fbo2 = FramebufferObject.Create2D(ref tex2, graphicsDevice, this.Width, this.Height, DepthStencilFormat.Depth24);
            tex1.SetTextureFilters(TextureMinFilter.Linear, TextureMagFilter.Linear);
            tex2.SetTextureFilters(TextureMinFilter.Linear, TextureMagFilter.Linear);
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

            const float CameraMoveSpeed = 5;
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
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            graphicsDevice.BlendingEnabled = false;
            graphicsDevice.DepthState = DepthTestingState.Default;
            graphicsDevice.ClearColor = Color4.Black;
            graphicsDevice.TextureCubemapSeamlessEnabled = true;
            graphicsDevice.DrawFramebuffer = fbo1;

            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4 mat = Matrix4.LookAt(cameraPos, cameraPos + new Vector3((float)Math.Cos(rotY), (float)Math.Tan(rotX), (float)Math.Sin(rotY)), Vector3.UnitY);
            program.Uniforms["View"].SetValueMat4(ref mat);
            cubemapProgram.Uniforms["View"].SetValueMat4(ref mat);
            texProgram.Uniforms["View"].SetValueMat4(ref mat);

            graphicsDevice.VertexArray = cubemapBuffer.VertexArray;
            cubemapProgram.Uniforms["cameraPos"].SetValue3(ref cameraPos);
            cubemapProgram.EnsurePreDrawStates();
            graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, cubemapBuffer.StorageLength);
            graphicsDevice.Clear(ClearBufferMask.DepthBufferBit);

            batcher.AddLine(new VertexColor(new Vector3(cameraPos.X - 100, 0, 0), new Color4b(255, 0, 0, 255)), new VertexColor(new Vector3(cameraPos.X + 100, 0, 0), new Color4b(255, 0, 0, 255)));
            batcher.AddLine(new VertexColor(new Vector3(0, cameraPos.Y - 100, 0), new Color4b(0, 255, 0, 255)), new VertexColor(new Vector3(0, cameraPos.Y + 100, 0), new Color4b(0, 255, 0, 255)));
            batcher.AddLine(new VertexColor(new Vector3(0, 0, cameraPos.Z - 100), new Color4b(0, 0, 255, 255)), new VertexColor(new Vector3(0, 0, cameraPos.Z + 100), new Color4b(0, 0, 255, 255)));

            Vector3 linecent = new Vector3((int)cameraPos.X, 0, (int)cameraPos.Z);
            for (int i = -15; i <= 15; i++)
            {
                const byte col = 64;
                if (i + linecent.Z != 0)
                    batcher.AddLine(new VertexColor(new Vector3(-100, 0, i) + linecent, new Color4b(col, 0, 0, 255)), new VertexColor(new Vector3(100, 0, i) + linecent, new Color4b(col, 0, 0, 255)));
                if (i + linecent.X != 0)
                    batcher.AddLine(new VertexColor(new Vector3(i, 0, -100) + linecent, new Color4b(0, 0, col, 255)), new VertexColor(new Vector3(i, 0, 100) + linecent, new Color4b(0, 0, col, 255)));
            }

            Vector3 forward = new Vector3((float)Math.Cos(rotY) * (float)Math.Cos(rotX), (float)Math.Sin(rotX), (float)Math.Sin(rotY) * (float)Math.Cos(rotX));
            Vector3 center = cameraPos + forward * (Math.Max(ms.Scroll.Y, 0f) * 0.1f + 1f);
            batcher.AddLine(new VertexColor(center, new Color4b(255, 0, 0, 255)), new VertexColor(new Vector3(0.25f, 0, 0) + center, new Color4b(255, 0, 0, 255)));
            batcher.AddLine(new VertexColor(center, new Color4b(0, 255, 0, 255)), new VertexColor(new Vector3(0, 0.25f, 0) + center, new Color4b(0, 255, 0, 255)));
            batcher.AddLine(new VertexColor(center, new Color4b(0, 0, 255, 255)), new VertexColor(new Vector3(0, 0, 0.25f) + center, new Color4b(0, 0, 255, 255)));

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

            mat = Matrix4.CreateRotationY(time * MathHelper.Pi);
            batcher.AddTriangleStrip(MultiplyAllToNew(cube, ref mat));

            mat = Matrix4.CreateRotationY(time * MathHelper.PiOver2) * Matrix4.CreateScale(0.6f, 1.5f, 0.6f) * Matrix4.CreateTranslation(2, 0, -1.4f);
            batcher.AddTriangleStrip(MultiplyAllToNew(cone, ref mat));

            mat = Matrix4.CreateRotationY(-time * MathHelper.PiOver2) * Matrix4.CreateScale(0.6f, 1.5f, 0.6f) * Matrix4.CreateTranslation(-1.4f, 0, 2);
            batcher.AddTriangleStrip(MultiplyAllToNew(cone, ref mat));

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


            program.EnsurePreDrawStates();
            if (batcher.TriangleVertexCount > triangleBuffer.StorageLength)
                triangleBuffer.RecreateStorage(batcher.TriangleVertexCapacity);
            batcher.WriteTrianglesTo(triangleBuffer.DataSubset);
            graphicsDevice.VertexArray = triangleBuffer.VertexArray;
            graphicsDevice.DrawArrays(PrimitiveType.Triangles, 0, batcher.TriangleVertexCount);
            batcher.ClearTriangles();

            if (batcher.LineVertexCount > lineBuffer.StorageLength)
                lineBuffer.RecreateStorage(batcher.LineVertexCapacity);
            batcher.WriteLinesTo(lineBuffer.DataSubset);
            graphicsDevice.VertexArray = lineBuffer.VertexArray;
            graphicsDevice.DrawArrays(PrimitiveType.Lines, 0, batcher.LineVertexCount);
            batcher.ClearLines();

            float ratio = (float)this.Width / (float)this.Height;
            mat = Matrix4.CreateScale(-ratio * 10f, 10f, 1f) * Matrix4.CreateTranslation(2.5f, 2, 12);
            texProgram.Uniforms["World"].SetValueMat4(ref mat);
            texProgram.Uniforms["samp"].SetValueTexture(tex2);
            texProgram.EnsurePreDrawStates();
            graphicsDevice.VertexArray = texBuffer.VertexArray;
            graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

            graphicsDevice.BlitFramebuffer(fbo1, null, new Rectangle(0, 0, this.Width, this.Height), new Rectangle(0, 0, this.Width, this.Height), ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);

            FramebufferObject fbotmp = fbo1;
            fbo1 = fbo2;
            fbo2 = fbotmp;
            Texture2D textmp = tex1;
            tex1 = tex2;
            tex2 = textmp;

            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            graphicsDevice.Viewport = new Rectangle(0, 0, this.Width, this.Height);

            float wid = this.Width / (float)this.Height;
            wid *= 0.5f;
            Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver2, this.Width / (float)this.Height, 0.0001f, 100f);
            program.Uniforms["Projection"].SetValueMat4(ref proj);
            cubemapProgram.Uniforms["Projection"].SetValueMat4(ref proj);
            texProgram.Uniforms["Projection"].SetValueMat4(ref proj);

            FramebufferObject.Resize2D(fbo1, this.Width, this.Height);
            FramebufferObject.Resize2D(fbo2, this.Width, this.Height);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Left)
                isMouseDown = true;
            else if (e.Button == MouseButton.Right)
            {
                Vector3 forward = new Vector3((float)Math.Cos(rotY) * (float)Math.Cos(rotX), (float)Math.Sin(rotX), (float)Math.Sin(rotY) * (float)Math.Cos(rotX));
                Vector3 center = cameraPos + forward * MathHelper.Clamp(ms.Scroll.Y * 0.1f, 0.1f, 50f);
                //fuckables.Add(new Fuckable(getRandMesh(), center));
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Left)
                isMouseDown = false;
        }

        protected override void OnUnload(EventArgs e)
        {
            triangleBuffer.Dispose();
            lineBuffer.Dispose();
            program.Dispose();

            cubemap.Dispose();
            cubemapProgram.Dispose();
            cubemapBuffer.Dispose();

            texProgram.Dispose();
            texBuffer.Dispose();
            fbo1.DisposeAttachments();
            fbo1.Dispose();
            fbo2.DisposeAttachments();
            fbo2.Dispose();

            graphicsDevice.Dispose();
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

    }
}
