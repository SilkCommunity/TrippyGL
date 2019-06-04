using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.IO;
using System.Runtime.InteropServices;
using TrippyGL;

namespace TrippyTesting.Tests
{
    class StructPaddingTest : GameWindow
    {
        System.Diagnostics.Stopwatch stopwatch;
        Random r = new Random();
        float time;

        GraphicsDevice graphicsDevice;

        ShaderProgram program;
        Texture2D texture;

        VertexBuffer<WeirdAssVertex> buffer;

        public StructPaddingTest() : base(1280, 720, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 0, 0, 0, ColorFormat.Empty, 2), "struct padding", GameWindowFlags.Default, DisplayDevice.Default, 4, 0, GraphicsContextFlags.Default)
        {
            VSync = VSyncMode.On;
            TrippyLib.Init();
            graphicsDevice = new GraphicsDevice(this.Context);

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

            program = new ShaderProgram(graphicsDevice);
            program.AddVertexShader(File.ReadAllText("spad/vs.glsl"));
            program.AddFragmentShader(File.ReadAllText("spad/fs.glsl"));
            program.SpecifyVertexAttribs<WeirdAssVertex>(new string[]
            {
                "x", "y", "z", "x2", "y2", "z2", "w", "cx", "cy"
            });
            program.LinkProgram();
            Console.WriteLine("Program info log: \n" + GL.GetProgramInfoLog(program.Handle) + "\n[END OF LOG]");

            Matrix4 id = Matrix4.Identity;
            program.Uniforms["World"].SetValueMat4(ref id);
            program.Uniforms["View"].SetValueMat4(ref id);
            program.Uniforms["Projection"].SetValueMat4(ref id);

            WeirdAssVertex[] data = new WeirdAssVertex[]
            {
                new WeirdAssVertex(new Vector3(0.2f, 0.2f, 0), new Vector2(0, 0)),
                new WeirdAssVertex(new Vector3(0.8f, 0.2f, 0), new Vector2(1, 0)),
                new WeirdAssVertex(new Vector3(0.2f, 0.8f, 0), new Vector2(0, 1)),
                new WeirdAssVertex(new Vector3(0.8f, 0.8f, 0), new Vector2(1, 1)),
            };
            buffer = new VertexBuffer<WeirdAssVertex>(graphicsDevice, data.Length, data, BufferUsageHint.DynamicDraw);

            texture = new Texture2D(graphicsDevice, "data/fondo.png");
            program.Uniforms["samp"].SetValueTexture(texture);
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
            BlendMode.Opaque.Apply();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, this.Width, this.Height);
            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            buffer.EnsureArrayBound();
            States.EnsureShaderProgramInUse(program);
            program.Uniforms.EnsureSamplerUniformsSet();
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

            SwapBuffers();

            int slp = (int)(15f - (stopwatch.Elapsed.TotalSeconds - time) * 1000f);
            if (slp >= 0)
                System.Threading.Thread.Sleep(slp);
        }

        protected override void OnUnload(EventArgs e)
        {
            program.Dispose();
            texture.Dispose();
            buffer.Dispose();

            TrippyLib.Quit();
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, this.Width, this.Height);
            Matrix4 mat = Matrix4.CreateOrthographicOffCenter(0, 1, 1, 0, 0, 1);
            program.Uniforms["Projection"].SetValueMat4(ref mat);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct WeirdAssVertex : IVertex
    {
        public byte x;
        public ushort y;
        public byte z;

        public ushort x2;
        public byte y2;
        public byte z2;

        public short w;

        public byte cx;
        public ushort cy;

        // "x", "y", "z", "x2", "y2", "z2", "w", "cx", "cy"

        public WeirdAssVertex(Vector3 pos, Vector2 tc)
        {
            this.x = tobyte(pos.X - 0.1f);
            this.y = toushort(pos.Y - 0.1f);
            this.z = tobyte(0);

            this.x2 = toushort(0.1f);
            this.y2 = tobyte(0.1f);
            this.z2 = tobyte(0);

            this.w = 1;

            this.cx = tobyte(tc.X);
            this.cy = toushort(tc.Y);
        }

        static byte tobyte(float v)
        {
            return (byte)(v * byte.MaxValue);
        }

        static ushort toushort(float v)
        {
            return (ushort)(v * ushort.MaxValue);
        }

        static int toint(float v)
        {
            return (int)(v * int.MaxValue);
        }
        
        public VertexAttribDescription[] AttribDescriptions
        {
            get
            {
                return new VertexAttribDescription[]
                {
                    new VertexAttribDescription(ActiveAttribType.Float, true, VertexAttribPointerType.UnsignedByte), //x
                    new VertexAttribDescription(ActiveAttribType.Float, true, VertexAttribPointerType.UnsignedShort), //y
                    new VertexAttribDescription(ActiveAttribType.Float, true, VertexAttribPointerType.UnsignedByte), //z

                    new VertexAttribDescription(ActiveAttribType.Float, true, VertexAttribPointerType.UnsignedShort), //x2
                    new VertexAttribDescription(ActiveAttribType.Float, true, VertexAttribPointerType.UnsignedByte), //y2
                    new VertexAttribDescription(ActiveAttribType.Float, true, VertexAttribPointerType.UnsignedByte), //z2

                    new VertexAttribDescription(ActiveAttribType.Float, false, VertexAttribPointerType.Short), //w
                    
                    new VertexAttribDescription(ActiveAttribType.Float, true, VertexAttribPointerType.UnsignedByte), //cx
                    new VertexAttribDescription(ActiveAttribType.Float, true, VertexAttribPointerType.UnsignedShort)  //cy
                };
            }
        }
    }
}
