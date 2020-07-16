using System;
using System.Drawing;
using System.IO;
using System.Numerics;
using Silk.NET.Input.Common;
using Silk.NET.OpenGL;
using TrippyGL;
using TrippyTestBase;

namespace GameOfLifeSim
{
    // Displays a Conway's Game of Life simulation. The camera can be moved around
    // by moving the mouse while holding the left button and scrolling to zoom in or out.
    // Pressing the R key will reset the simulation to a random state.

    class GameOfLifeSim : TestBase
    {
        const uint SimulationWidth = 2048, SimulationHeight = 1536;

        VertexBuffer<VertexTexture> vertexBuffer;

        Texture2D tex1, tex2;
        FramebufferObject fbo1, fbo2;

        ShaderProgram simProgram;
        ShaderUniform simPrevUniform;

        ShaderProgram drawProgram;
        ShaderUniform drawTransformUniform;
        ShaderUniform drawSampUniform;

        PointF lastMousePos;
        float mouseMoveScale;

        Vector2 offset;
        float scaleExponent;
        float scale;

        Random r;

        protected override void OnLoad()
        {
            Span<VertexTexture> vertices = stackalloc VertexTexture[]
            {
                new VertexTexture(new Vector3(-1, -1, 0), new Vector2(0, 1)),
                new VertexTexture(new Vector3(-1, 1, 0), new Vector2(0, 0)),
                new VertexTexture(new Vector3(1, -1, 0), new Vector2(1, 1)),
                new VertexTexture(new Vector3(1, 1, 0), new Vector2(1, 0))
            };

            vertexBuffer = new VertexBuffer<VertexTexture>(graphicsDevice, vertices, BufferUsageARB.StaticDraw);

            fbo1 = FramebufferObject.Create2D(ref tex1, graphicsDevice, SimulationWidth, SimulationHeight, DepthStencilFormat.None);
            fbo2 = FramebufferObject.Create2D(ref tex2, graphicsDevice, SimulationWidth, SimulationHeight, DepthStencilFormat.None);

            tex1.SetTextureFilters(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            tex2.SetTextureFilters(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            tex1.SetWrapModes(TextureWrapMode.Repeat, TextureWrapMode.Repeat);
            tex2.SetWrapModes(TextureWrapMode.Repeat, TextureWrapMode.Repeat);

            r = new Random();

            ShaderProgramBuilder programBuilder = new ShaderProgramBuilder();
            programBuilder.VertexShaderCode = File.ReadAllText("sim_vs.glsl");
            programBuilder.FragmentShaderCode = File.ReadAllText("sim_fs.glsl");
            programBuilder.SpecifyVertexAttribs<VertexTexture>(new string[] { "vPosition", "vTexCoords" });

            simProgram = programBuilder.Create(graphicsDevice, true);
            Console.WriteLine("VS Log: " + programBuilder.VertexShaderLog);
            Console.WriteLine("FS Log: " + programBuilder.FragmentShaderLog);
            Console.WriteLine("Program Log: " + programBuilder.ProgramLog);

            programBuilder.VertexShaderCode = File.ReadAllText("draw_vs.glsl");
            programBuilder.FragmentShaderCode = File.ReadAllText("draw_fs.glsl");
            drawProgram = programBuilder.Create(graphicsDevice, true);
            Console.WriteLine("VS Log: " + programBuilder.VertexShaderLog);
            Console.WriteLine("FS Log: " + programBuilder.FragmentShaderLog);
            Console.WriteLine("Program Log: " + programBuilder.ProgramLog);

            simProgram.Uniforms["pixelDelta"].SetValueVec2(1f / SimulationWidth, 1f / SimulationHeight);
            simPrevUniform = simProgram.Uniforms["previous"];
            drawTransformUniform = drawProgram.Uniforms["Transform"];
            drawSampUniform = drawProgram.Uniforms["samp"];

            graphicsDevice.BlendingEnabled = false;
            graphicsDevice.DepthTestingEnabled = false;

            OnKeyDown(null, Key.Home, 0);
            OnKeyDown(null, Key.R, 0);
        }

        protected override void OnRender(double dt)
        {
            graphicsDevice.VertexArray = vertexBuffer;

            graphicsDevice.Framebuffer = fbo2;
            graphicsDevice.SetViewport(0, 0, fbo1.Width, fbo1.Height);
            graphicsDevice.ShaderProgram = simProgram;
            simPrevUniform.SetValueTexture(tex1);
            graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, vertexBuffer.StorageLength);

            graphicsDevice.Framebuffer = null;
            graphicsDevice.ClearColor = new Vector4(0, 0, 0, 1);
            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit);
            graphicsDevice.SetViewport(0, 0, (uint)Window.Size.Width, (uint)Window.Size.Height);
            graphicsDevice.ShaderProgram = drawProgram;
            drawSampUniform.SetValueTexture(tex2);
            graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, vertexBuffer.StorageLength);

            FramebufferObject tmpFbo = fbo2;
            fbo2 = fbo1;
            fbo1 = tmpFbo;

            Texture2D tmpTex = tex2;
            tex2 = tex1;
            tex1 = tmpTex;

            Window.SwapBuffers();
        }

        protected override void OnResized(Size size)
        {
            if (size.Width == 0 || size.Height == 0)
                return;

            if (size.Width < size.Height)
            {
                drawProgram.Uniforms["Projection"].SetValueMat4(Matrix4x4.CreateOrthographic(2f * size.Width / size.Height, 2f, 0.01f, 10f));
                mouseMoveScale = 2f / size.Height;
            }
            else
            {
                drawProgram.Uniforms["Projection"].SetValueMat4(Matrix4x4.CreateOrthographic(2f, 2f * size.Height / size.Width, 0.01f, 10f));
                mouseMoveScale = 2f / size.Width;
            }

        }

        protected override void OnUnload()
        {
            vertexBuffer.Dispose();
            fbo1.Dispose();
            fbo2.Dispose();
            tex1.Dispose();
            tex2.Dispose();
            simProgram.Dispose();
            drawProgram.Dispose();
            graphicsDevice.Dispose();
        }

        protected override void OnMouseMove(IMouse sender, System.Drawing.PointF position)
        {
            if (sender.IsButtonPressed(MouseButton.Left))
            {
                offset.X += (position.X - lastMousePos.X) * mouseMoveScale / scale;
                offset.Y += (lastMousePos.Y - position.Y) * mouseMoveScale / scale;
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

        protected override void OnKeyDown(IKeyboard sender, Key key, int n)
        {
            switch (key)
            {
                case Key.Home:
                    offset = Vector2.Zero;
                    scaleExponent = 0.2f;
                    scale = MathF.Pow(10, scaleExponent);
                    UpdateTransformMatrix();
                    break;

                case Key.R:
                    Color4b[] noise = new Color4b[tex1.Width * tex1.Height];
                    for (int i = 0; i < noise.Length; i++)
                        noise[i] = new Color4b((byte)r.Next(256), (byte)r.Next(255), (byte)r.Next(255), 255);
                    tex1.SetData<Color4b>(noise);
                    break;
            }
        }

        private void UpdateTransformMatrix()
        {
            Matrix3x2 mat = Matrix3x2.CreateScale(SimulationWidth / (float)SimulationHeight, 1f) * Matrix3x2.CreateTranslation(offset) * Matrix3x2.CreateScale(scale);
            drawTransformUniform.SetValueMat3x2(mat);
            //Window.Title = "offset=" + offset.ToString() + ", scale=" + scale.ToString() + ", scaleExponent=" + scaleExponent.ToString();
        }
    }
}
