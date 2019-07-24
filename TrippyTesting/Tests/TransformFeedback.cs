using System;
using System.IO;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using TrippyGL;

namespace TrippyTesting.Tests
{
    class TransformFeedback : GameWindow
    {
        System.Diagnostics.Stopwatch stopwatch;
        float time;
        static Random r = new Random();

        ShaderProgram program;
        VertexBuffer<VertexNormal> bufferRead, bufferWrite;

        int tbo;

        GraphicsDevice graphicsDevice;

        public TransformFeedback() : base(1280, 720, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 0, 0, 0, ColorFormat.Empty, 2), "haha yes", GameWindowFlags.Default, DisplayDevice.Default, 4, 0, GraphicsContextFlags.Default)
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

            program = new ShaderProgram(graphicsDevice);
            program.AddVertexShader(File.ReadAllText("tfeedback/vs.glsl"));
            program.AddFragmentShader(File.ReadAllText("tfeedback/fs.glsl"));
            program.SpecifyVertexAttribs<VertexNormal>(new string[] { "vPosition", "vNormal" });
            GL.TransformFeedbackVaryings(program.Handle, 2, new string[] { "tPosition", "tNormal" }, TransformFeedbackMode.InterleavedAttribs);
            program.LinkProgram();

            VertexNormal[] vertices = new VertexNormal[]
            {
                new VertexNormal(new Vector3(-0.5f, -0.5f, 0), new Vector3(0.1f, 0.4f, 0.7f)),
                new VertexNormal(new Vector3(0, 0.5f, 0), new Vector3(0.8f, 0.2f, 0.5f)),
                new VertexNormal(new Vector3(0.5f, -0.5f, 0), new Vector3(0.6f, 0.9f, 0.3f)),
            };

            bufferRead = new VertexBuffer<VertexNormal>(graphicsDevice, vertices, BufferUsageHint.DynamicDraw);
            bufferWrite = new VertexBuffer<VertexNormal>(graphicsDevice, new VertexNormal[vertices.Length], BufferUsageHint.DynamicDraw);

            tbo = GL.GenTransformFeedback();
            GL.BindTransformFeedback(TransformFeedbackTarget.TransformFeedback, tbo);
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
            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit);

            graphicsDevice.ShaderProgram = program;
            GL.BindTransformFeedback(TransformFeedbackTarget.TransformFeedback, tbo);
            GL.BindBufferBase(BufferRangeTarget.TransformFeedbackBuffer, 0, bufferWrite.Buffer.Handle);
            GL.BindBufferBase(BufferRangeTarget.TransformFeedbackBuffer, 1, bufferWrite.Buffer.Handle);
            GL.BeginTransformFeedback(TransformFeedbackPrimitiveType.Triangles);

            graphicsDevice.VertexArray = bufferRead.VertexArray;
            graphicsDevice.DrawArrays(PrimitiveType.Triangles, 0, bufferRead.StorageLength);

            GL.EndTransformFeedback();

            VertexNormal[] data = new VertexNormal[bufferWrite.StorageLength];
            bufferWrite.GetVertexData(data);

            VertexBuffer<VertexNormal> tmp = bufferRead;
            bufferRead = bufferWrite;
            bufferWrite = tmp;

            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            graphicsDevice.Viewport = new Rectangle(0, 0, this.Width, this.Height);
        }

        protected override void OnUnload(EventArgs e)
        {


            graphicsDevice.Dispose();
        }
    }
}
