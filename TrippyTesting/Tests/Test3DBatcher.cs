using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Common;
using System;
using System.IO;
using System.Numerics;
using TrippyGL;
using Silk.NET.Input;
using Silk.NET.Input.Common;

namespace TrippyTesting.Tests
{
    class Test3DBatcher
    {
        System.Diagnostics.Stopwatch stopwatch;
        public static Random r = new Random();
        public static float time, deltaTime;
        readonly IWindow window;
        IInputContext inputContext;

        GraphicsDevice graphicsDevice;

        SimpleShaderProgram batcherProgram;

        PrimitiveBatcher<VertexColor> batcher;
        VertexBuffer<VertexColor> triangleBuffer, lineBuffer;

        TextureCubemap cubemap;
        ShaderProgram cubemapProgram;
        VertexBuffer<VertexPosition> cubemapBuffer;

        VertexBuffer<VertexColorTexture> texBuffer;
        SimpleShaderProgram texProgram;

        Framebuffer2D fbo1, fbo2;

        Vector3 cameraPos;
        float rotY, rotX;

        public Test3DBatcher()
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
            inputContext = window.CreateInput();
            inputContext.Mice[0].MouseMove += Test3DBatcher_MouseMove;
            inputContext.Mice[0].MouseDown += Test3DBatcher_MouseDown;
            inputContext.Mice[0].MouseUp += Test3DBatcher_MouseUp;
            inputContext.Keyboards[0].KeyDown += Test3DBatcher_KeyDown;

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

            cameraPos = new Vector3(-3.6f, 2.4f, -3f);
            rotY = 0.5f;
            rotX = -0.4f;

            SimpleShaderProgramBuilder programBuilder = new SimpleShaderProgramBuilder()
            {
                VertexColorsEnabled = true
            };
            programBuilder.ConfigureVertexAttribs<VertexColor>();
            batcherProgram = programBuilder.Create(graphicsDevice, true);
            Console.WriteLine("VS Log: " + programBuilder.VertexShaderLog);
            Console.WriteLine("FS Log: " + programBuilder.FragmentShaderLog);
            Console.WriteLine("Program Log: " + programBuilder.ProgramLog);

            batcher = new PrimitiveBatcher<VertexColor>(512, 128);
            triangleBuffer = new VertexBuffer<VertexColor>(graphicsDevice, (uint)batcher.TriangleVertexCapacity, BufferUsageARB.StreamDraw);
            lineBuffer = new VertexBuffer<VertexColor>(graphicsDevice, (uint)batcher.LineVertexCapacity, BufferUsageARB.StreamDraw);

            ShaderProgramBuilder cProgramBuilder = new ShaderProgramBuilder();
            cProgramBuilder.VertexShaderCode = File.ReadAllText("cubemap/vs.glsl");
            cProgramBuilder.FragmentShaderCode = File.ReadAllText("cubemap/fs.glsl");
            cProgramBuilder.SpecifyVertexAttribs<VertexPosition>(new string[] { "vPosition" });
            cubemapProgram = cProgramBuilder.Create(graphicsDevice, true);
            Console.WriteLine("VS Log: " + cProgramBuilder.VertexShaderLog);
            Console.WriteLine("FS Log: " + cProgramBuilder.FragmentShaderLog);
            Console.WriteLine("Program Log: " + cProgramBuilder.ProgramLog);

            Span<VertexColorTexture> texBufferData = stackalloc VertexColorTexture[] {
                new VertexColorTexture(new Vector3(-0.5f, -0.5f, 0), new Color4b(255, 255, 255, 255), new Vector2(0, 0)),
                new VertexColorTexture(new Vector3(-0.5f, 0.5f, 0), new Color4b(255, 255, 255, 255), new Vector2(0, 1)),
                new VertexColorTexture(new Vector3(0.5f, -0.5f, 0), new Color4b(255, 255, 255, 255), new Vector2(1, 0)),
                new VertexColorTexture(new Vector3(0.5f, 0.5f, 0), new Color4b(255, 255, 255, 255), new Vector2(1, 1)),
            };

            texBuffer = new VertexBuffer<VertexColorTexture>(graphicsDevice, texBufferData, BufferUsageARB.StaticDraw);

            programBuilder = new SimpleShaderProgramBuilder()
            {
                VertexColorsEnabled = true,
                TextureEnabled = true
            };
            programBuilder.ConfigureVertexAttribs<VertexColorTexture>();
            texProgram = programBuilder.Create(graphicsDevice, true);
            Console.WriteLine("VS Log: " + programBuilder.VertexShaderLog);
            Console.WriteLine("FS Log: " + programBuilder.FragmentShaderLog);
            Console.WriteLine("Program Log: " + programBuilder.ProgramLog);

            Span<VertexPosition> cubemapBufferData = stackalloc VertexPosition[] {
                new Vector3(-0.5f, -0.5f, -0.5f), //4
                new Vector3(-0.5f, -0.5f, 0.5f), //3
                new Vector3(-0.5f, 0.5f, -0.5f), //7
                new Vector3(-0.5f, 0.5f, 0.5f), //8
                new Vector3(0.5f, 0.5f, 0.5f), //5
                new Vector3(-0.5f, -0.5f, 0.5f), //3
                new Vector3(0.5f, -0.5f, 0.5f), //1
                new Vector3(-0.5f, -0.5f, -0.5f), //4
                new Vector3(0.5f, -0.5f, -0.5f), //2
                new Vector3(-0.5f, 0.5f, -0.5f), //7
                new Vector3(0.5f, 0.5f, -0.5f), //6
                new Vector3(0.5f, 0.5f, 0.5f), //5
                new Vector3(0.5f, -0.5f, -0.5f), //2
                new Vector3(0.5f, -0.5f, 0.5f), //1
            };

            cubemapBuffer = new VertexBuffer<VertexPosition>(graphicsDevice, cubemapBufferData, BufferUsageARB.StaticDraw);

            cubemap = TextureCubemapExtensions.FromFiles(graphicsDevice,
                "cubemap/cubemap1_back.png", "cubemap/cubemap1_front.png",
                "cubemap/cubemap1_bottom.png", "cubemap/cubemap1_top.png",
                "cubemap/cubemap1_left.png", "cubemap/cubemap1_right.png"
                );
            cubemap.SetTextureFilters(TextureMinFilter.Linear, TextureMagFilter.Linear);
            cubemapProgram.Uniforms["samp"].SetValueTexture(cubemap);

            fbo1 = new Framebuffer2D(graphicsDevice, (uint)window.Size.Width, (uint)window.Size.Height, DepthStencilFormat.Depth24Stencil8);
            fbo2 = new Framebuffer2D(graphicsDevice, (uint)window.Size.Width, (uint)window.Size.Height, DepthStencilFormat.Depth24);
            fbo1.Texture.SetTextureFilters(TextureMinFilter.Linear, TextureMagFilter.Linear);
            fbo2.Texture.SetTextureFilters(TextureMinFilter.Linear, TextureMagFilter.Linear);

            OnWindowResized(window.Size);
        }

        private void Test3DBatcher_KeyDown(IKeyboard sender, Key key, int idk)
        {
            if (key == Key.Enter)
            {
                fbo2.Framebuffer.SaveAsImage(string.Concat("fbo", MathF.Round(time, 1).ToString(), ".png"), SaveImageFormat.Png);
                fbo2.Texture.SaveAsImage(string.Concat("tex", MathF.Round(time, 1).ToString(), ".png"), SaveImageFormat.Png);
            }
        }

        private System.Drawing.PointF oldLocation;
        private void Test3DBatcher_MouseMove(IMouse sender, System.Drawing.PointF location)
        {
            if (sender.IsButtonPressed(MouseButton.Left))
            {
                rotY += (location.X - oldLocation.X) * 0.005f;
                rotX += (location.Y - oldLocation.Y) * -0.005f;
                rotX = Math.Clamp(rotX, -1.57f, 1.57f);
                //inputContext.Mice[0].Position = new System.Drawing.PointF(window.Size.Width / 2f, window.Size.Height / 2f);

                oldLocation = location;
            }
        }

        private void Test3DBatcher_MouseUp(IMouse sender, MouseButton btn)
        {

        }

        private void Test3DBatcher_MouseDown(IMouse sender, MouseButton btn)
        {
            if (btn == MouseButton.Left)
                oldLocation = sender.Position;
            if (btn == MouseButton.Right)
            {
                Vector3 forward = new Vector3(MathF.Cos(rotY) * MathF.Cos(rotX), MathF.Sin(rotX), MathF.Sin(rotY) * MathF.Cos(rotX));
                Vector3 center = cameraPos + forward * Math.Clamp(sender.ScrollWheels[0].Y * 0.1f, 0.1f, 50f);
                //fuckables.Add(new Fuckable(getRandMesh(), center));
            }
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

            IKeyboard kb = inputContext.Keyboards[0];

            const float CameraMoveSpeed = 5;
            if (kb.IsKeyPressed(Key.Left))
                cameraPos.X -= CameraMoveSpeed * deltaTime;
            if (kb.IsKeyPressed(Key.Right))
                cameraPos.X += CameraMoveSpeed * deltaTime;
            if (kb.IsKeyPressed(Key.ShiftLeft) || kb.IsKeyPressed(Key.ShiftRight))
            {
                if (kb.IsKeyPressed(Key.E))
                    cameraPos.Z += CameraMoveSpeed * deltaTime;
                if (kb.IsKeyPressed(Key.Q))
                    cameraPos.Z -= CameraMoveSpeed * deltaTime;
            }
            else
            {
                if (kb.IsKeyPressed(Key.E))
                    cameraPos.Y += CameraMoveSpeed * deltaTime;
                if (kb.IsKeyPressed(Key.Q))
                    cameraPos.Y -= CameraMoveSpeed * deltaTime;
            }

            if (kb.IsKeyPressed(Key.R))
                cameraPos.Y += CameraMoveSpeed * deltaTime;
            if (kb.IsKeyPressed(Key.F))
                cameraPos.Y -= CameraMoveSpeed * deltaTime;

            float jejeX = kb.IsKeyPressed(Key.ShiftLeft) || kb.IsKeyPressed(Key.ShiftRight) ? rotX : 0;
            if (kb.IsKeyPressed(Key.W))
                cameraPos += new Vector3(MathF.Cos(rotY) * MathF.Cos(jejeX), MathF.Sin(jejeX), MathF.Sin(rotY) * MathF.Cos(jejeX)) * CameraMoveSpeed * deltaTime;
            if (kb.IsKeyPressed(Key.S))
                cameraPos -= new Vector3(MathF.Cos(rotY) * MathF.Cos(jejeX), MathF.Sin(jejeX), MathF.Sin(rotY) * MathF.Cos(jejeX)) * CameraMoveSpeed * deltaTime;

            if (kb.IsKeyPressed(Key.A))
                cameraPos += new Vector3(MathF.Sin(rotY) * MathF.Cos(jejeX), MathF.Sin(jejeX), MathF.Cos(rotY) * -MathF.Cos(jejeX)) * CameraMoveSpeed * deltaTime;
            if (kb.IsKeyPressed(Key.D))
                cameraPos -= new Vector3(MathF.Sin(rotY) * MathF.Cos(jejeX), -MathF.Sin(jejeX), MathF.Cos(rotY) * -MathF.Cos(jejeX)) * CameraMoveSpeed * deltaTime;

        }

        private void OnWindowRender(double dtSeconds)
        {
            if (window.IsClosing)
                return;

            graphicsDevice.BlendingEnabled = false;
            graphicsDevice.DepthState = DepthState.Default;
            graphicsDevice.ClearColor = new Vector4(0, 0, 0, 1);
            graphicsDevice.TextureCubemapSeamlessEnabled = true;
            graphicsDevice.DrawFramebuffer = fbo1;

            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4x4 mat = Matrix4x4.CreateLookAt(cameraPos, cameraPos + new Vector3(MathF.Cos(rotY), MathF.Tan(rotX), MathF.Sin(rotY)), Vector3.UnitY);
            batcherProgram.View = mat;
            cubemapProgram.Uniforms["View"].SetValueMat4(mat);
            texProgram.View = mat;

            graphicsDevice.VertexArray = cubemapBuffer.VertexArray;
            cubemapProgram.Uniforms["cameraPos"].SetValueVec3(cameraPos);
            graphicsDevice.ShaderProgram = cubemapProgram;
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

            Vector3 forward = new Vector3(MathF.Cos(rotY) * MathF.Cos(rotX), MathF.Sin(rotX), MathF.Sin(rotY) * MathF.Cos(rotX));
            Vector3 center = cameraPos + forward * (MathF.Max(inputContext.Mice[0].ScrollWheels[0].Y, 0f) * 0.1f + 1f);
            batcher.AddLine(new VertexColor(center, new Color4b(255, 0, 0, 255)), new VertexColor(new Vector3(0.25f, 0, 0) + center, new Color4b(255, 0, 0, 255)));
            batcher.AddLine(new VertexColor(center, new Color4b(0, 255, 0, 255)), new VertexColor(new Vector3(0, 0.25f, 0) + center, new Color4b(0, 255, 0, 255)));
            batcher.AddLine(new VertexColor(center, new Color4b(0, 0, 255, 255)), new VertexColor(new Vector3(0, 0, 0.25f) + center, new Color4b(0, 0, 255, 255)));

            Span<VertexColor> cube = stackalloc VertexColor[] {
                new VertexColor(new Vector3(-0.5f, -0.5f, -0.5f), Color4b.LightBlue),//4
                new VertexColor(new Vector3(-0.5f, -0.5f, 0.5f), Color4b.Lime),//3
                new VertexColor(new Vector3(-0.5f, 0.5f, -0.5f), Color4b.White),//7
                new VertexColor(new Vector3(-0.5f, 0.5f, 0.5f), Color4b.Black),//8
                new VertexColor(new Vector3(0.5f, 0.5f, 0.5f), Color4b.Blue),//5
                new VertexColor(new Vector3(-0.5f, -0.5f, 0.5f), Color4b.Lime),//3
                new VertexColor(new Vector3(0.5f, -0.5f, 0.5f), Color4b.Red),//1
                new VertexColor(new Vector3(-0.5f, -0.5f, -0.5f), Color4b.LightBlue),//4
                new VertexColor(new Vector3(0.5f, -0.5f, -0.5f), Color4b.Yellow),//2
                new VertexColor(new Vector3(-0.5f, 0.5f, -0.5f), Color4b.White),//7
                new VertexColor(new Vector3(0.5f, 0.5f, -0.5f), Color4b.Pink),//6
                new VertexColor(new Vector3(0.5f, 0.5f, 0.5f), Color4b.Blue),//5
                new VertexColor(new Vector3(0.5f, -0.5f, -0.5f), Color4b.Yellow),//2
                new VertexColor(new Vector3(0.5f, -0.5f, 0.5f), Color4b.Red),//1
            };

            Span<VertexColor> cone = stackalloc VertexColor[]
            {
                new VertexColor(new Vector3(-1, 0, -1), new Color4b(255, 0, 0, 255)),
                new VertexColor(new Vector3(-1, 0, 1), new Color4b(0, 255, 0, 255)),
                new VertexColor(new Vector3(0, 1, 0), new Color4b(0, 0, 255, 255)),
                new VertexColor(new Vector3(1, 0, 1), new Color4b(255, 0, 0, 255)),
                new VertexColor(new Vector3(1, 0, -1), new Color4b(0, 255, 0, 255)),
                new VertexColor(new Vector3(-1, 0, -1), new Color4b(0, 0, 255, 255)),
                new VertexColor(new Vector3(0, 1, 0), new Color4b(0, 0, 255, 255)),
            };

            Span<VertexColor> circleFan = stackalloc VertexColor[12];
            circleFan[0] = new VertexColor(new Vector3(0, 0, 0), new Color4b(0, 0, 0, 255));
            for (int i = 1; i < circleFan.Length - 1; i++)
            {
                float rot = (i - 1) * MathF.PI * 2f / (circleFan.Length - 2);
                circleFan[i] = new VertexColor(new Vector3(MathF.Cos(rot), 0, MathF.Sin(rot)), randomCol());
            }
            circleFan[circleFan.Length - 1] = circleFan[1];

            mat = Matrix4x4.CreateScale(1.5f) * Matrix4x4.CreateTranslation(0, -1f, 0);
            batcher.AddTriangleFan(MultiplyAllToNew(circleFan, mat));

            mat = Matrix4x4.CreateRotationY(time * MathF.PI);
            batcher.AddTriangleStrip(MultiplyAllToNew(cube, mat));

            mat = Matrix4x4.CreateRotationY(time * MathF.PI * 0.5f) * Matrix4x4.CreateScale(0.6f, 1.5f, 0.6f) * Matrix4x4.CreateTranslation(2, 0, -1.4f);
            batcher.AddTriangleStrip(MultiplyAllToNew(cone, mat));

            mat = Matrix4x4.CreateRotationY(-time * MathF.PI * 0.5f) * Matrix4x4.CreateScale(0.6f, 1.5f, 0.6f) * Matrix4x4.CreateTranslation(-1.4f, 0, 2);
            batcher.AddTriangleStrip(MultiplyAllToNew(cone, mat));

            mat = Matrix4x4.CreateTranslation(2f, 2f, 2f);
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
            }, mat).AsSpan(3, 8));

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
            }.AsSpan(2, 10));


            graphicsDevice.ShaderProgram = batcherProgram;
            if (batcher.TriangleVertexCount > triangleBuffer.StorageLength)
                triangleBuffer.RecreateStorage((uint)batcher.TriangleVertexCapacity);
            triangleBuffer.DataSubset.SetData(batcher.TriangleVertices);
            graphicsDevice.VertexArray = triangleBuffer.VertexArray;
            graphicsDevice.DrawArrays(PrimitiveType.Triangles, 0, (uint)batcher.TriangleVertexCount);
            batcher.ClearTriangles();

            if (batcher.LineVertexCount > lineBuffer.StorageLength)
                lineBuffer.RecreateStorage((uint)batcher.LineVertexCapacity);
            lineBuffer.DataSubset.SetData(batcher.LineVertices);
            graphicsDevice.VertexArray = lineBuffer.VertexArray;
            graphicsDevice.DrawArrays(PrimitiveType.Lines, 0, (uint)batcher.LineVertexCount);

            VertexColor[] linesGet = new VertexColor[lineBuffer.StorageLength];
            lineBuffer.DataSubset.GetData(linesGet);

            batcher.ClearLines();

            float ratio = fbo1.Width / (float)fbo1.Height;
            mat = Matrix4x4.CreateScale(-ratio * 10f, 10f, 1f) * Matrix4x4.CreateTranslation(2.5f, 2, 12);
            texProgram.World = mat;
            texProgram.Texture = fbo2;
            graphicsDevice.ShaderProgram = texProgram;
            graphicsDevice.VertexArray = texBuffer.VertexArray;
            graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

            graphicsDevice.ReadFramebuffer = fbo1;
            graphicsDevice.DrawFramebuffer = null;
            graphicsDevice.BlitFramebuffer(0, 0, (int)fbo1.Width, (int)fbo1.Height, 0, 0, (int)fbo1.Width, (int)fbo1.Height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);

            Framebuffer2D fbotmp = fbo1;
            fbo1 = fbo2;
            fbo2 = fbotmp;

            window.SwapBuffers();
        }

        private void OnWindowResized(System.Drawing.Size size)
        {
            if (size.Width == 0 || size.Height == 0)
                return;

            graphicsDevice.SetViewport(0, 0, (uint)size.Width, (uint)size.Height);

            float wid = size.Width / (float)size.Height;
            wid *= 0.5f;
            Matrix4x4 proj = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI * 0.5f, size.Width / (float)size.Height, 0.0001f, 100f);
            batcherProgram.Projection = proj;
            cubemapProgram.Uniforms["Projection"].SetValueMat4(proj);
            texProgram.Projection = proj;

            fbo1.Resize((uint)size.Width, (uint)size.Height);
            fbo2.Resize((uint)size.Width, (uint)size.Height);
        }

        private void OnWindowClosing()
        {
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

        public static VertexColor[] MultiplyAllToNew(ReadOnlySpan<VertexColor> vertex, in Matrix4x4 mat)
        {
            VertexColor[] arr = new VertexColor[vertex.Length];
            for (int i = 0; i < vertex.Length; i++)
            {
                Vector4 t = new Vector4(vertex[i].Position, 1f);
                t = Vector4.Transform(t, mat);
                arr[i].Position = new Vector3(t.X, t.Y, t.Z);
                arr[i].Color = vertex[i].Color;
            }
            return arr;
        }

        public static VertexColor[] SetColorAllToNew(ReadOnlySpan<VertexColor> vertex, Color4b color)
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
