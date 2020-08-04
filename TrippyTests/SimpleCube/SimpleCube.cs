using Silk.NET.OpenGL;
using System;
using System.Diagnostics;
using System.Numerics;
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

        protected override void OnLoad()
        {
            Span<VertexColor> cubemapBufferData = stackalloc VertexColor[] {
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

            vertexBuffer = new VertexBuffer<VertexColor>(graphicsDevice, cubemapBufferData, BufferUsageARB.StaticCopy);

            SimpleShaderProgramBuilder programBuilder = new SimpleShaderProgramBuilder()
            {
                VertexColorsEnabled = true
            };
            programBuilder.ConfigureVertexAttribs<VertexColor>();
            shaderProgram = programBuilder.Create(graphicsDevice, true);
            Console.WriteLine("VS Log: " + programBuilder.VertexShaderLog);
            Console.WriteLine("FS Log: " + programBuilder.FragmentShaderLog);
            Console.WriteLine("Program Log: " + programBuilder.ProgramLog);

            shaderProgram.View = Matrix4x4.CreateLookAt(new Vector3(0, 1.0f, -1.5f), Vector3.Zero, Vector3.UnitY);

            graphicsDevice.DepthState = DepthTestingState.Default;
            graphicsDevice.BlendState = BlendState.Opaque;

            stopwatch = Stopwatch.StartNew();
        }

        protected override void OnRender(double dt)
        {
            graphicsDevice.ClearDepth = 1f;
            graphicsDevice.ClearColor = Vector4.Zero;
            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            shaderProgram.World = Matrix4x4.CreateRotationY(2 * (float)stopwatch.Elapsed.TotalSeconds);
            graphicsDevice.ShaderProgram = shaderProgram;
            graphicsDevice.VertexArray = vertexBuffer;

            graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, vertexBuffer.StorageLength);

            Window.SwapBuffers();
        }

        protected override void OnResized(System.Drawing.Size size)
        {
            if (size.Width == 0 || size.Height == 0)
                return;

            graphicsDevice.SetViewport(0, 0, (uint)size.Width, (uint)size.Height);
            shaderProgram.Projection = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 2f, size.Width / (float)size.Height, 0.01f, 100f);
        }

        protected override void OnUnload()
        {
            vertexBuffer.Dispose();
            shaderProgram.Dispose();
            graphicsDevice.Dispose();
        }
    }
}
