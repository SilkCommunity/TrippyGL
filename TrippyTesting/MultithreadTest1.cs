using System;
using System.IO;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using System.Drawing.Imaging;
using TrippyGL;
using System.Threading;

namespace TrippyTesting
{
    class MultithreadTest1 : GameWindow
    {
        Random r = new Random();
        System.Diagnostics.Stopwatch stopwatch;
        float time;

        GraphicsDevice graphicsDevice;

        Thread thread;

        VertexBuffer<VertexColorTexture> buffer;
        ShaderProgram program;

        Texture2D tex2d;
        Texture1D tex1d;

        public bool isDoneLoading = false;
        public bool markDoneLoading = false;

        public MultithreadTest1() : base(1280, 720, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 0, 0, 0, ColorFormat.Empty, 2), "haha yes", GameWindowFlags.Default, DisplayDevice.Default, 4, 0, GraphicsContextFlags.Default)
        {
            this.Title = "Loading...";
            VSync = VSyncMode.On;
            graphicsDevice = new GraphicsDevice(this.Context);
            graphicsDevice.DebugMessagingEnabled = true;
            graphicsDevice.DebugMessage += Program.OnDebugMessage;

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

            VertexColorTexture[] vboData = new VertexColorTexture[]
            {
                new VertexColorTexture(new Vector3(-0.5f, -0.5f, 0), new Color4b(255, 255, 255, 255), new Vector2(0, 0)),
                new VertexColorTexture(new Vector3(-0.5f, 0.5f, 0), new Color4b(255, 255, 255, 255), new Vector2(0, 1)),
                new VertexColorTexture(new Vector3(0.5f, -0.5f, 0), new Color4b(255, 255, 255, 255), new Vector2(1, 0)),
                new VertexColorTexture(new Vector3(0.5f, 0.5f, 0), new Color4b(255, 255, 255, 255), new Vector2(1, 1))
            };
            buffer = new VertexBuffer<VertexColorTexture>(graphicsDevice, vboData, BufferUsageHint.StaticDraw);

            GL.Finish();

            this.Context.MakeCurrent(null);

            thread = new Thread(LoadThread);
            thread.Start();

            while (!markDoneLoading) ;
            this.Context.MakeCurrent(this.WindowInfo);
            GL.Finish();
            graphicsDevice.MakeMine(tex2d);
            graphicsDevice.MakeMine(tex1d);
            graphicsDevice.MakeMine(program);
            graphicsDevice.ResetStates();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            time = (float)stopwatch.Elapsed.TotalSeconds;
            ErrorCode c;
            while ((c = GL.GetError()) != ErrorCode.NoError)
            {
                Console.WriteLine("Error found: " + c);
            }

            if (markDoneLoading)
            {
                isDoneLoading = true;
                this.Title = "Multithreaded loading! :D";
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            buffer.EnsureArrayBound();

            program.Uniforms["samp"].SetValueTexture(tex2d);
            program.Uniforms["samp1d"].SetValueTexture(tex1d);
            program.EnsurePreDrawStates();
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, buffer.StorageLength);

            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, this.Width, this.Height);
            Matrix4 mat = Matrix4.CreateOrthographicOffCenter(-1, 1, 1, -1, -1, 1);
            program.Uniforms["Proj"].SetValueMat4(ref mat);
        }

        protected override void OnUnload(EventArgs e)
        {
            buffer.Dispose();
            program.Dispose();
            tex2d.Dispose();

            graphicsDevice.Dispose();
        }

        private void LoadThread()
        {
            GLControl control = new GLControl(new GraphicsMode(ColorFormat.Empty, 0, 0, 0, ColorFormat.Empty, 0), graphicsDevice.GLMajorVersion, graphicsDevice.GLMinorVersion, GraphicsContextFlags.Offscreen);
            control.Context.MakeCurrent(control.WindowInfo);
            GraphicsDevice loadingDevice = new GraphicsDevice(control.Context);

            Console.WriteLine("thread before " + GL.GetError());
            tex2d = new Texture2D(loadingDevice, "data/YARN.png");
            tex1d = new Texture1D(loadingDevice, "dataa3/tex1d.png");
            tex1d.SetTextureFilters(TextureMinFilter.Linear, TextureMagFilter.Linear);
            //Thread.Sleep(5000);
            program = new ShaderProgram(loadingDevice);
            program.AddVertexShader("#version 400\r\nuniform mat4 Proj; in vec3 vP; in vec4 vC; in vec2 vT; out vec4 fC; out vec2 fT; void main() { gl_Position = Proj * vec4(vP, 1.0); fC = vC; fT = vT; }");
            program.AddFragmentShader("#version 400\r\nuniform sampler2D samp; uniform sampler1D samp1d; in vec4 fC; in vec2 fT; out vec4 FragColor; void main() { FragColor = fC * texture(samp, fT) * texture(samp1d, fT.x); }");
            program.SpecifyVertexAttribs<VertexColorTexture>(new string[] { "vP", "vC", "vT" });
            program.LinkProgram();

            Console.WriteLine("thread after " + GL.GetError());

            control.Context.MakeCurrent(null);
            control.Context.Dispose();

            markDoneLoading = true;
        }
    }
}
