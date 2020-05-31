using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Common;
using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using TrippyGL;

namespace TrippyTesting.Tests
{
    class StructPaddingTest
    {
        System.Diagnostics.Stopwatch stopwatch;
        Random r = new Random();

        IWindow window;

        GraphicsDevice graphicsDevice;

        ShaderProgram program;
        VertexBuffer<WeirdAssVertex> buffer;
        Texture2D texture;

        public StructPaddingTest()
        {
            window = CreateWindow();

            window.Load += OnWindowLoad;
            window.Update += OnWindowUpdate;
            window.Render += OnWindowRender;
            window.Resize += OnWindowResized;
            window.Closing += OnWindowClosing;
        }

        private IWindow CreateWindow()
        {
            GraphicsAPI graphicsApi = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.Debug, new APIVersion(3, 3));
            VideoMode videoMode = new VideoMode(new System.Drawing.Size(1280, 720));
            ViewOptions viewOpts = new ViewOptions(true, 60.0, 60.0, graphicsApi, VSyncMode.Adaptive, 30, false, videoMode, 0);
            return Window.Create(new WindowOptions(viewOpts));
        }

        public void Run()
        {
            window.Run();
        }

        private void OnWindowLoad()
        {
            graphicsDevice = new GraphicsDevice(GL.GetApi(window));
            graphicsDevice.DebugMessagingEnabled = true;
            graphicsDevice.DebugMessage += Program.OnDebugMessage;

            Console.WriteLine(string.Concat("GL Version: ", graphicsDevice.GLMajorVersion, ".", graphicsDevice.GLMinorVersion));
            Console.WriteLine("GL Version String: " + graphicsDevice.GLVersion);
            Console.WriteLine("GL Vendor: " + graphicsDevice.GLVendor);
            Console.WriteLine("GL Renderer: " + graphicsDevice.GLRenderer);
            Console.WriteLine("GL ShadingLanguageVersion: " + graphicsDevice.GLShadingLanguageVersion);
            Console.WriteLine("GL TextureUnits: " + graphicsDevice.MaxTextureImageUnits);
            Console.WriteLine("GL MaxTextureSize: " + graphicsDevice.MaxTextureSize);
            Console.WriteLine("GL MaxSamples: " + graphicsDevice.MaxSamples);

            stopwatch = System.Diagnostics.Stopwatch.StartNew();

            texture = Texture2DExtensions.FromFile(graphicsDevice, "data4/jeru.png");

            ShaderProgramBuilder programBuilder = new ShaderProgramBuilder();
            programBuilder.VertexShaderCode = File.ReadAllText("spad/vs.glsl");
            programBuilder.FragmentShaderCode = File.ReadAllText("spad/fs.glsl");
            programBuilder.SpecifyVertexAttribs<WeirdAssVertex>(new string[] { "x", "y", "z", "x2", "y2", "z2", "mat", "w", "cx", "cy" });
            program = programBuilder.Create(graphicsDevice, true);
            Matrix4x4 id = Matrix4x4.Identity;
            program.Uniforms["World"].SetValueMat4(id);
            program.Uniforms["View"].SetValueMat4(id);
            program.Uniforms["Projection"].SetValueMat4(id);
            program.Uniforms["samp"].SetValueTexture(texture);

            WeirdAssVertex[] data = new WeirdAssVertex[]
            {
                new WeirdAssVertex(new Vector3(0.2f, 0.2f, 0), new Vector2(0, 0)),
                new WeirdAssVertex(new Vector3(0.8f, 0.2f, 0), new Vector2(1, 0)),
                new WeirdAssVertex(new Vector3(0.2f, 0.8f, 0), new Vector2(0, 1)),
                new WeirdAssVertex(new Vector3(0.8f, 0.8f, 0), new Vector2(1, 1)),
            };
            buffer = new VertexBuffer<WeirdAssVertex>(graphicsDevice, data, BufferUsageARB.StaticDraw);

            OnWindowResized(window.Size);
        }

        private void OnWindowUpdate(double dtSeconds)
        {
            GLEnum c;
            while ((c = graphicsDevice.GL.GetError()) != GLEnum.NoError)
            {
                Console.WriteLine("Error found: " + c);
            }
        }

        private void OnWindowRender(double dtSeconds)
        {
            if (window.IsClosing)
                return;

            graphicsDevice.ClearColor = new Vector4(0f, 0f, 0f, 1f);
            graphicsDevice.BlendingEnabled = false;
            graphicsDevice.DepthTestingEnabled = false;

            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit);
            graphicsDevice.VertexArray = buffer.VertexArray;
            graphicsDevice.ShaderProgram = program;
            graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

            window.SwapBuffers();
        }

        private void OnWindowResized(System.Drawing.Size size)
        {
            if (size.Width == 0 || size.Height == 0)
                return;

            graphicsDevice.SetViewport(0, 0, (uint)size.Width, (uint)size.Height);
            Matrix4x4 mat = Matrix4x4.CreateOrthographicOffCenter(0, 1, 1, 0, 0, 1);
            program.Uniforms["Projection"].SetValueMat4(mat);
        }

        private void OnWindowClosing()
        {
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

            public Matrix4x4 mat4;
            private Vector4 LMAOvec4;

            public short w;
            private byte LMAObyte1;

            public byte cx;
            private Vector3 LMAOvec3;
            public ushort cy;

            private Matrix4x4 mainkra1;
            private Matrix4x4 mainkra2;
            private Matrix4x4 mainkra3;
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

                mat4 = Matrix4x4.Identity;

                w = 1;

                cx = tobyte(tc.X);
                cy = toushort(tc.Y);

                LMAOvec4 = new Vector4();
                LMAObyte1 = 0;
                LMAOvec3 = new Vector3();
                LMAObyte0 = 0;
                mainkra1 = new Matrix4x4();
                mainkra2 = new Matrix4x4();
                mainkra3 = new Matrix4x4();
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
                descriptions[0] = new VertexAttribDescription(AttributeType.Float, true, VertexAttribPointerType.UnsignedByte); //x
                descriptions[1] = new VertexAttribDescription(AttributeType.Float, true, VertexAttribPointerType.UnsignedShort); //y
                descriptions[2] = new VertexAttribDescription(AttributeType.Float, true, VertexAttribPointerType.UnsignedByte); //z

                descriptions[3] = new VertexAttribDescription(AttributeType.Float, true, VertexAttribPointerType.UnsignedShort); //x2
                descriptions[4] = new VertexAttribDescription(AttributeType.Float, true, VertexAttribPointerType.UnsignedByte); //y2
                descriptions[5] = new VertexAttribDescription(1); //LMAObyte0 padding
                descriptions[6] = new VertexAttribDescription(AttributeType.Float, true, VertexAttribPointerType.UnsignedByte); //z2

                descriptions[7] = new VertexAttribDescription(AttributeType.FloatMat4); //mat4
                descriptions[8] = VertexAttribDescription.CreatePadding(AttributeType.FloatVec4); //LMAOvec4 padding

                descriptions[9] = new VertexAttribDescription(AttributeType.Float, false, VertexAttribPointerType.Short); //w
                descriptions[10] = new VertexAttribDescription(1); //LMAObyte1 padding

                descriptions[11] = new VertexAttribDescription(AttributeType.Float, true, VertexAttribPointerType.UnsignedByte); //cx
                descriptions[12] = VertexAttribDescription.CreatePadding(AttributeType.FloatVec3); //LMAOvec3 padding
                descriptions[13] = new VertexAttribDescription(AttributeType.Float, true, VertexAttribPointerType.UnsignedShort);  //cy

                descriptions[14] = new VertexAttribDescription(193); //mainkra1-4 padding (3 matrices and a single byte
            }
        }
    }
}
