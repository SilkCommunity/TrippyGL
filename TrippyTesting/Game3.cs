using System;
using System.IO;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using System.Drawing.Imaging;
using TrippyGL;

namespace TrippyTesting
{
    class Game3 : GameWindow
    {
        Random r = new Random();
        System.Diagnostics.Stopwatch stopwatch;
        float time;

        GraphicsDevice graphicsDevice;

        ShaderProgram program;
        VertexBuffer<VertexColorTexture> buffer;
        Texture2D tex2d;
        Texture1D tex1d, otherTex1d;

        Texture2D fboTexture;

        ShaderProgram program3d;
        VertexBuffer<VertexColor> batchBuffer;
        PrimitiveBatcher<VertexColor> batcher;

        int fbo, rbo;

        public Game3() : base(1280, 720, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 0, 0, 0, ColorFormat.Empty, 2), "haha yes", GameWindowFlags.Default, DisplayDevice.Default, 4, 0, GraphicsContextFlags.Default)
        {
            VSync = VSyncMode.On;
            TrippyLib.Init();
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

            tex2d = new Texture2D(graphicsDevice, "data/YARN.png");
            GL.GenerateMipmap((GenerateMipmapTarget)tex2d.TextureType);
            tex2d.SetTextureFilters(TextureMinFilter.NearestMipmapLinear, TextureMagFilter.Nearest);

            tex1d = new Texture1D(graphicsDevice, "dataa3/tex1d.png");
            otherTex1d = new Texture1D(graphicsDevice, 1);
            otherTex1d.SetData(new Color4b[] { Color4b.White });

            program = new ShaderProgram(graphicsDevice);
            program.AddVertexShader(File.ReadAllText("dataa3/vs.glsl"));
            program.AddFragmentShader(File.ReadAllText("dataa3/fs.glsl"));
            program.SpecifyVertexAttribs<VertexColorTexture>(new string[] { "vPosition", "vColor", "vTexCoords" });
            program.LinkProgram();

            VertexColorTexture[] vboData = new VertexColorTexture[]
            {
                new VertexColorTexture(new Vector3(-0.5f, -0.5f, 0), new Color4b(255, 0, 0, 255), new Vector2(0, 1)),
                new VertexColorTexture(new Vector3(-0.5f, 0.5f, 0), new Color4b(0, 255, 0, 255), new Vector2(0, 0)),
                new VertexColorTexture(new Vector3(0.5f, -0.5f, 0), new Color4b(0, 0, 255, 255), new Vector2(1, 1)),
                new VertexColorTexture(new Vector3(0.5f, 0.5f, 0), new Color4b(255, 255, 0, 255), new Vector2(1, 0))
            };

            buffer = new VertexBuffer<VertexColorTexture>(graphicsDevice, vboData.Length, 0, vboData, BufferUsageHint.StaticDraw);



            batchBuffer = new VertexBuffer<VertexColor>(graphicsDevice, 512, BufferUsageHint.StreamDraw);
            batcher = new PrimitiveBatcher<VertexColor>(512, 64);
            program3d = new ShaderProgram(graphicsDevice);
            program3d.AddVertexShader(File.ReadAllText("dataa3/vs3d.glsl"));
            program3d.AddFragmentShader(File.ReadAllText("dataa3/fs3d.glsl"));
            program3d.SpecifyVertexAttribs<VertexColor>(new string[] { "vPosition", "vColor" });
            program3d.LinkProgram();
            Matrix4 mat = Matrix4.Identity;
            program3d.Uniforms["World"].SetValueMat4(ref mat);
            program3d.Uniforms["View"].SetValueMat4(ref mat);
            program3d.Uniforms["Projection"].SetValueMat4(ref mat);

            fboTexture = new Texture2D(graphicsDevice, this.Width, this.Height);
            fbo = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, fboTexture.TextureType, fboTexture.Handle, 0);

            rbo = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rbo);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, this.Width, this.Height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, rbo);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
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

            BlendMode.AlphaBlend.Apply();
            Matrix4 mat;
            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.ClearDepth(1f);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
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
            mat = Matrix4.CreateRotationY(time) * Matrix4.CreateScale(0.9f, 2.2f, 0.9f);
            batcher.AddTriangleStrip(MultiplyAllToNew(cone, ref mat));
            batcher.WriteTrianglesTo(batchBuffer.DataBuffer);
            batcher.ClearTriangles();

            batchBuffer.EnsureArrayBound();
            program3d.EnsurePreDrawStates();
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, batchBuffer.StorageLength);



            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            buffer.EnsureArrayBound();
            program.Uniforms["time"].SetValue1(time);
            mat = Matrix4.CreateScale(0.7f) * Matrix4.CreateTranslation(-0.5f, 0f, 0f);
            program.Uniforms["World"].SetValueMat4(ref mat);
            program.Uniforms["samp2d"].SetValueTexture(tex2d);
            program.Uniforms["samp1d"].SetValueTexture(otherTex1d);
            program.EnsurePreDrawStates();

            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, buffer.StorageLength);

            mat = Matrix4.CreateScale(0.7f) * Matrix4.CreateTranslation(0.5f, 0f, 0f);
            program.Uniforms["World"].SetValueMat4(ref mat);
            program.Uniforms["samp2d"].SetValueTexture(fboTexture);
            program.EnsurePreDrawStates();

            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, buffer.StorageLength);

            SwapBuffers();
        }

        protected override void OnUnload(EventArgs e)
        {
            program.Dispose();
            tex2d.Dispose();
            tex1d.Dispose();
            buffer.Dispose();

            TrippyLib.Quit();
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, this.Width, this.Height);
            Matrix4 mat = Matrix4.Identity;
            program.Uniforms["Projection"].SetValueMat4(ref mat);
            program.Uniforms["View"].SetValueMat4(ref mat);
            program.Uniforms["World"].SetValueMat4(ref mat);

            mat = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver2, this.Width / (float)this.Height, 0.0001f, 100f);
            program3d.Uniforms["Projection"].SetValueMat4(ref mat);
            mat = Matrix4.LookAt(new Vector3(0.5f, -0.5f, 0.5f), Vector3.Zero, -Vector3.UnitY);
            program3d.Uniforms["View"].SetValueMat4(ref mat);

            fboTexture.RecreateImage(this.Width, this.Height);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rbo);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, this.Width, this.Height);
        }



        public static VertexColor[] MultiplyAllToNew(VertexColor[] vertex, ref Matrix4 mat)
        {
            VertexColor[] arr = new VertexColor[vertex.Length];
            for (int i = 0; i < vertex.Length; i++)
            {
                Vector4 t = new Vector4(vertex[i].Position, 1f);
                Vector4.Transform(ref t, ref mat, out t);
                arr[i].Position = t.Xyz;
                arr[i].Color = vertex[i].Color;
            }
            return arr;
        }

    }
}
