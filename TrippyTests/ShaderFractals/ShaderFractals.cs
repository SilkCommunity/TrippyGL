using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Maths;
using TrippyGL.ImageSharp;
using TrippyGL;
using TrippyTestBase;

namespace ShaderFractals
{
    // Renders a moving Julia fractal with purple colors all around.
    // The view can be moved around by moving the mouse while holding the left button and
    // the scale can be changed with the scroll wheel.
    // Pressing the spacebar pauses the animation and pressing the S key saves a screenshot as png.
    // You can also toggle fullscreen with F11 and reset the camera with the "Home" key

    class ShaderFractals : TestBase
    {
        Stopwatch stopwatch;

        VertexBuffer<VertexPosition> vertexBuffer;
        ShaderProgram shaderProgram;
        ShaderUniform transformUniform;
        ShaderUniform cUniform;

        Vector2 lastMousePos;
        float mouseMoveScale;

        Vector2 offset;
        float scaleExponent;
        float scale;

        protected override void OnLoad()
        {
            Span<VertexPosition> vertexData = stackalloc VertexPosition[]
            {
                new Vector3(-1, -1, 0),
                new Vector3(-1, 1, 0),
                new Vector3(1, -1, 0),
                new Vector3(1, 1, 0),
            };

            vertexBuffer = new VertexBuffer<VertexPosition>(graphicsDevice, vertexData, BufferUsage.StaticDraw);

            shaderProgram = ShaderProgram.FromFiles<VertexPosition>(graphicsDevice, "vs.glsl", "fs.glsl", "vPosition");

            transformUniform = shaderProgram.Uniforms["Transform"];
            cUniform = shaderProgram.Uniforms["c"];

            graphicsDevice.DepthTestingEnabled = false;
            graphicsDevice.BlendingEnabled = false;

            stopwatch = Stopwatch.StartNew();

            OnKeyDown(null, Key.Home, 0);
        }

        protected override void OnRender(double dt)
        {
            const float min = 0.27f, max = 0.264f, spd = 0.5f;
            float cx = min + (max - min) * (MathF.Sin((float)stopwatch.Elapsed.TotalSeconds * spd) + 1) * 0.5f;
            cUniform.SetValueVec2(new Vector2(cx, 0.0f));

            graphicsDevice.ShaderProgram = shaderProgram;
            graphicsDevice.VertexArray = vertexBuffer;
            graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, vertexBuffer.StorageLength);
        }

        protected override void OnMouseMove(IMouse sender, Vector2 position)
        {
            if (sender.IsButtonPressed(MouseButton.Left))
            {
                offset.X += (lastMousePos.X - position.X) * mouseMoveScale * scale;
                offset.Y += (position.Y - lastMousePos.Y) * mouseMoveScale * scale;
                lastMousePos = position;
                UpdateTransformMatrix();
            }
        }

        protected override void OnMouseDown(IMouse sender, MouseButton button)
        {
            if (button == MouseButton.Left)
                lastMousePos = sender.Position;
        }

        protected override void OnMouseScroll(IMouse sender, ScrollWheel scroll)
        {
            scaleExponent = Math.Clamp(scaleExponent + scroll.Y * 0.05f, -5.5f, 1.0f);
            scale = MathF.Pow(10, scaleExponent);
            UpdateTransformMatrix();
        }

        protected override void OnKeyDown(IKeyboard sender, Key key, int idk)
        {
            switch (key)
            {
                case Key.Home:
                    offset = new Vector2(-0.0504f, 0.2522f);
                    scaleExponent = 0.4f;
                    scale = MathF.Pow(10, scaleExponent);
                    UpdateTransformMatrix();
                    break;

                case Key.Space:
                    if (stopwatch.IsRunning)
                        stopwatch.Stop();
                    else
                        stopwatch.Start();
                    break;

                case Key.S:
                    TakeScreenshot();
                    break;
            }
        }

        protected override void OnResized(Vector2D<int> size)
        {
            if (size.X == 0 || size.Y == 0)
                return;

            graphicsDevice.SetViewport(0, 0, (uint)size.X, (uint)size.Y);
            if (size.X < size.Y)
            {
                shaderProgram.Uniforms["Projection"].SetValueMat4(Matrix4x4.CreateOrthographic(2f * size.X / size.Y, 2f, 0.01f, 10f));
                mouseMoveScale = 2f / size.Y;
            }
            else
            {
                shaderProgram.Uniforms["Projection"].SetValueMat4(Matrix4x4.CreateOrthographic(2f, 2f * size.Y / size.X, 0.01f, 10f));
                mouseMoveScale = 2f / size.X;
            }
        }

        protected override void OnUnload()
        {
            vertexBuffer.Dispose();
            shaderProgram.Dispose();
        }

        private void UpdateTransformMatrix()
        {
            Matrix3x2 mat = Matrix3x2.CreateScale(scale) * Matrix3x2.CreateTranslation(offset);
            transformUniform.SetValueMat3x2(mat);
            //window.Title = "offset=" + offset.ToString() + ", scale=" + scale.ToString() + ", scaleExponent=" + scaleExponent.ToString();
        }

        private unsafe void TakeScreenshot()
        {
            using Framebuffer2D fbo = new Framebuffer2D(graphicsDevice, (uint)Window.Size.X, (uint)Window.Size.Y, DepthStencilFormat.None);
            graphicsDevice.Framebuffer = fbo;
            OnRender(0);
            fbo.Framebuffer.SaveAsImage(GetFileName(), SaveImageFormat.Png, true);

            graphicsDevice.Framebuffer = null;

            static string GetFileName()
            {
                const string name = "screenshot";
                const string ext = ".png";

                if (!File.Exists(name + ext))
                    return name + ext;

                int i = 1;
                while (true)
                {
                    string n = name + i.ToString() + ext;
                    if (!File.Exists(n))
                        return n;
                    i++;
                }
            }
        }
    }
}
