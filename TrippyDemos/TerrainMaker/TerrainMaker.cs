using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using Silk.NET.Input.Common;
using Silk.NET.OpenGL;
using TrippyGL;
using TrippyGL.ImageSharp;
using TrippyTestBase;

namespace TerrainMaker
{
    class TerrainMaker : TestBase
    {
        public static Stopwatch stopwatch = new Stopwatch();
        public static Random random = new Random();

        InputManager3D inputManager;

        ShaderProgram waterProgram;
        ShaderUniform waterViewUniform;
        ShaderProgram terrainProgram;
        ShaderUniform terrainViewUniform;

        Framebuffer2D waterReflectFbo;
        Framebuffer2D waterRefractFbo;
        Texture2D waterDistortMap, waterNormalsMap;

        SimpleShaderProgram linesProgram;
        VertexBuffer<VertexColor> linesBuffer;

        ChunkManager chunkManager;

        public TerrainMaker() : base(null, 24, true) { }

        protected override void OnLoad()
        {
            stopwatch.Restart();
            inputManager = new InputManager3D(InputContext);

            Vector3 startCoords = new Vector3(TerrainGenerator.ChunkSize / 2f, 0f, TerrainGenerator.ChunkSize / 2f);
            startCoords.Y = Math.Max(NoiseGenerator.GenHeight(new Vector2(startCoords.X, startCoords.Z)), 0) + 32f;
            inputManager.CameraPosition = startCoords;

            terrainProgram = ShaderProgram.FromFiles<VertexNormalColor>(graphicsDevice, "data/terrainVs.glsl", "data/terrainFs.glsl", "vPosition", "vNormal", "vColor");
            terrainViewUniform = terrainProgram.Uniforms["View"];

            waterProgram = ShaderProgram.FromFiles<VertexNormalColor>(graphicsDevice, "data/waterVs.glsl", "data/waterFs.glsl", "vPosition");
            waterViewUniform = waterProgram.Uniforms["View"];

            waterDistortMap = Texture2DExtensions.FromFile(graphicsDevice, "data/distortMap.png");
            waterNormalsMap = Texture2DExtensions.FromFile(graphicsDevice, "data/normalMap.png");

            waterReflectFbo = new Framebuffer2D(graphicsDevice, (uint)Window.Size.Width, (uint)Window.Size.Height, DepthStencilFormat.Depth24);
            waterRefractFbo = new Framebuffer2D(graphicsDevice, (uint)Window.Size.Width, (uint)Window.Size.Height, DepthStencilFormat.Depth24);

            Span<VertexColor> lines = stackalloc VertexColor[]
            {
                new VertexColor(new Vector3(0, 0, 0), Color4b.Red),
                new VertexColor(new Vector3(1, 0, 0), Color4b.Red),
                new VertexColor(new Vector3(0, 0, 0), Color4b.Lime),
                new VertexColor(new Vector3(0, 1, 0), Color4b.Lime),
                new VertexColor(new Vector3(0, 0, 0), Color4b.Blue),
                new VertexColor(new Vector3(0, 0, 1), Color4b.Blue),
            };

            linesBuffer = new VertexBuffer<VertexColor>(graphicsDevice, lines, BufferUsageARB.StaticDraw);
            linesProgram = SimpleShaderProgram.Create<VertexColor>(graphicsDevice);

            chunkManager = new ChunkManager(12, (int)inputManager.CameraPosition.X / TerrainGenerator.ChunkSize, (int)inputManager.CameraPosition.Z / TerrainGenerator.ChunkSize);

            graphicsDevice.BlendState = BlendState.NonPremultiplied;
            graphicsDevice.DepthState = new DepthState(true, DepthFunction.Lequal);
            graphicsDevice.ClearColor = Vector4.UnitW;
        }

        protected override void OnUpdate(double dt)
        {
            base.OnUpdate(dt);

            inputManager.CameraMoveSpeed = inputManager.CurrentKeyboard.IsKeyPressed(Key.ControlLeft) ? 175 : 30;
            inputManager.Update((float)dt);
            //float th = NoiseGenerator.GenHeight(new Vector2(inputManager.CameraPosition.X, inputManager.CameraPosition.Z));
            //inputManager.CameraPosition.Y = Math.Max(inputManager.CameraPosition.Y, th + 4);
            //Window.Title = inputManager.CameraPosition.ToString();

            chunkManager.SetCenterChunk((int)MathF.Floor(inputManager.CameraPosition.X / TerrainGenerator.ChunkSize), (int)MathF.Floor(inputManager.CameraPosition.Z / TerrainGenerator.ChunkSize));
            chunkManager.ProcessChunks(graphicsDevice);
        }

        protected override void OnRender(double dt)
        {
            graphicsDevice.Framebuffer = null;
            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4x4 view = inputManager.CalculateViewMatrix();
            /*
            Matrix4x4 invertedView = Matrix4x4.CreateTranslation(-inputManager.CameraPosition.X, inputManager.CameraPosition.Y, -inputManager.CameraPosition.Z);
            invertedView *= Matrix4x4.CreateRotationY(inputManager.CameraRotationY + MathF.PI / 2f);
            invertedView *= Matrix4x4.CreateRotationX(inputManager.CameraRotationX);

            graphicsDevice.DrawFramebuffer = waterRefractFbo;
            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            graphicsDevice.ShaderProgram = terrainProgram;
            terrainViewUniform.SetValueMat4(view);
            chunkManager.RenderAllUnderwaters(graphicsDevice);

            graphicsDevice.DrawFramebuffer = waterReflectFbo;
            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            terrainViewUniform.SetValueMat4(invertedView);
            chunkManager.RenderAllTerrains(graphicsDevice);

            graphicsDevice.DrawFramebuffer = null;
            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            terrainViewUniform.SetValueMat4(view);
            chunkManager.RenderAllTerrains(graphicsDevice);
            */

            graphicsDevice.FaceCullingEnabled = true;
            graphicsDevice.ShaderProgram = terrainProgram;
            terrainViewUniform.SetValueMat4(view);
            chunkManager.RenderAllTerrains(graphicsDevice);
            chunkManager.RenderAllUnderwaters(graphicsDevice);

            graphicsDevice.FaceCullingEnabled = false;
            graphicsDevice.ShaderProgram = waterProgram;
            waterViewUniform.SetValueMat4(view);
            chunkManager.RenderAllUnderwaters(graphicsDevice);

            graphicsDevice.ShaderProgram = linesProgram;
            linesProgram.View = view;
            linesProgram.World = Matrix4x4.CreateScale(0.05f) * Matrix4x4.CreateTranslation(inputManager.CameraPosition + inputManager.CalculateForwardVector());
            graphicsDevice.VertexArray = linesBuffer;
            graphicsDevice.DrawArrays(PrimitiveType.Lines, 0, linesBuffer.StorageLength);

            Window.SwapBuffers();
        }

        protected override void OnResized(Size size)
        {
            if (size.Width == 0 || size.Height == 0)
                return;

            graphicsDevice.Viewport = new Viewport(0, 0, (uint)size.Width, (uint)size.Height);

            Matrix4x4 proj = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 2f, (float)size.Width / size.Height, 0.01f, 500);
            terrainProgram.Uniforms["Projection"].SetValueMat4(proj);
            waterProgram.Uniforms["Projection"].SetValueMat4(proj);
            linesProgram.Projection = proj;

            waterRefractFbo.Resize((uint)size.Width, (uint)size.Height);
            waterReflectFbo.Resize((uint)size.Width, (uint)size.Height);
        }

        protected override void OnUnload()
        {
            terrainProgram.Dispose();
        }
    }
}
