using System;
using System.IO;
using OpenTK;
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

            Console.WriteLine(string.Concat("GL Version: ", graphicsDevice.GLMajorVersion, ".", graphicsDevice.GLMinorVersion));
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

            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit);
            graphicsDevice.VertexArray = buffer.VertexArray;
            graphicsDevice.ShaderProgram = program;
            graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            graphicsDevice.Viewport = new Rectangle(0, 0, Width, Height);
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
            private byte LMAObyte0;
            public byte z2;

            public Matrix4 mat4;
            private Vector4 LMAOvec4;

            public short w;
            private byte LMAObyte1;

            public byte cx;
            private Vector3 LMAOvec3;
            public ushort cy;

            private Matrix4 mainkra1;
            private Matrix4 mainkra2;
            private Matrix4 mainkra3;
            private byte mainkra4;

            // "x", "y", "z", "x2", "y2", "z2", "w", "cx", "cy"

            public WeirdAssVertex(Vector3 pos, Vector2 tc)
            {
                x = tobyte(pos.X - 0.1f);
                y = toushort(pos.Y - 0.1f);
                z = tobyte(0);

                x2 = toushort(0.1f);
                y2 = tobyte(0.1f);
                z2 = tobyte(0);

                mat4 = Matrix4.Identity;

                w = 1;

                cx = tobyte(tc.X);
                cy = toushort(tc.Y);

                LMAOvec4 = new Vector4();
                LMAObyte1 = 0;
                LMAOvec3 = new Vector3();
                LMAObyte0 = 0;
                mainkra1 = new Matrix4();
                mainkra2 = new Matrix4();
                mainkra3 = new Matrix4();
                mainkra4 = 0;
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

            public int AttribDescriptionCount => 15;

            public void WriteAttribDescriptions(Span<VertexAttribDescription> descriptions)
            {
                descriptions[0] = new VertexAttribDescription(ActiveAttribType.Float, true, VertexAttribPointerType.UnsignedByte); //x
                descriptions[1] = new VertexAttribDescription(ActiveAttribType.Float, true, VertexAttribPointerType.UnsignedShort); //y
                descriptions[2] = new VertexAttribDescription(ActiveAttribType.Float, true, VertexAttribPointerType.UnsignedByte); //z

                descriptions[3] = new VertexAttribDescription(ActiveAttribType.Float, true, VertexAttribPointerType.UnsignedShort); //x2
                descriptions[4] = new VertexAttribDescription(ActiveAttribType.Float, true, VertexAttribPointerType.UnsignedByte); //y2
                descriptions[5] = new VertexAttribDescription(1); //LMAObyte0 padding
                descriptions[6] = new VertexAttribDescription(ActiveAttribType.Float, true, VertexAttribPointerType.UnsignedByte); //z2

                descriptions[7] = new VertexAttribDescription(ActiveAttribType.FloatMat4); //mat4
                descriptions[8] = VertexAttribDescription.CreatePadding(ActiveAttribType.FloatVec4); //LMAOvec4 padding

                descriptions[9] = new VertexAttribDescription(ActiveAttribType.Float, false, VertexAttribPointerType.Short); //w
                descriptions[10] = new VertexAttribDescription(1); //LMAObyte1 padding

                descriptions[11] = new VertexAttribDescription(ActiveAttribType.Float, true, VertexAttribPointerType.UnsignedByte); //cx
                descriptions[12] = VertexAttribDescription.CreatePadding(ActiveAttribType.FloatVec3); //LMAOvec3 padding
                descriptions[13] = new VertexAttribDescription(ActiveAttribType.Float, true, VertexAttribPointerType.UnsignedShort);  //cy

                descriptions[14] = new VertexAttribDescription(193); //mainkra1-4 padding (3 matrices and a single byte
            }
        }
    }
}
