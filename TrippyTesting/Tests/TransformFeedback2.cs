using System;
using System.IO;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using TrippyGL;

namespace TrippyTesting.Tests
{
    class TransformFeedback2 : GameWindow
    {
        System.Diagnostics.Stopwatch stopwatch;
        float time;
        static Random r = new Random();

        ShaderProgram program;

        TransformFeedbackObject tfoRead, tfoWrite;

        BufferObject bufferRead, bufferWrite;
        VertexDataBufferSubset<VertexNormal> subsetRead, subsetWrite;
        VertexArray arrayRead, arrayWrite;

        GraphicsDevice graphicsDevice;

        public TransformFeedback2() : base(1280, 720, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 0, 0, 0, ColorFormat.Empty, 2), "haha yes", GameWindowFlags.Default, DisplayDevice.Default, 4, 0, GraphicsContextFlags.Default)
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

            VertexNormal[] vertices = new VertexNormal[]
            {
                new VertexNormal(new Vector3(-0.5f, -0.5f, 0), new Vector3(0.1f, 0.4f, 0.7f)),
                new VertexNormal(new Vector3(0, 0.5f, 0), new Vector3(0.8f, 0.2f, 0.5f)),
                new VertexNormal(new Vector3(0.5f, -0.5f, 0), new Vector3(0.6f, 0.9f, 0.3f)),
            };

            Vector3[] positions = new Vector3[vertices.Length];
            Vector3[] normals = new Vector3[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                positions[i] = vertices[i].Position;
                normals[i] = vertices[i].Normal;
            }

            bufferRead = new BufferObject(graphicsDevice, vertices.Length * VertexNormal.SizeInBytes, BufferUsageHint.DynamicDraw);
            bufferWrite = new BufferObject(graphicsDevice, vertices.Length * VertexNormal.SizeInBytes, BufferUsageHint.DynamicDraw);
            subsetRead = new VertexDataBufferSubset<VertexNormal>(bufferRead, vertices);
            subsetWrite = new VertexDataBufferSubset<VertexNormal>(bufferWrite);

            arrayRead = new VertexArray(graphicsDevice, new VertexAttribSource[]
            {
                new VertexAttribSource(subsetRead, ActiveAttribType.FloatVec3),
                new VertexAttribSource(subsetRead, ActiveAttribType.FloatVec3)
            });
            arrayWrite = new VertexArray(graphicsDevice, new VertexAttribSource[]
            {
                new VertexAttribSource(subsetWrite, ActiveAttribType.FloatVec3),
                new VertexAttribSource(subsetWrite, ActiveAttribType.FloatVec3)
            });

            tfoRead = new TransformFeedbackObject(graphicsDevice, new TransformFeedbackVariableDescription[]
            {
                new TransformFeedbackVariableDescription(subsetWrite, TransformFeedbackType.FloatVec3),
                new TransformFeedbackVariableDescription(subsetWrite, TransformFeedbackType.FloatVec3)
            }, TransformFeedbackPrimitiveType.Triangles);
            tfoWrite = new TransformFeedbackObject(graphicsDevice, new TransformFeedbackVariableDescription[]
            {
                new TransformFeedbackVariableDescription(subsetRead, TransformFeedbackType.FloatVec3),
                new TransformFeedbackVariableDescription(subsetRead, TransformFeedbackType.FloatVec3)
            }, TransformFeedbackPrimitiveType.Triangles);

            program = new ShaderProgram(graphicsDevice);
            program.AddVertexShader(File.ReadAllText("tfeedback/vs.glsl"));
            program.AddFragmentShader(File.ReadAllText("tfeedback/fs.glsl"));
            program.SpecifyVertexAttribs<VertexNormal>(new string[] { "vPosition", "vNormal" });
            //program TRANSFORM FEEDBACK
            program.LinkProgram();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {

        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            graphicsDevice.ClearColor = Color4.Black;
            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            graphicsDevice.ShaderProgram = program;
            graphicsDevice.VertexArray = arrayRead;
            graphicsDevice.DrawArrays(PrimitiveType.Triangles, 0, subsetRead.StorageLength);

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
