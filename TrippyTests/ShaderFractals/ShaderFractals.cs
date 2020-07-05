using Silk.NET.Input.Common;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Processing;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Numerics;
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

        PointF lastMousePos;
        float mouseMoveScale;

        Vector2 offset;
        float scaleExponent;
        float scale;

        protected override void OnLoad()
        {
            Span<VertexPosition> vertexData = stackalloc VertexPosition[]
            {
                new Vector3(-1f, -1f, 0),
                new Vector3(-1f, 1f, 0),
                new Vector3(1f, -1f, 0),
                new Vector3(1f, 1f, 0),
            };

            vertexBuffer = new VertexBuffer<VertexPosition>(graphicsDevice, vertexData, BufferUsageARB.StaticDraw);

            ShaderProgramBuilder programBuilder = new ShaderProgramBuilder();
            programBuilder.VertexShaderCode = File.ReadAllText("vs.glsl");
            programBuilder.FragmentShaderCode = File.ReadAllText("fs.glsl");
            programBuilder.SpecifyVertexAttribs<VertexPosition>(new string[] { "vPosition" });
            shaderProgram = programBuilder.Create(graphicsDevice, true);
            Console.WriteLine("VS Log: " + programBuilder.VertexShaderLog);
            Console.WriteLine("FS Log: " + programBuilder.FragmentShaderLog);
            Console.WriteLine("Program Log: " + programBuilder.ProgramLog);

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
            float cx = min + (max - min) * ((float)Math.Sin(stopwatch.Elapsed.TotalSeconds * spd) + 1) * 0.5f;
            cUniform.SetValueVec2(new Vector2(cx, 0.0f));

            graphicsDevice.ShaderProgram = shaderProgram;
            graphicsDevice.VertexArray = vertexBuffer;
            graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, vertexBuffer.StorageLength);

            Window.SwapBuffers();
        }

        protected override void OnMouseMove(IMouse sender, PointF position)
        {
            if (sender.IsButtonPressed(MouseButton.Left))
            {
                offset.X += (lastMousePos.X - position.X) * mouseMoveScale * scale;
                offset.Y += (position.Y - lastMousePos.Y) * mouseMoveScale * scale;
                lastMousePos = position;
                UpdateTransformMatrix();
            }
        }

        protected override void OnMouseDown(IMouse sender, MouseButton btn)
        {
            if (btn == MouseButton.Left)
                lastMousePos = sender.Position;
        }

        protected override void OnMouseScroll(IMouse sender, ScrollWheel scroll)
        {
            scaleExponent = Math.Clamp(scaleExponent + scroll.Y * 0.05f, -5.5f, 1.0f);
            scale = (float)Math.Pow(10, scaleExponent);
            UpdateTransformMatrix();
        }

        protected override void OnKeyDown(IKeyboard sender, Key key, int idk)
        {
            switch (key)
            {
                case Key.Home:
                    offset = new Vector2(-0.0504f, 0.2522f);
                    scaleExponent = 0.4f;
                    scale = (float)Math.Pow(10, scaleExponent);
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

        protected override void OnResized(Size size)
        {
            if (size.Width == 0 || size.Height == 0)
                return;

            graphicsDevice.SetViewport(0, 0, (uint)size.Width, (uint)size.Height);
            if (size.Width < size.Height)
            {
                shaderProgram.Uniforms["Projection"].SetValueMat4(Matrix4x4.CreateOrthographic(2f * size.Width / size.Height, 2f, 0.01f, 10f));
                mouseMoveScale = 2f / size.Height;
            }
            else
            {
                shaderProgram.Uniforms["Projection"].SetValueMat4(Matrix4x4.CreateOrthographic(2f, 2f * size.Height / size.Width, 0.01f, 10f));
                mouseMoveScale = 2f / size.Width;
            }
        }

        protected override void OnUnload()
        {
            vertexBuffer.Dispose();
            shaderProgram.Dispose();
            graphicsDevice.Dispose();
        }

        private void UpdateTransformMatrix()
        {
            Matrix3x2 mat = Matrix3x2.CreateScale(scale) * Matrix3x2.CreateTranslation(offset);
            transformUniform.SetValueMat3x2(mat);
            //window.Title = "offset=" + offset.ToString() + ", scale=" + scale.ToString() + ", scaleExponent=" + scaleExponent.ToString();
        }

        private unsafe void TakeScreenshot()
        {
            // We could normally use the FramebufferObject.SaveAsImage() extension from
            // TrippyGL.ImageSharp, but since we want to save the default framebuffer we
            // have to do this manually.

            using Image<SixLabors.ImageSharp.PixelFormats.Rgba32> image = new Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(Window.Size.Width, Window.Size.Height);

            graphicsDevice.Framebuffer = null;
            fixed (void* ptr = image.GetPixelSpan())
                graphicsDevice.GL.ReadPixels(0, 0, (uint)Window.Size.Width, (uint)Window.Size.Height, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
            image.Mutate(x => x.Flip(FlipMode.Vertical));

            string file = GetFileName();
            using FileStream fileStream = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Read);
            image.SaveAsPng(fileStream);

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
