using System;
using System.Drawing;
using System.Numerics;
using Silk.NET.Input.Common;
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

        Framebuffer2D fbo1, fbo2;

        ShaderProgram simProgram;
        ShaderUniform simPrevUniform;

        SimpleShaderProgram drawProgram;

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

            vertexBuffer = new VertexBuffer<VertexTexture>(graphicsDevice, vertices, BufferUsage.StaticDraw);

            fbo1 = new Framebuffer2D(graphicsDevice, SimulationWidth, SimulationHeight, DepthStencilFormat.None);//FramebufferObject.Create2D(ref tex1, graphicsDevice, SimulationWidth, SimulationHeight, DepthStencilFormat.None);
            fbo2 = new Framebuffer2D(graphicsDevice, SimulationWidth, SimulationHeight, DepthStencilFormat.None);//FramebufferObject.Create2D(ref tex2, graphicsDevice, SimulationWidth, SimulationHeight, DepthStencilFormat.None);

            fbo1.Texture.SetTextureFilters(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            fbo2.Texture.SetTextureFilters(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            fbo1.Texture.SetWrapModes(TextureWrapMode.Repeat, TextureWrapMode.Repeat);
            fbo2.Texture.SetWrapModes(TextureWrapMode.Repeat, TextureWrapMode.Repeat);

            r = new Random();

            simProgram = ShaderProgram.FromFiles<VertexTexture>(graphicsDevice, "sim_vs.glsl", "sim_fs.glsl", new string[] { "vPosition", "vTexCoords" });
            drawProgram = SimpleShaderProgram.Create<VertexTexture>(graphicsDevice);

            simProgram.Uniforms["pixelDelta"].SetValueVec2(1f / SimulationWidth, 1f / SimulationHeight);
            simPrevUniform = simProgram.Uniforms["previous"];

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
            simPrevUniform.SetValueTexture(fbo1);
            graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, vertexBuffer.StorageLength);

            graphicsDevice.Framebuffer = null;
            graphicsDevice.ClearColor = new Vector4(0, 0, 0, 1);
            graphicsDevice.Clear(ClearBuffers.Color);
            graphicsDevice.SetViewport(0, 0, (uint)Window.Size.Width, (uint)Window.Size.Height);
            graphicsDevice.ShaderProgram = drawProgram;
            drawProgram.Texture = fbo2;
            graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, vertexBuffer.StorageLength);

            Framebuffer2D tmpFbo = fbo2;
            fbo2 = fbo1;
            fbo1 = tmpFbo;

            Window.SwapBuffers();
        }

        protected override void OnResized(Size size)
        {
            if (size.Width == 0 || size.Height == 0)
                return;

            if (size.Width < size.Height)
            {
                drawProgram.Projection = Matrix4x4.CreateOrthographic(2f * size.Width / size.Height, 2f, 0.01f, 10f);
                mouseMoveScale = 2f / size.Height;
            }
            else
            {
                drawProgram.Projection = Matrix4x4.CreateOrthographic(2f, 2f * size.Height / size.Width, 0.01f, 10f);
                mouseMoveScale = 2f / size.Width;
            }

        }

        protected override void OnUnload()
        {
            vertexBuffer.Dispose();
            fbo1.Dispose();
            fbo2.Dispose();
            simProgram.Dispose();
            drawProgram.Dispose();
        }

        protected override void OnMouseMove(IMouse sender, PointF position)
        {
            if (Window.IsClosing)
                return;

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
            if (Window.IsClosing)
                return;

            scaleExponent = Math.Clamp(scaleExponent + scroll.Y * 0.05f, -5.5f, 1.0f);
            scale = MathF.Pow(10, scaleExponent);
            UpdateTransformMatrix();
        }

        protected override void OnKeyDown(IKeyboard sender, Key key, int n)
        {
            if (Window.IsClosing)
                return;

            switch (key)
            {
                case Key.Home:
                    offset = Vector2.Zero;
                    scaleExponent = 0.2f;
                    scale = MathF.Pow(10, scaleExponent);
                    UpdateTransformMatrix();
                    break;

                case Key.R:
                    Color4b[] noise = new Color4b[fbo1.Width * fbo1.Height];
                    for (int i = 0; i < noise.Length; i++)
                        noise[i] = new Color4b((byte)r.Next(256), (byte)r.Next(255), (byte)r.Next(255), 255);
                    fbo1.Texture.SetData<Color4b>(noise);
                    break;
            }
        }

        private void UpdateTransformMatrix()
        {
            Matrix4x4 mat = Matrix4x4.CreateScale(SimulationWidth / (float)SimulationHeight, 1f, 1f) * Matrix4x4.CreateTranslation(offset.X, offset.Y, 0) * Matrix4x4.CreateScale(scale);
            drawProgram.SetView(mat, Vector3.Zero);
        }
    }
}
