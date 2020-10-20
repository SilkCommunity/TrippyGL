using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using Silk.NET.Input.Common;
using Silk.NET.OpenGL;
using TrippyGL;
using TrippyTestBase;

namespace TerrainMaker
{
    class TerrainMaker : TestBase
    {
        public static Stopwatch stopwatch = Stopwatch.StartNew();
        public static Random random = new Random();

        private InputManager3D inputManager;

        private SimpleShaderProgram waterProgram;
        private ShaderProgram terrainProgram;
        private ShaderUniform terrainViewUniform;

        private SimpleShaderProgram linesProgram;
        private VertexBuffer<VertexColor> linesBuffer;

        ChunkManager chunkManager;

        public TerrainMaker() : base(null, 24, true) { }

        protected override void OnLoad()
        {
            inputManager = new InputManager3D(InputContext);
            inputManager.CameraPosition = new Vector3(TerrainGenerator.ChunkSize / 2f, 10f, TerrainGenerator.ChunkSize / 2f);

            terrainProgram = ShaderProgram.FromFiles<TerrainVertex>(graphicsDevice, "data/terrainVs.glsl", "data/terrainFs.glsl", "vPosition", "vNormal", "vHumidity", "vVegetation");
            terrainViewUniform = terrainProgram.Uniforms["View"];

            waterProgram = SimpleShaderProgram.Create<TerrainVertex>(graphicsDevice, 0, 0, true);

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

            chunkManager = new ChunkManager(8, (int)inputManager.CameraPosition.X / TerrainGenerator.ChunkSize, (int)inputManager.CameraPosition.Z / TerrainGenerator.ChunkSize);

            graphicsDevice.BlendState = BlendState.NonPremultiplied;
            graphicsDevice.DepthState = DepthState.Default;
            graphicsDevice.ClearColor = new Vector4(0, 0, 0, 1);
        }

        protected override void OnUpdate(double dt)
        {
            base.OnUpdate(dt);

            inputManager.CameraMoveSpeed = inputManager.CurrentKeyboard.IsKeyPressed(Key.ControlLeft) ? 15 : 1;
            inputManager.Update((float)dt);
            Window.Title = inputManager.CameraPosition.ToString();

            chunkManager.SetCenterChunk((int)MathF.Floor(inputManager.CameraPosition.X / TerrainGenerator.ChunkSize), (int)MathF.Floor(inputManager.CameraPosition.Z / TerrainGenerator.ChunkSize));
            chunkManager.ProcessChunks(graphicsDevice);
        }

        protected override void OnRender(double dt)
        {
            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4x4 view = inputManager.CalculateViewMatrix();

            graphicsDevice.ShaderProgram = terrainProgram;
            terrainViewUniform.SetValueMat4(view);
            chunkManager.RenderAllTerrains(graphicsDevice);
            chunkManager.RenderAllUnderwaters(graphicsDevice);

            graphicsDevice.ShaderProgram = waterProgram;
            waterProgram.Color = new Vector4(0, 0, 0.7f, 0.5f);
            waterProgram.View = view;
            chunkManager.RenderAllWaters(graphicsDevice);

            graphicsDevice.ShaderProgram = linesProgram;
            linesProgram.View = view;
            linesProgram.World = Matrix4x4.CreateScale(0.25f) * Matrix4x4.CreateTranslation(inputManager.CameraPosition + inputManager.CalculateForwardVector());
            graphicsDevice.VertexArray = linesBuffer;
            graphicsDevice.DrawArrays(PrimitiveType.Lines, 0, linesBuffer.StorageLength);

            // TODO: delete chunk.RenderTerrain(), RenderWater(), RenderUnderwater()
            /*chunk.RenderTerrain();
            otherChunk.RenderTerrain();
            chunk.RenderUnderwater();
            otherChunk.RenderUnderwater();

            graphicsDevice.ShaderProgram = waterProgram;
            waterProgram.Color = new Vector4(0, 0, 0.7f, 0.5f);
            waterProgram.View = view;
            chunk.RenderWater();
            otherChunk.RenderWater();*/

            Window.SwapBuffers();
        }

        protected override void OnResized(Size size)
        {
            if (size.Width == 0 || size.Height == 0)
                return;

            graphicsDevice.Viewport = new Viewport(0, 0, (uint)size.Width, (uint)size.Height);

            Matrix4x4 proj = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 2f, (float)size.Width / size.Height, 0.01f, 250);
            terrainProgram.Uniforms["Projection"].SetValueMat4(proj);
            waterProgram.Projection = proj;
            linesProgram.Projection = proj;
        }

        protected override void OnUnload()
        {
            terrainProgram.Dispose();
        }
    }
}
