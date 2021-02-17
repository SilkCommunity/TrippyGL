using System;
using System.Numerics;
using Silk.NET.Maths;
using TrippyGL;
using TrippyGL.ImageSharp;
using TrippyTestBase;

namespace SimpleCubemap
{
    // Loads the images in the cubemap folder into a TextureCubemap and displays it as a skybox.
    // The camera can be moved to look around by moving the mouse while holding the left button.
    // A gamepad also works for moving the camera. (the cubemap's images are somewhat low-res)

    class SimpleCubemap : TestBase
    {
        TextureCubemap cubemap;
        ShaderProgram shaderProgram;
        ShaderUniform viewUniform;
        VertexBuffer<VertexPosition> vertexBuffer;

        InputManager3D inputManager;

        protected override void OnLoad()
        {
            inputManager = new InputManager3D(InputContext);

            cubemap = TextureCubemapExtensions.FromFiles(
                graphicsDevice,
                "cubemap/back.png", "cubemap/front.png",
                "cubemap/bottom.png", "cubemap/top.png",
                "cubemap/left.png", "cubemap/right.png"
            );
            cubemap.SetTextureFilters(TextureMinFilter.Linear, TextureMagFilter.Linear);

            shaderProgram = ShaderProgram.FromFiles<VertexPosition>(graphicsDevice, "vs.glsl", "fs.glsl", "vPosition");

            shaderProgram.Uniforms["cubemap"].SetValueTexture(cubemap);
            shaderProgram.Uniforms["World"].SetValueMat4(Matrix4x4.Identity);
            viewUniform = shaderProgram.Uniforms["View"];
            viewUniform.SetValueMat4(Matrix4x4.Identity);

            Span<VertexPosition> vertexData = stackalloc VertexPosition[]
            {
                new Vector3(-0.5f, -0.5f, -0.5f),//4
                new Vector3(-0.5f, -0.5f, 0.5f),//3
                new Vector3(-0.5f, 0.5f, -0.5f),//7
                new Vector3(-0.5f, 0.5f, 0.5f),//8
                new Vector3(0.5f, 0.5f, 0.5f),//5
                new Vector3(-0.5f, -0.5f, 0.5f),//3
                new Vector3(0.5f, -0.5f, 0.5f),//1
                new Vector3(-0.5f, -0.5f, -0.5f),//4
                new Vector3(0.5f, -0.5f, -0.5f),//2
                new Vector3(-0.5f, 0.5f, -0.5f),//7
                new Vector3(0.5f, 0.5f, -0.5f),//6
                new Vector3(0.5f, 0.5f, 0.5f),//5
                new Vector3(0.5f, -0.5f, -0.5f),//2
                new Vector3(0.5f, -0.5f, 0.5f),//1
            };

            vertexBuffer = new VertexBuffer<VertexPosition>(graphicsDevice, vertexData, BufferUsage.StaticDraw);

            graphicsDevice.DepthTestingEnabled = false;
            graphicsDevice.BlendingEnabled = false;
        }

        protected override void OnUpdate(double dt)
        {
            base.OnUpdate(dt);
        }

        protected override void OnRender(double dt)
        {
            inputManager.Update((float)dt);

            graphicsDevice.ShaderProgram = shaderProgram;
            viewUniform.SetValueMat4(inputManager.CalculateViewMatrixNoTranslation());
            graphicsDevice.VertexArray = vertexBuffer;
            graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, vertexBuffer.StorageLength);
        }

        protected override void OnResized(Vector2D<int> size)
        {
            if (size.X == 0 || size.Y == 0)
                return;

            graphicsDevice.SetViewport(0, 0, (uint)size.X, (uint)size.Y);
            shaderProgram.Uniforms["Projection"].SetValueMat4(Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 2f, size.X / (float)size.Y, 0.01f, 10f));
        }

        protected override void OnUnload()
        {
            vertexBuffer.Dispose();
            shaderProgram.Dispose();
            cubemap.Dispose();
        }
    }
}
