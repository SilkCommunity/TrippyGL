using System;
using System.IO;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using System.Drawing.Imaging;
using TrippyGL;

namespace TrippyTesting.Tests
{
    class FramebufferTest : GameWindow
    {
        Random r = new Random();
        System.Diagnostics.Stopwatch stopwatch;
        float time;

        GraphicsDevice graphicsDevice;

        ShaderProgram program;
        Texture2D tex1;

        VertexArray array;
        VertexDataBufferObject<VertexTexture> buffer1pos;
        VertexDataBufferObject<Color4b> buffer1color;

        ShaderProgram program3d;
        VertexBuffer<VertexColor> buffer3d;
        PrimitiveBatcher<VertexColor> batcher3d;

        Framebuffer2D fb1, fb2;

        MouseState ms, oldms;

        public FramebufferTest() : base(1280, 720, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 0, 0, 0, ColorFormat.Empty, 2), "haha yes", GameWindowFlags.Default, DisplayDevice.Default, 4, 0, GraphicsContextFlags.Default)
        {
            VSync = VSyncMode.On;
            graphicsDevice = new GraphicsDevice(this.Context);

            Console.WriteLine(String.Concat("GL Version: ", graphicsDevice.GLMajorVersion, ".", graphicsDevice.GLMinorVersion));
            Console.WriteLine("GL Version String: " + graphicsDevice.GLVersion);
            Console.WriteLine("GL Vendor: " + graphicsDevice.GLVendor);
            Console.WriteLine("GL Renderer: " + graphicsDevice.GLRenderer);
            Console.WriteLine("GL ShadingLanguageVersion: " + graphicsDevice.GLShadingLanguageVersion);
            Console.WriteLine("GL TextureUnits: " + graphicsDevice.MaxTextureImageUnits);
            Console.WriteLine("GL MaxTextureSize: " + graphicsDevice.MaxTextureSize);
            Console.WriteLine("GL MaxSamples:" + graphicsDevice.MaxSamples);
        }

        protected override void OnLoad(EventArgs e)
        {
            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            time = 0;

            graphicsDevice = new GraphicsDevice(this.Context);

            program = new ShaderProgram(graphicsDevice);
            program.AddVertexShader(File.ReadAllText("fbotest/simplevs.glsl"));
            program.AddFragmentShader(File.ReadAllText("fbotest/simplefs.glsl"));
            program.SpecifyVertexAttribs<VertexColorTexture>(new string[] { "vPosition", "vColor", "vTexCoords" });
            program.LinkProgram();

            buffer1pos = new VertexDataBufferObject<VertexTexture>(graphicsDevice, new VertexTexture[]
            {
                new VertexTexture(new Vector3(-0.5f, -0.5f, 0), new Vector2(0, 0)),
                new VertexTexture(new Vector3(0.5f, -0.5f, 0), new Vector2(1, 0)),
                new VertexTexture(new Vector3(-0.5f, 0.5f, 0), new Vector2(0, 1)),
                new VertexTexture(new Vector3(0.5f, 0.5f, 0), new Vector2(1, 1))
            }, BufferUsageHint.DynamicDraw);

            buffer1color = new VertexDataBufferObject<Color4b>(graphicsDevice, new Color4b[]
            {
                Color4b.White, Color4b.White, Color4b.White, Color4b.White
            }, BufferUsageHint.DynamicDraw);

            array = new VertexArray(graphicsDevice, new VertexAttribSource[]
            {
                new VertexAttribSource(buffer1pos, ActiveAttribType.FloatVec3),
                new VertexAttribSource(buffer1color, ActiveAttribType.FloatVec4, true, VertexAttribPointerType.UnsignedByte),
                new VertexAttribSource(buffer1pos, ActiveAttribType.FloatVec2)
            });

            program3d = new ShaderProgram(graphicsDevice);
            program3d.AddVertexShader(File.ReadAllText("dataa3/vs3d.glsl"));
            program3d.AddFragmentShader(File.ReadAllText("dataa3/fs3d.glsl"));
            program3d.SpecifyVertexAttribs<VertexColor>(new string[] { "vPosition", "vColor" });
            program3d.LinkProgram();

            buffer3d = new VertexBuffer<VertexColor>(graphicsDevice, 512, BufferUsageHint.StreamDraw);
            batcher3d = new PrimitiveBatcher<VertexColor>(512, 0);

            tex1 = new Texture2D(graphicsDevice, "data/texture.png");
            program.Uniforms["samp"].SetValueTexture(tex1);

            fb1 = new Framebuffer2D(graphicsDevice, this.Width, this.Height, DepthStencilFormat.Depth24Stencil8);
            fb2 = new Framebuffer2D(graphicsDevice, this.Width, this.Height, DepthStencilFormat.Depth24);

            GL.ClearColor(0f, 0f, 0f, 1f);
            graphicsDevice.EnsureFramebufferBound(fb1);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            graphicsDevice.EnsureFramebufferBound(fb2);
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            oldms = ms;
            ms = Mouse.GetState();

            time = (float)stopwatch.Elapsed.TotalSeconds;
            ErrorCode c;
            while ((c = GL.GetError()) != ErrorCode.NoError)
            {
                Console.WriteLine("Error found: " + c);
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Enable(EnableCap.DepthTest);
            Matrix4 mat;

            graphicsDevice.EnsureFramebufferBound(fb1);
            GL.ClearColor(0.1f + (time % 0.2f), 0.2f, 0.2f, 1f);
            GL.ClearDepth(1f);
            //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            mat = Matrix4.CreateScale(0.4f, 0.8f, 0.4f) * Matrix4.CreateTranslation(0.25f, 0.5f, -0.9996f);
            program.Uniforms["World"].SetValueMat4(ref mat);
            mat = Matrix4.Identity;
            program.Uniforms["View"].SetValueMat4(ref mat);
            program.Uniforms["samp"].SetValueTexture(tex1);
            program.EnsurePreDrawStates();
            graphicsDevice.EnsureVertexArrayBound(array);
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
            VertexColor[] cone = new VertexColor[]
            {
                new VertexColor(new Vector3(-1, 0, -1), new Color4b(255, 0, 0, 255)),
                new VertexColor(new Vector3(-1, 0, 1), new Color4b(0, 255, 0, 255)),
                new VertexColor(new Vector3(0, 1, 0), new Color4b(0, 0, 255, 255)),
                new VertexColor(new Vector3(1, 0, 1), new Color4b(255, 0, 0, 255)),
                new VertexColor(new Vector3(1, 0, -1), new Color4b(0, 255, 0, 255)),
                new VertexColor(new Vector3(-1, 0, -1), new Color4b(0, 0, 255, 255)),
                new VertexColor(new Vector3(0, 1, 0), new Color4b(0, 0, 255, 255)),
            };
            batcher3d.AddTriangleStrip(cone);
            mat = Matrix4.CreateScale(0.4f, 1.3f, 0.4f) * Matrix4.CreateRotationY(time) * Matrix4.CreateTranslation(0, ms.Y / (float)this.Height * 3f, ms.X / (float)this.Width * 3f);
            program3d.Uniforms["World"].SetValueMat4(ref mat);
            mat = Matrix4.LookAt(new Vector3(2, 1, 0), Vector3.Zero, Vector3.UnitY);
            program3d.Uniforms["View"].SetValueMat4(ref mat);
            mat = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver2, (float)this.Width / this.Height, 0.001f, 10f);
            program3d.Uniforms["Projection"].SetValueMat4(ref mat);
            batcher3d.WriteTrianglesTo(buffer3d.DataBuffer);
            graphicsDevice.EnsureVertexArrayBound(buffer3d.VertexArray);
            program3d.EnsurePreDrawStates();
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, batcher3d.TriangleVertexCount);
            batcher3d.ClearTriangles();

            mat = Matrix4.CreateScale(0.8f, 0.4f, 0.8f) * Matrix4.CreateRotationZ(MathHelper.PiOver2) * Matrix4.CreateTranslation(0.75f, 0.5f, 0);
            program.Uniforms["World"].SetValueMat4(ref mat);
            program.Uniforms["samp"].SetValueTexture(fb2.Texture);
            program.EnsurePreDrawStates();
            graphicsDevice.EnsureVertexArrayBound(array);
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

            graphicsDevice.EnsureFramebufferBoundDraw(null);
            graphicsDevice.EnsureFramebufferBoundRead(fb1);
            GL.BlitFramebuffer(0, 0, this.Width, this.Height, 0, 0, this.Width, this.Height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);

            Framebuffer2D tmp = fb1;
            fb1 = fb2;
            fb2 = tmp;

            if(ms.MiddleButton == ButtonState.Pressed && oldms.MiddleButton == ButtonState.Released)
            {
                fb2.SaveAsImage("xdxd" + time + ".png", SaveImageFormat.Png);
                fb2.Texture.SaveAsImage("xdxd" + time + "TEX.png", SaveImageFormat.Png);
            }

            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, this.Width, this.Height);
            Matrix4 mat = Matrix4.CreateOrthographicOffCenter(0, 1, 1, 0, 0, 1);
            program.Uniforms["Projection"].SetValueMat4(ref mat);
            mat = Matrix4.Identity;
            program.Uniforms["View"].SetValueMat4(ref mat);
            program.Uniforms["World"].SetValueMat4(ref mat);

            fb1.ReacreateFramebuffer(this.Width, this.Height);
            fb2.ReacreateFramebuffer(this.Width, this.Height);
        }

        protected override void OnUnload(EventArgs e)
        {
            fb1.Dispose();
            fb2.Dispose();
            buffer1pos.Dispose();
            buffer1color.Dispose();
            array.Dispose();
            program.Dispose();
            tex1.Dispose();

            graphicsDevice.Dispose();
        }
    }
}
