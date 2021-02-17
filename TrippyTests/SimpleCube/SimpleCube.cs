using System;
using System.Diagnostics;
using System.Numerics;
using Silk.NET.Maths;
using TrippyGL;
using TrippyTestBase;

namespace SimpleCube
{
    // Draws a 3D rotating colored cube in front of a static camera.

    class SimpleCube : TestBase
    {
        Stopwatch stopwatch;

        VertexBuffer<VertexColor> vertexBuffer;
        SimpleShaderProgram shaderProgram;

        public SimpleCube() : base(null, 24) { }

        protected override void OnLoad()
        {
            Span<VertexColor> cubeBufferData = stackalloc VertexColor[] {
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

            vertexBuffer = new VertexBuffer<VertexColor>(graphicsDevice, cubeBufferData, BufferUsage.StaticCopy);

            shaderProgram = SimpleShaderProgram.Create<VertexColor>(graphicsDevice);

            shaderProgram.View = Matrix4x4.CreateLookAt(new Vector3(0, 1.0f, -1.5f), Vector3.Zero, Vector3.UnitY);

            graphicsDevice.DepthState = DepthState.Default;
            graphicsDevice.BlendState = BlendState.Opaque;

            stopwatch = Stopwatch.StartNew();
        }

        protected override void OnRender(double dt)
        {
            graphicsDevice.ClearDepth = 1f;
            graphicsDevice.ClearColor = Vector4.Zero;
            graphicsDevice.Clear(ClearBuffers.Color | ClearBuffers.Depth);

            shaderProgram.World = Matrix4x4.CreateRotationY(2 * (float)stopwatch.Elapsed.TotalSeconds);
            graphicsDevice.ShaderProgram = shaderProgram;
            graphicsDevice.VertexArray = vertexBuffer;

            graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, vertexBuffer.StorageLength);
        }

        protected override void OnResized(Vector2D<int> size)
        {
            if (size.X == 0 || size.Y == 0)
                return;

            graphicsDevice.SetViewport(0, 0, (uint)size.X, (uint)size.Y);
            shaderProgram.Projection = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 2f, size.X / (float)size.Y, 0.01f, 100f);
        }

        protected override void OnUnload()
        {
            vertexBuffer.Dispose();
            shaderProgram.Dispose();
        }
    }
}
