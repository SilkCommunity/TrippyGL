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
    class TerrainMaker
    {
        const float MinX = -25, MaxX = 25, MinY = -25, MaxY = 25;
        const float WATER_TOP = 0.61810450337554419905178858716746f;

        System.Diagnostics.Stopwatch stopwatch;
        public static Random r = new Random();
        public static float time, deltaTime;
        readonly IWindow window;
        IInputContext inputContext;

        GraphicsDevice graphicsDevice;

        VertexBuffer<VertexPosition> terrBuffer;
        ShaderProgram terrProgram;

        VertexBuffer<VertexColor> waterBuffer;
        ShaderProgram waterProgram;
        Framebuffer2D refractionFbo, reflectionFbo;
        Texture2D distortMap, normalMap;

        TextureCubemap cubemap;
        ShaderProgram cubemapProgram;
        VertexBuffer<VertexPosition> cubemapBuffer;

        Vector3 cameraPos;
        float rotY, rotX;

        public TerrainMaker()
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
            inputContext.Keyboards[0].KeyDown += OnKeyDown;
            inputContext.Mice[0].MouseDown += OnMouseDown;
            inputContext.Mice[0].MouseUp += OnMouseUp;
            inputContext.Mice[0].MouseMove += OnMouseMove;

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

            terrBuffer = new VertexBuffer<VertexPosition>(graphicsDevice, (uint)batcher.TriangleVertexCount, BufferUsageARB.StaticDraw);
            terrBuffer.DataSubset.SetData(batcher.TriangleVertices);

            ShaderProgramBuilder programBuilder = new ShaderProgramBuilder();
            programBuilder.VertexShaderCode = File.ReadAllText("terrain/terrvs.glsl");
            programBuilder.FragmentShaderCode = File.ReadAllText("terrain/terrfs.glsl");
            programBuilder.SpecifyVertexAttribs<VertexPosition>(new string[] { "vPosition" });
            terrProgram = programBuilder.Create(graphicsDevice, true);
            Matrix4x4 identity = Matrix4x4.Identity;
            terrProgram.Uniforms["World"].SetValueMat4(identity);

            #endregion

            #region LoadWater

            programBuilder = new ShaderProgramBuilder();
            programBuilder.VertexShaderCode = File.ReadAllText("terrain/watervs.glsl");
            programBuilder.FragmentShaderCode = File.ReadAllText("terrain/waterfs.glsl");
            programBuilder.SpecifyVertexAttribs<VertexColor>(new string[] { "vPosition", "vColor" });
            waterProgram = programBuilder.Create(graphicsDevice, true);
            waterProgram.Uniforms["World"].SetValueMat4(identity);

            waterBuffer = new VertexBuffer<VertexColor>(graphicsDevice, new VertexColor[]
            {
                new VertexColor(new Vector3(MinX, WATER_TOP, MinY), new Color4b(100, 200, 255, 255)),
                new VertexColor(new Vector3(MinX, WATER_TOP, MaxY), new Color4b(100, 200, 255, 255)),
                new VertexColor(new Vector3(MaxX, WATER_TOP, MinY), new Color4b(100, 200, 255, 255)),
                new VertexColor(new Vector3(MaxX, WATER_TOP, MaxY), new Color4b(100, 200, 255, 255)),
            }, BufferUsageARB.StaticDraw);

            distortMap = Texture2DExtensions.FromFile(graphicsDevice, "terrain/distortMap.png");
            normalMap = Texture2DExtensions.FromFile(graphicsDevice, "terrain/normalMap.png");
            distortMap.SetWrapModes(TextureWrapMode.Repeat, TextureWrapMode.Repeat);
            normalMap.SetWrapModes(TextureWrapMode.Repeat, TextureWrapMode.Repeat);
            distortMap.SetTextureFilters(TextureMinFilter.Linear, TextureMagFilter.Linear);
            normalMap.SetTextureFilters(TextureMinFilter.Linear, TextureMagFilter.Linear);
            waterProgram.Uniforms["distortMap"].SetValueTexture(distortMap);
            waterProgram.Uniforms["normalMap"].SetValueTexture(normalMap);

            refractionFbo = new Framebuffer2D(graphicsDevice, (uint)window.Size.Width, (uint)window.Size.Height, DepthStencilFormat.Depth32f, 0, TextureImageFormat.Color4b, true);//FramebufferObject.Create2D(ref refractionTex, graphicsDevice, (uint)window.Size.Width, (uint)window.Size.Height, DepthStencilFormat.Depth32f);
            reflectionFbo = new Framebuffer2D(graphicsDevice, (uint)window.Size.Width, (uint)window.Size.Height, DepthStencilFormat.Depth32f, 0, TextureImageFormat.Color4b, true);//FramebufferObject.Create2D(ref reflectionTex, graphicsDevice, (uint)window.Size.Width, (uint)window.Size.Height, DepthStencilFormat.Depth32f);
            refractionFbo.Texture.SetWrapModes(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);
            reflectionFbo.Texture.SetWrapModes(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);
            refractionFbo.Texture.SetTextureFilters(TextureMinFilter.Linear, TextureMagFilter.Linear);

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
            }, BufferUsageARB.StaticDraw);

            programBuilder = new ShaderProgramBuilder();
            programBuilder.VertexShaderCode = File.ReadAllText("cubemap/vs.glsl");
            programBuilder.FragmentShaderCode = File.ReadAllText("cubemap/fs.glsl");
            programBuilder.SpecifyVertexAttribs<VertexPosition>(new string[] { "vPosition" });
            cubemapProgram = programBuilder.Create(graphicsDevice, true);

            cubemap = TextureCubemapExtensions.FromFiles(graphicsDevice,
                "cubemap/cubemap1_back.png", "cubemap/cubemap1_front.png",
                "cubemap/cubemap1_bottom.png", "cubemap/cubemap1_top.png",
                "cubemap/cubemap1_left.png", "cubemap/cubemap1_right.png"
                );
            cubemap.SetTextureFilters(TextureMinFilter.Linear, TextureMagFilter.Linear);
            cubemapProgram.Uniforms["samp"].SetValueTexture(cubemap);

            #endregion LoadCubeMap

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

            IKeyboard kb = inputContext.Keyboards[0];

            float CameraMoveSpeed = kb.IsKeyPressed(Key.ControlLeft) ? 15f : 1.75f;
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

            graphicsDevice.EnableClipDistance(0);
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthState = DepthState.Default;
            graphicsDevice.ClearColor = new Vector4(0f, 0f, 0f, 1f);

            waterProgram.Uniforms["time"].SetValueFloat(time);

            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Vector3 cameraDir = new Vector3(MathF.Cos(rotY) * MathF.Cos(rotX), MathF.Sin(rotX), MathF.Sin(rotY) * MathF.Cos(rotX));
            Vector3 invertedCameraDir = new Vector3(MathF.Cos(rotY) * MathF.Cos(-rotX), MathF.Sin(-rotX), MathF.Sin(rotY) * MathF.Cos(-rotX));

            Matrix4x4 view = Matrix4x4.CreateLookAt(cameraPos, cameraPos + new Vector3(MathF.Cos(rotY), MathF.Tan(rotX), MathF.Sin(rotY)), Vector3.UnitY);
            Vector3 invertedCameraPos = new Vector3(cameraPos.X, WATER_TOP * 2 - cameraPos.Y, cameraPos.Z);
            Matrix4x4 waterInvertedView = Matrix4x4.CreateLookAt(invertedCameraPos, invertedCameraPos + new Vector3(MathF.Cos(rotY), MathF.Tan(-rotX), MathF.Sin(rotY)), Vector3.UnitY);
            terrProgram.Uniforms["View"].SetValueMat4(view);
            waterProgram.Uniforms["View"].SetValueMat4(view);
            cubemapProgram.Uniforms["cameraPos"].SetValueVec3(cameraPos);
            waterProgram.Uniforms["cameraPos"].SetValueVec3(cameraPos);

            Matrix4x4 world = Matrix4x4.CreateTranslation((int)cameraPos.X, 0, (int)cameraPos.Z);
            terrProgram.Uniforms["World"].SetValueMat4(world);
            waterProgram.Uniforms["World"].SetValueMat4(world);

            graphicsDevice.DrawFramebuffer = refractionFbo;
            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            terrProgram.Uniforms["clipOffset"].SetValueFloat(WATER_TOP + 0.01f);
            terrProgram.Uniforms["clipMultiplier"].SetValueFloat(-1.0f); //render only below the water
            graphicsDevice.VertexArray = terrBuffer;
            graphicsDevice.ShaderProgram = terrProgram;
            graphicsDevice.DrawArrays(PrimitiveType.Triangles, 0, terrBuffer.StorageLength);

            graphicsDevice.DrawFramebuffer = reflectionFbo;
            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit);
            cubemapProgram.Uniforms["View"].SetValueMat4(waterInvertedView);
            cubemapProgram.Uniforms["cameraPos"].SetValueVec3(invertedCameraPos);
            graphicsDevice.ShaderProgram = cubemapProgram;
            graphicsDevice.VertexArray = cubemapBuffer;
            graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, cubemapBuffer.StorageLength);
            graphicsDevice.Clear(ClearBufferMask.DepthBufferBit);
            terrProgram.Uniforms["clipOffset"].SetValueFloat(WATER_TOP - 0.01f);
            terrProgram.Uniforms["clipMultiplier"].SetValueFloat(1.0f); //render only above the water
            terrProgram.Uniforms["View"].SetValueMat4(waterInvertedView);
            graphicsDevice.VertexArray = terrBuffer;
            graphicsDevice.ShaderProgram = terrProgram;
            graphicsDevice.DrawArrays(PrimitiveType.Triangles, 0, terrBuffer.StorageLength);

            terrProgram.Uniforms["View"].SetValueMat4(view);

            graphicsDevice.DrawFramebuffer = null;
            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit);
            cubemapProgram.Uniforms["View"].SetValueMat4(view);
            cubemapProgram.Uniforms["cameraPos"].SetValueVec3(cameraPos);
            graphicsDevice.ShaderProgram = cubemapProgram;
            graphicsDevice.VertexArray = cubemapBuffer;
            graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, cubemapBuffer.StorageLength);
            graphicsDevice.Clear(ClearBufferMask.DepthBufferBit);

            graphicsDevice.VertexArray = terrBuffer;
            terrProgram.Uniforms["clipOffset"].SetValueFloat(WATER_TOP - 0.01f);
            terrProgram.Uniforms["clipMultiplier"].SetValueFloat(1.0f); //render only above the water
            graphicsDevice.ShaderProgram = terrProgram;
            graphicsDevice.DrawArrays(PrimitiveType.Triangles, 0, terrBuffer.StorageLength);

            graphicsDevice.VertexArray = waterBuffer;
            waterProgram.Uniforms["refractionSamp"].SetValueTexture(refractionFbo);
            waterProgram.Uniforms["reflectionSamp"].SetValueTexture(reflectionFbo);
            graphicsDevice.ShaderProgram = waterProgram;
            graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

            window.SwapBuffers();
        }

        private System.Drawing.PointF oldLocation;
        private void OnMouseMove(IMouse sender, System.Drawing.PointF location)
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

        private void OnMouseUp(IMouse sender, MouseButton btn)
        {

        }

        private void OnMouseDown(IMouse sender, MouseButton btn)
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

        private void OnKeyDown(IKeyboard sender, Key key, int idk)
        {

        }

        private void OnWindowResized(System.Drawing.Size size)
        {
            if (size.Width == 0 || size.Height == 0)
                return;

            graphicsDevice.SetViewport(0, 0, (uint)size.Width, (uint)size.Height);

            float wid = size.Width / (float)size.Height;
            wid *= 0.5f;
            Matrix4x4 proj = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI * 0.5f, size.Width / (float)size.Height, 0.0001f, 100f);
            terrProgram.Uniforms["Projection"].SetValueMat4(proj);
            waterProgram.Uniforms["Projection"].SetValueMat4(proj);
            cubemapProgram.Uniforms["Projection"].SetValueMat4(proj);

            refractionFbo.Resize((uint)size.Width, (uint)size.Height);
            reflectionFbo.Resize((uint)size.Width, (uint)size.Height);
        }

        private void OnWindowClosing()
        {
            graphicsDevice.Dispose();
        }
    }
}
