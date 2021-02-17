using System;
using System.Numerics;
using TrippyGL;
using TrippyTestBase;

namespace SimpleTriangle
{
    // A simple project that opens an OpenGL window and renders a centered colored triangle.

    class SimpleTriangle : TestBase
    {
        VertexBuffer<VertexColor> vertexBuffer;
        SimpleShaderProgram shaderProgram;

        protected override void OnLoad()
        {
            Span<VertexColor> vertexData = stackalloc VertexColor[]
            {
                new VertexColor(new Vector3(-0.5f, -0.5f, 0), new Color4b(255, 0, 0, 255)),
                new VertexColor(new Vector3(0, 0.5f, 0), new Color4b(0, 255, 0, 255)),
                new VertexColor(new Vector3(0.5f, -0.5f, 0), new Color4b(0, 0, 255, 255)),
            };

            vertexBuffer = new VertexBuffer<VertexColor>(graphicsDevice, vertexData, BufferUsage.StaticDraw);
            shaderProgram = SimpleShaderProgram.Create<VertexColor>(graphicsDevice);

            graphicsDevice.ClearColor = new Vector4(0, 0, 0, 1);
            graphicsDevice.BlendingEnabled = false;
            graphicsDevice.DepthTestingEnabled = false;
        }

        protected override void OnRender(double dt)
        {
            graphicsDevice.Clear(ClearBuffers.Color);

            graphicsDevice.VertexArray = vertexBuffer;
            graphicsDevice.ShaderProgram = shaderProgram;
            graphicsDevice.DrawArrays(PrimitiveType.Triangles, 0, 3);

            Window.SwapBuffers();
        }

        protected override void OnResized(System.Drawing.Size size)
        {
            if (size.Width == 0 || size.Height == 0)
                return;

            graphicsDevice.SetViewport(0, 0, (uint)size.Width, (uint)size.Height);
        }

        protected override void OnUnload()
        {
            vertexBuffer.Dispose();
            shaderProgram.Dispose();
        }
    }
}
