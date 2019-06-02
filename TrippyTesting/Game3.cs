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

        float scale = 1;
        ShaderProgram program;
        VertexBuffer<VertexColorTexture> buffer;
        Texture2D tex2d;
        Texture1D tex1d;

        public Game3() : base(1280, 720, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 0, 0, 8, ColorFormat.Empty, 2), "haha yes", GameWindowFlags.Default, DisplayDevice.Default, 4, 0, GraphicsContextFlags.Default)
        {
            VSync = VSyncMode.On;
            TrippyLib.Init();

            Console.WriteLine(String.Concat("GL Version: ", TrippyLib.GLMajorVersion, ".", TrippyLib.GLMinorVersion));
            Console.WriteLine("GL Version String: " + TrippyLib.GLVersion);
            Console.WriteLine("GL Vendor: " + TrippyLib.GLVendor);
            Console.WriteLine("GL Renderer: " + TrippyLib.GLRenderer);
            Console.WriteLine("GL ShadingLanguageVersion: " + TrippyLib.GLShadingLanguageVersion);
            Console.WriteLine("GL TextureUnits: " + TrippyLib.MaxTextureImageUnits);
            Console.WriteLine("GL MaxTextureSize: " + TrippyLib.MaxTextureSize);
            Console.WriteLine("GL MaxSamples:" + TrippyLib.MaxSamples);
        }

        protected override void OnLoad(EventArgs e)
        {
            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            time = 0;

            tex2d = new Texture2D("data/YARN.png");
            GL.GenerateMipmap((GenerateMipmapTarget)tex2d.TextureType);
            tex2d.SetTextureFilters(TextureMinFilter.NearestMipmapNearest, TextureMagFilter.Nearest);

            tex1d = new Texture1D("dataa3/tex1d.png");

            program = new ShaderProgram();
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

            buffer = new VertexBuffer<VertexColorTexture>(vboData.Length, 0, vboData, BufferUsageHint.StaticDraw);
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
            BlendMode.AlphaBlend.Apply();

            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            buffer.EnsureArrayBound();
            Matrix4 mat = Matrix4.CreateScale(scale);
            program.Uniforms["World"].SetValueMat4(ref mat);
            program.Uniforms["samp2d"].SetValueTexture(tex2d);
            program.Uniforms["samp1d"].SetValueTexture(tex1d);
            program.Uniforms.EnsureSamplerUniformsSet();

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
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            scale += e.DeltaPrecise * 0.1f;
            scale = MathHelper.Clamp(scale, 0.01f, 10f);
        }
    }
}
