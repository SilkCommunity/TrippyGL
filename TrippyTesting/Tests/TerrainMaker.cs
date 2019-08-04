using System;
using System.IO;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using TrippyGL;

namespace TrippyTesting.Tests
{
    class TerrainMaker : GameWindow
    {
        const float MinX = -25, MaxX = 25, MinY = -25, MaxY = 25;
        const float WATER_TOP = 0.61810450337554419905178858716746f;

        System.Diagnostics.Stopwatch stopwatch;
        public static Random r = new Random();
        public static float time, deltaTime;

        VertexBuffer<VertexPosition> terrBuffer;
        ShaderProgram terrProgram;

        VertexBuffer<VertexColor> waterBuffer;
        ShaderProgram waterProgram;
        Texture2D refractionTex, reflectionTex;
        FramebufferObject refractionFbo, reflectionFbo;
        Texture2D distortMap, normalMap;

        TextureCubemap cubemap;
        ShaderProgram cubemapProgram;
        VertexBuffer<VertexPosition> cubemapBuffer;

        GraphicsDevice graphicsDevice;

        bool isMouseDown;
        Vector3 cameraPos;
        float rotY, rotX;

        MouseState ms, oldMs;
        KeyboardState ks, oldKs;

        public TerrainMaker() : base(1280, 720, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 32, 0, 0, ColorFormat.Empty, 2), "haha yes", GameWindowFlags.Default, DisplayDevice.Default, 4, 0, GraphicsContextFlags.Default)
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

            #region LoadTerrain

            PrimitiveBatcher<VertexPosition> batcher = new PrimitiveBatcher<VertexPosition>();

            float delta = 0.2f;
            for (float y = MinY; y < MaxY; y += delta)
                for (float x = MinX; x < MaxX; x += delta)
                {
                    batcher.AddQuad(
                        new VertexPosition(new Vector3(x, 0, y)),
                        new VertexPosition(new Vector3(x + delta, 0, y)),
                        new VertexPosition(new Vector3(x + delta, 0, y + delta)),
                        new VertexPosition(new Vector3(x, 0, y + delta))
                    );
                }

            terrBuffer = new VertexBuffer<VertexPosition>(graphicsDevice, batcher.TriangleVertexCount, BufferUsageHint.StaticDraw);
            batcher.WriteTrianglesTo(terrBuffer.DataSubset);

            terrProgram = new ShaderProgram(graphicsDevice);
            terrProgram.AddVertexShader(File.ReadAllText("terrain/terrvs.glsl"));
            terrProgram.AddFragmentShader(File.ReadAllText("terrain/terrfs.glsl"));
            terrProgram.SpecifyVertexAttribs<VertexPosition>(new string[] { "vPosition" });
            terrProgram.LinkProgram();
            Matrix4 identity = Matrix4.Identity;
            terrProgram.Uniforms["World"].SetValueMat4(ref identity);

            #endregion

            #region LoadWater

            waterProgram = new ShaderProgram(graphicsDevice);
            waterProgram.AddVertexShader(File.ReadAllText("terrain/watervs.glsl"));
            waterProgram.AddFragmentShader(File.ReadAllText("terrain/waterfs.glsl"));
            waterProgram.SpecifyVertexAttribs<VertexColor>(new string[] { "vPosition", "vColor" });
            waterProgram.LinkProgram();
            waterProgram.Uniforms["World"].SetValueMat4(ref identity);

            waterBuffer = new VertexBuffer<VertexColor>(graphicsDevice, new VertexColor[]
            {
                new VertexColor(new Vector3(MinX, WATER_TOP, MinY), new Color4b(100, 200, 255, 255)),
                new VertexColor(new Vector3(MinX, WATER_TOP, MaxY), new Color4b(100, 200, 255, 255)),
                new VertexColor(new Vector3(MaxX, WATER_TOP, MinY), new Color4b(100, 200, 255, 255)),
                new VertexColor(new Vector3(MaxX, WATER_TOP, MaxY), new Color4b(100, 200, 255, 255)),
            }, BufferUsageHint.StaticDraw);

            distortMap = new Texture2D(graphicsDevice, "terrain/distortMap.png");
            normalMap = new Texture2D(graphicsDevice, "terrain/normalMap.png");
            distortMap.SetWrapModes(TextureWrapMode.Repeat, TextureWrapMode.Repeat);
            normalMap.SetWrapModes(TextureWrapMode.Repeat, TextureWrapMode.Repeat);
            distortMap.SetTextureFilters(TextureMinFilter.Linear, TextureMagFilter.Linear);
            normalMap.SetTextureFilters(TextureMinFilter.Linear, TextureMagFilter.Linear);
            waterProgram.Uniforms["distortMap"].SetValueTexture(distortMap);
            waterProgram.Uniforms["normalMap"].SetValueTexture(normalMap);

            refractionTex = null;
            reflectionTex = null;
            refractionFbo = FramebufferObject.Create2D(ref refractionTex, graphicsDevice, Width, Height, DepthStencilFormat.Depth32f);
            reflectionFbo = FramebufferObject.Create2D(ref reflectionTex, graphicsDevice, Width, Height, DepthStencilFormat.Depth32f);
            refractionTex.SetWrapModes(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);
            reflectionTex.SetWrapModes(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);
            refractionTex.SetTextureFilters(TextureMinFilter.Linear, TextureMagFilter.Linear);

            #endregion

            #region LoadCubeMap

            graphicsDevice.TextureCubemapSeamlessEnabled = true;

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

            cubemapProgram = new ShaderProgram(graphicsDevice);
            cubemapProgram.AddVertexShader(File.ReadAllText("cubemap/vs.glsl"));
            cubemapProgram.AddFragmentShader(File.ReadAllText("cubemap/fs.glsl"));
            cubemapProgram.SpecifyVertexAttribs<VertexPosition>(new string[] { "vPosition" });
            cubemapProgram.LinkProgram();

            cubemap = new TextureCubemap(graphicsDevice, 800);
            cubemap.SetData(CubeMapFace.PositiveX, "cubemap/cubemap1_front.png");
            cubemap.SetData(CubeMapFace.NegativeX, "cubemap/cubemap1_back.png");
            cubemap.SetData(CubeMapFace.NegativeZ, "cubemap/cubemap1_left.png");
            cubemap.SetData(CubeMapFace.PositiveZ, "cubemap/cubemap1_right.png");
            cubemap.SetData(CubeMapFace.PositiveY, "cubemap/cubemap1_top.png");
            cubemap.SetData(CubeMapFace.NegativeY, "cubemap/cubemap1_bottom.png");
            cubemap.SetTextureFilters(TextureMinFilter.Linear, TextureMagFilter.Linear);
            cubemapProgram.Uniforms["samp"].SetValueTexture(cubemap);

            #endregion LoadCubeMap
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

            float CameraMoveSpeed = ks.IsKeyDown(Key.LControl) ? 15 : 1.75f;
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

            if (WindowState != WindowState.Minimized && isMouseDown)
            {
                rotY += (ms.X - oldMs.X) * 0.005f;
                rotX += (ms.Y - oldMs.Y) * -0.005f;
                rotX = MathHelper.Clamp(rotX, -1.57f, 1.57f);
                Mouse.SetPosition(Width / 2f + X, Height / 2f + Y);
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            graphicsDevice.ClipDistances[0] = true;
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthState = DepthTestingState.Default;
            graphicsDevice.ClearColor = new Color4(0f, 0f, 0f, 1f);

            waterProgram.Uniforms["time"].SetValue1(time);

            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Vector3 cameraDir = new Vector3((float)Math.Cos(rotY) * (float)Math.Cos(rotX), (float)Math.Sin(rotX), (float)Math.Sin(rotY) * (float)Math.Cos(rotX));
            Vector3 invertedCameraDir = new Vector3((float)Math.Cos(rotY) * (float)Math.Cos(-rotX), (float)Math.Sin(-rotX), (float)Math.Sin(rotY) * (float)Math.Cos(-rotX));

            Matrix4 view = Matrix4.LookAt(cameraPos, cameraPos + new Vector3((float)Math.Cos(rotY), (float)Math.Tan(rotX), (float)Math.Sin(rotY)), Vector3.UnitY);
            Vector3 invertedCameraPos = new Vector3(cameraPos.X, WATER_TOP * 2 - cameraPos.Y, cameraPos.Z);
            Matrix4 waterInvertedView = Matrix4.LookAt(invertedCameraPos, invertedCameraPos + new Vector3((float)Math.Cos(rotY), (float)Math.Tan(-rotX), (float)Math.Sin(rotY)), Vector3.UnitY);
            terrProgram.Uniforms["View"].SetValueMat4(ref view);
            waterProgram.Uniforms["View"].SetValueMat4(ref view);
            cubemapProgram.Uniforms["cameraPos"].SetValue3(ref cameraPos);
            waterProgram.Uniforms["cameraPos"].SetValue3(ref cameraPos);

            Matrix4 world = Matrix4.CreateTranslation((int)cameraPos.X, 0, (int)cameraPos.Z);
            terrProgram.Uniforms["World"].SetValueMat4(ref world);
            waterProgram.Uniforms["World"].SetValueMat4(ref world);

            graphicsDevice.DrawFramebuffer = refractionFbo;
            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            terrProgram.Uniforms["clipOffset"].SetValue1(WATER_TOP+0.01f);
            terrProgram.Uniforms["clipMultiplier"].SetValue1(-1.0f); //render only below the water
            graphicsDevice.VertexArray = terrBuffer.VertexArray;
            graphicsDevice.ShaderProgram = terrProgram;
            graphicsDevice.DrawArrays(PrimitiveType.Triangles, 0, terrBuffer.StorageLength);

            graphicsDevice.DrawFramebuffer = reflectionFbo;
            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit);
            cubemapProgram.Uniforms["View"].SetValueMat4(ref waterInvertedView);
            cubemapProgram.Uniforms["cameraPos"].SetValue3(ref invertedCameraPos);
            graphicsDevice.ShaderProgram = cubemapProgram;
            graphicsDevice.VertexArray = cubemapBuffer.VertexArray;
            graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, cubemapBuffer.StorageLength);
            graphicsDevice.Clear(ClearBufferMask.DepthBufferBit);
            terrProgram.Uniforms["clipOffset"].SetValue1(WATER_TOP-0.01f);
            terrProgram.Uniforms["clipMultiplier"].SetValue1(1.0f); //render only above the water
            terrProgram.Uniforms["View"].SetValueMat4(ref waterInvertedView);
            graphicsDevice.VertexArray = terrBuffer.VertexArray;
            graphicsDevice.ShaderProgram = terrProgram;
            graphicsDevice.DrawArrays(PrimitiveType.Triangles, 0, terrBuffer.StorageLength);

            terrProgram.Uniforms["View"].SetValueMat4(ref view);

            graphicsDevice.DrawFramebuffer = null;
            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit);
            cubemapProgram.Uniforms["View"].SetValueMat4(ref view);
            cubemapProgram.Uniforms["cameraPos"].SetValue3(ref cameraPos);
            graphicsDevice.ShaderProgram = cubemapProgram;
            graphicsDevice.VertexArray = cubemapBuffer.VertexArray;
            graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, cubemapBuffer.StorageLength);
            graphicsDevice.Clear(ClearBufferMask.DepthBufferBit);

            graphicsDevice.VertexArray = terrBuffer.VertexArray;
            terrProgram.Uniforms["clipOffset"].SetValue1(WATER_TOP-0.01f);
            terrProgram.Uniforms["clipMultiplier"].SetValue1(1.0f); //render only above the water
            graphicsDevice.ShaderProgram = terrProgram;
            graphicsDevice.DrawArrays(PrimitiveType.Triangles, 0, terrBuffer.StorageLength);

            graphicsDevice.VertexArray = waterBuffer.VertexArray;
            waterProgram.Uniforms["refractionSamp"].SetValueTexture(refractionTex);
            waterProgram.Uniforms["reflectionSamp"].SetValueTexture(reflectionTex);
            graphicsDevice.ShaderProgram = waterProgram;
            graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

            SwapBuffers();
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

        protected override void OnResize(EventArgs e)
        {
            graphicsDevice.Viewport = new Rectangle(0, 0, Width, Height);

            float wid = Width / (float)Height;
            wid *= 0.5f;
            Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver2, Width / (float)Height, 0.0001f, 100f);
            terrProgram.Uniforms["Projection"].SetValueMat4(ref proj);
            waterProgram.Uniforms["Projection"].SetValueMat4(ref proj);
            cubemapProgram.Uniforms["Projection"].SetValueMat4(ref proj);

            FramebufferObject.Resize2D(refractionFbo, Width, Height);
            FramebufferObject.Resize2D(reflectionFbo, Width, Height);
        }

        protected override void OnUnload(EventArgs e)
        {
            terrBuffer.Dispose();
            terrProgram.Dispose();
            waterBuffer.Dispose();
            waterProgram.Dispose();
            reflectionFbo.DisposeAttachments();
            reflectionFbo.Dispose();
            refractionFbo.DisposeAttachments();
            refractionFbo.Dispose();
            distortMap.Dispose();
            normalMap.Dispose();
            cubemap.Dispose();
            cubemapProgram.Dispose();
            cubemapBuffer.Dispose();

            graphicsDevice.Dispose();
        }
    }
}
