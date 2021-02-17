using System;
using System.Drawing;
using System.Numerics;
using Silk.NET.Maths;
using TrippyGL;
using TrippyTestBase;

namespace ComplexVertexFormats
{
    // Renders two triangles on a black background using a highly unusual vertex format.
    // You should see two right triangles that look like a rectangle got split up diagonally.
    // The bottom-left and top-right vertices should be red, the top-left vertices should be
    // blue and the bottom-right vertices should be green.

    // The ComplexVertex type requires 16 vertex attrib indices, so if your GPU has less than
    // that this will fail when creating the VertexArray.
    // My GTX 765m supports no more than 16 vertex attrib indices in case you're wondering.

    class ComplexVertexFormats : TestBase
    {
        VertexBuffer<ComplexVertex> vertexBuffer;
        ShaderProgram shaderProgram;

        protected override void OnLoad()
        {
            Span<ComplexVertex> vertices = stackalloc ComplexVertex[]
            {
                new ComplexVertex(new Vector3(-0.6f, -0.6f, 0), new Color4b(255, 0, 0, 255)),
                new ComplexVertex(new Vector3(0.4f, -0.6f, 0), new Color4b(0, 255, 0, 255)),
                new ComplexVertex(new Vector3(-0.6f, 0.4f, 0), new Color4b(0, 0, 255, 255)),

                new ComplexVertex(new Vector3(0.6f, -0.4f, 0), new Color4b(0, 255, 0, 255)),
                new ComplexVertex(new Vector3(-0.4f, 0.6f, 0), new Color4b(0, 0, 255, 255)),
                new ComplexVertex(new Vector3(0.6f, 0.6f, 0), new Color4b(255, 0, 0, 255)),
            };

            vertexBuffer = new VertexBuffer<ComplexVertex>(graphicsDevice, vertices, BufferUsage.StaticDraw);
            shaderProgram = ShaderProgram.FromFiles<ComplexVertex>(graphicsDevice, "vs1.glsl", "fs1.glsl", new string[] { "sixtyThree", "X", "nothing0", "colorR", "matrix1", "colorG", "sixtyFour", "Y", "colorB", "Z", "oneTwoThreeFour", "alwaysZero", "alsoZero" });

            shaderProgram.Uniforms["Projection"].SetValueMat4(Matrix4x4.Identity);

            graphicsDevice.BlendingEnabled = false;
            graphicsDevice.DepthTestingEnabled = false;
        }

        protected override void OnRender(double dt)
        {
            graphicsDevice.ClearColor = new Vector4(0, 0, 0, 1);
            graphicsDevice.Clear(ClearBuffers.Color);

            graphicsDevice.VertexArray = vertexBuffer;
            graphicsDevice.ShaderProgram = shaderProgram;
            graphicsDevice.DrawArrays(PrimitiveType.Triangles, 0, vertexBuffer.StorageLength);
        }

        protected override void OnResized(Vector2D<int> size)
        {
            if (size.X == 0 || size.Y == 0)
                return;

            graphicsDevice.SetViewport(0, 0, (uint)size.X, (uint)size.Y);
        }

        protected override void OnUnload()
        {
            vertexBuffer.Dispose();
            shaderProgram.Dispose();
        }
    }
}
