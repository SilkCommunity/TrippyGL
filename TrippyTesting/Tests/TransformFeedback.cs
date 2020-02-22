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

        BufferObject buffer1, buffer2;
        VertexArray arrayRead, arrayWrite;
        VertexDataBufferSubset<Vector3> subsetPositionRead, subsetNormalRead, subsetPositionWrite, subsetNormalWrite;

        TransformFeedbackObject TFObject;

        int tfo;

        GraphicsDevice graphicsDevice;

        public TransformFeedback() : base(1280, 720, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 0, 0, 0, ColorFormat.Empty, 2), "haha yes", GameWindowFlags.Default, DisplayDevice.Default, 4, 0, GraphicsContextFlags.Default)
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

            program = new ShaderProgram(graphicsDevice);
            program.AddVertexShader(File.ReadAllText("tfeedback/vs.glsl"));
            program.AddFragmentShader(File.ReadAllText("tfeedback/fs.glsl"));
            program.SpecifyVertexAttribs<VertexNormal>(new string[] { "vPosition", "vNormal" });
            GL.TransformFeedbackVaryings(program.Handle, 2, new string[] { "tPosition", "tNormal", }, TransformFeedbackMode.SeparateAttribs);
            program.LinkProgram();

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

            buffer1 = new BufferObject(graphicsDevice, vertices.Length * VertexNormal.SizeInBytes, BufferUsageHint.DynamicDraw);
            buffer2 = new BufferObject(graphicsDevice, vertices.Length * VertexNormal.SizeInBytes, BufferUsageHint.DynamicDraw);
            subsetPositionRead = new VertexDataBufferSubset<Vector3>(buffer1, 0, positions.Length, positions);
            subsetNormalRead = new VertexDataBufferSubset<Vector3>(buffer1, subsetPositionRead.StorageNextInBytes, normals.Length, positions);
            subsetPositionWrite = new VertexDataBufferSubset<Vector3>(buffer2, 0, normals.Length, new Vector3[positions.Length]);
            subsetNormalWrite = new VertexDataBufferSubset<Vector3>(buffer2, subsetPositionWrite.StorageNextInBytes, normals.Length, new Vector3[normals.Length]);
            arrayRead = new VertexArray(graphicsDevice, new VertexAttribSource[]
            {
                new VertexAttribSource(subsetPositionRead, ActiveAttribType.FloatVec3),
                new VertexAttribSource(subsetNormalRead, ActiveAttribType.FloatVec3)
            });
            arrayWrite = new VertexArray(graphicsDevice, new VertexAttribSource[]
            {
                new VertexAttribSource(subsetPositionWrite, ActiveAttribType.FloatVec3),
                new VertexAttribSource(subsetNormalWrite, ActiveAttribType.FloatVec3)
            });

            tfo = GL.GenTransformFeedback();
            GL.BindTransformFeedback(TransformFeedbackTarget.TransformFeedback, tfo);

            TFObject = new TransformFeedbackObject(graphicsDevice, new TransformFeedbackVariableDescription[]
            {
                new TransformFeedbackVariableDescription(subsetPositionWrite, TransformFeedbackType.FloatVec3),
                new TransformFeedbackVariableDescription(subsetNormalWrite, TransformFeedbackType.FloatVec3)
            });
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
            GL.BindTransformFeedback(TransformFeedbackTarget.TransformFeedback, tfo);
            GL.BindBufferRange(BufferRangeTarget.TransformFeedbackBuffer, 0, subsetPositionWrite.BufferHandle, (IntPtr)subsetPositionWrite.StorageOffsetInBytes, subsetPositionWrite.StorageLengthInBytes);
            GL.BindBufferRange(BufferRangeTarget.TransformFeedbackBuffer, 1, subsetNormalWrite.BufferHandle, (IntPtr)subsetNormalWrite.StorageOffsetInBytes, subsetNormalWrite.StorageOffsetInBytes);

            GL.BeginTransformFeedback(TransformFeedbackPrimitiveType.Triangles);

            graphicsDevice.VertexArray = arrayRead;
            graphicsDevice.DrawArrays(PrimitiveType.Triangles, 0, subsetPositionRead.StorageLength);

            GL.EndTransformFeedback();

            VertexArray tmpvao = arrayRead;
            arrayRead = arrayWrite;
            arrayWrite = tmpvao;

            VertexDataBufferSubset<Vector3> tmpsub = subsetPositionRead;
            subsetPositionRead = subsetPositionWrite;
            subsetPositionWrite = tmpsub;

            tmpsub = subsetNormalRead;
            subsetNormalRead = subsetNormalWrite;
            subsetNormalWrite = tmpsub;

            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            graphicsDevice.Viewport = new Rectangle(0, 0, Width, Height);
        }

        protected override void OnUnload(EventArgs e)
        {


            graphicsDevice.Dispose();
        }
    }
}
