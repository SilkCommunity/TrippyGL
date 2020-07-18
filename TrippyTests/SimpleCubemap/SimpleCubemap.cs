using Silk.NET.Input.Common;
using Silk.NET.OpenGL;
using System;
using System.IO;
using System.Numerics;
using TrippyGL;
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
        VertexBuffer<VertexPosition> vertexBuffer;

        IGamepad currentGamepad;
        Vector2 thumbstickDir;
        Vector2 cameraRot;
        System.Drawing.PointF lastMousePos;

        protected override void OnLoad()
        {
            cubemap = TextureCubemapExtensions.FromFiles(
                graphicsDevice,
                "cubemap/back.png", "cubemap/front.png",
                "cubemap/bottom.png", "cubemap/top.png",
                "cubemap/left.png", "cubemap/right.png"
            );
            cubemap.SetTextureFilters(TextureMinFilter.Linear, TextureMagFilter.Linear);

            ShaderProgramBuilder programBuilder = new ShaderProgramBuilder();
            programBuilder.VertexShaderCode = File.ReadAllText("vs.glsl");
            programBuilder.FragmentShaderCode = File.ReadAllText("fs.glsl");
            programBuilder.SpecifyVertexAttribs<VertexPosition>(new string[] { "vPosition" });
            shaderProgram = programBuilder.Create(graphicsDevice, true);
            Console.WriteLine("VS Log: " + programBuilder.VertexShaderLog);
            Console.WriteLine("FS Log: " + programBuilder.FragmentShaderLog);
            Console.WriteLine("Program Log: " + programBuilder.ProgramLog);

            shaderProgram.Uniforms["cubemap"].SetValueTexture(cubemap);
            shaderProgram.Uniforms["World"].SetValueMat4(Matrix4x4.Identity);
            shaderProgram.Uniforms["View"].SetValueMat4(Matrix4x4.Identity);

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

            vertexBuffer = new VertexBuffer<VertexPosition>(graphicsDevice, vertexData, BufferUsageARB.StaticDraw);

            graphicsDevice.DepthTestingEnabled = false;
            graphicsDevice.BlendingEnabled = false;
        }

        protected override void OnUpdate(double dt)
        {
            base.OnUpdate(dt);

            if (currentGamepad != null && currentGamepad.IsConnected)
            {
                const float sensitivity = 5f;
                cameraRot.Y += thumbstickDir.X * (float)dt * sensitivity;
                cameraRot.X = Math.Clamp(cameraRot.X + thumbstickDir.Y * (float)dt * sensitivity, -1.57f, 1.57f);
                UpdateCamera();
            }
        }

        protected override void OnRender(double dt)
        {
            graphicsDevice.ShaderProgram = shaderProgram;
            graphicsDevice.VertexArray = vertexBuffer;
            graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, vertexBuffer.StorageLength);

            Window.SwapBuffers();
        }

        protected override void OnMouseMove(IMouse sender, System.Drawing.PointF position)
        {
            if (sender.IsButtonPressed(MouseButton.Left))
            {
                const float sensitivity = 1f / 200f;
                cameraRot.Y += (position.X - lastMousePos.X) * sensitivity;
                cameraRot.X = Math.Clamp(cameraRot.X + (position.Y - lastMousePos.Y) * sensitivity, -1.57f, 1.57f);

                lastMousePos = position;

                UpdateCamera();
            }
        }

        protected override void OnMouseDown(IMouse sender, MouseButton button)
        {
            if (button == MouseButton.Left)
                lastMousePos = sender.Position;
        }

        protected override void OnGamepadThumbstickMoved(IGamepad sender, Thumbstick thumbstick)
        {
            if (thumbstick.Index == 0)
            {
                currentGamepad = sender;
                if (Math.Abs(thumbstick.Position) < 0.2)
                    thumbstickDir = default;
                else
                {
                    thumbstickDir.X = thumbstick.X;
                    thumbstickDir.Y = thumbstick.Y;
                }
            }
        }

        protected override void OnResized(System.Drawing.Size size)
        {
            if (size.Width == 0 || size.Height == 0)
                return;

            graphicsDevice.SetViewport(0, 0, (uint)size.Width, (uint)size.Height);
            shaderProgram.Uniforms["Projection"].SetValueMat4(Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 2f, size.Width / (float)size.Height, 0.01f, 10f));
        }

        private void UpdateCamera()
        {
            shaderProgram.Uniforms["View"].SetValueMat4(Matrix4x4.CreateRotationY(cameraRot.Y) * Matrix4x4.CreateRotationX(cameraRot.X));
        }

        protected override void OnUnload()
        {
            vertexBuffer.Dispose();
            shaderProgram.Dispose();
            cubemap.Dispose();
            graphicsDevice.Dispose();
        }
    }
}
