using System;
using System.IO;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using TrippyGL;
using System.Runtime.InteropServices;

namespace TrippyTesting.Tests
{
    class StructPaddingTest : GameWindow
    {
        System.Diagnostics.Stopwatch stopwatch;
        float time;
        Random r = new Random();

        ShaderProgram program;
        VertexBuffer<WeirdAssVertex> buffer;
        Texture2D texture;

        GraphicsDevice graphicsDevice;

        public StructPaddingTest() : base(1280, 720, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 0, 0, 8, ColorFormat.Empty, 2), "haha yes", GameWindowFlags.Default, DisplayDevice.Default, 4, 0, GraphicsContextFlags.Default)
        {
            VSync = VSyncMode.On;
            graphicsDevice = new GraphicsDevice(Context);
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

            texture = new Texture2D(graphicsDevice, "data4/jeru.png");

            program = new ShaderProgram(graphicsDevice);
            program.AddVertexShader(File.ReadAllText("spad/vs.glsl"));
            program.AddFragmentShader(File.ReadAllText("spad/fs.glsl"));
            program.SpecifyVertexAttribs<WeirdAssVertex>(new string[]
            {
                "x", "y", "z", "x2", "y2", "z2", "mat", "w", "cx", "cy"
            });
            program.LinkProgram();
            Matrix4 id = Matrix4.Identity;
            program.Uniforms["World"].SetValueMat4(ref id);
            program.Uniforms["View"].SetValueMat4(ref id);
            program.Uniforms["Projection"].SetValueMat4(ref id);
            program.Uniforms["samp"].SetValueTexture(texture);

            WeirdAssVertex[] data = new WeirdAssVertex[]
            {
                new WeirdAssVertex(new Vector3(0.2f, 0.2f, 0), new Vector2(0, 0)),
                new WeirdAssVertex(new Vector3(0.8f, 0.2f, 0), new Vector2(1, 0)),
                new WeirdAssVertex(new Vector3(0.2f, 0.8f, 0), new Vector2(0, 1)),
                new WeirdAssVertex(new Vector3(0.8f, 0.8f, 0), new Vector2(1, 1)),
            };
            buffer = new VertexBuffer<WeirdAssVertex>(graphicsDevice, data, BufferUsageHint.StaticDraw);
            
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
            graphicsDevice.ClearColor = new Color4(0f, 0f, 0f, 1f);
            graphicsDevice.BlendingEnabled = false;
            graphicsDevice.DepthTestingEnabled = false;

            GL.Clear(ClearBufferMask.ColorBufferBit);
            graphicsDevice.BindVertexArray(buffer.VertexArray);
            program.EnsurePreDrawStates();
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            graphicsDevice.Viewport = new Rectangle(0, 0, this.Width, this.Height);
            Matrix4 mat = Matrix4.CreateOrthographicOffCenter(0, 1, 1, 0, 0, 1);
            program.Uniforms["Projection"].SetValueMat4(ref mat);
        }

        protected override void OnUnload(EventArgs e)
        {
            program.Dispose();
            texture.Dispose();
            buffer.Dispose();

            graphicsDevice.Dispose();
        }


        [StructLayout(LayoutKind.Sequential)]
        private struct WeirdAssVertex : IVertex
        {
            public byte x;
            public ushort y;
            public byte z;

            public ushort x2;
            public byte y2;
            public byte z2;

            public Matrix4 mat4;

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

                this.mat4 = Matrix4.Identity;

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
                    
                    new VertexAttribDescription(ActiveAttribType.FloatMat4), //mat4

                    new VertexAttribDescription(ActiveAttribType.Float, false, VertexAttribPointerType.Short), //w
                    
                    new VertexAttribDescription(ActiveAttribType.Float, true, VertexAttribPointerType.UnsignedByte), //cx
                    new VertexAttribDescription(ActiveAttribType.Float, true, VertexAttribPointerType.UnsignedShort)  //cy
                    };
                }
            }
        }
    }
}
