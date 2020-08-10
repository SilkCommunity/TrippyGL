using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Common;
using System;
using System.IO;
using System.Numerics;
using TrippyGL;

namespace TrippyTesting.Tests
{
    class ObjDragon
    {
        System.Diagnostics.Stopwatch stopwatch;
        readonly IWindow window;

        GraphicsDevice graphicsDevice;

        VertexBuffer<VertexNormal> vertexBuffer;

        ShaderProgram shaderProgram;


        public ObjDragon()
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
            ViewOptions viewOpts = new ViewOptions(true, 60.0, 60.0, graphicsApi, VSyncMode.On, 30, false, videoMode, 24);
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

            VertexNormal[] dragon = OBJLoader.FromFile<VertexNormal>("objs/dragon.obj");
            vertexBuffer = new VertexBuffer<VertexNormal>(graphicsDevice, dragon, BufferUsageARB.StaticDraw);

            ShaderProgramBuilder programBuilder = new ShaderProgramBuilder();
            programBuilder.VertexShaderCode = File.ReadAllText("objs/dragon_vs.glsl");
            programBuilder.FragmentShaderCode = File.ReadAllText("objs/dragon_fs.glsl");
            programBuilder.SpecifyVertexAttribs<VertexNormal>(new string[] { "vPosition", "vNormal" });
            shaderProgram = programBuilder.Create(graphicsDevice, true);
            Console.WriteLine("VS Log: " + programBuilder.VertexShaderLog);
            Console.WriteLine("FS Log: " + programBuilder.FragmentShaderLog);
            Console.WriteLine("Program Log: " + programBuilder.ProgramLog);

            //(inverse(View) * vec4(0.0, 0.0, 0.0, 1.0)).xyz
            Matrix4x4 view = Matrix4x4.CreateLookAt(new Vector3(0, 7, -15), new Vector3(0, 5, 0), new Vector3(0, 1, 0));
            shaderProgram.Uniforms["View"].SetValueMat4(view);
            Matrix4x4.Invert(view, out Matrix4x4 invertedView);
            Vector4 camPos = Vector4.Transform(Vector4.UnitW, invertedView);
            shaderProgram.Uniforms["cameraPos"].SetValueVec3(camPos.X, camPos.Y, camPos.Z);

            //shaderProgram.Uniforms["lightPos"].SetValueVec3(4, 0, -12);
            shaderProgram.Uniforms["lightDir"].SetValueVec3(Vector3.Normalize(new Vector3(0, -1, 0)));
            shaderProgram.Uniforms["lightColor"].SetValueVec4(Color4b.White);
            shaderProgram.Uniforms["shineDamper"].SetValueFloat(8f);
            shaderProgram.Uniforms["reflectivity"].SetValueFloat(1f);

            graphicsDevice.BlendingEnabled = false;
            graphicsDevice.DepthState = DepthTestingState.Default;

            // For some reason if we don't set enabled to true last, this doesnt work???
            graphicsDevice.CullFaceMode = CullFaceMode.Back;
            graphicsDevice.PolygonFrontFace = FrontFaceDirection.Ccw;
            graphicsDevice.FaceCullingEnabled = true;

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

            graphicsDevice.ClearColor = new Vector4(0, 0, 0, 1);
            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            shaderProgram.Uniforms["World"].SetValueMat4(Matrix4x4.CreateRotationY((float)stopwatch.Elapsed.TotalSeconds));

            graphicsDevice.ShaderProgram = shaderProgram;
            graphicsDevice.VertexArray = vertexBuffer;
            graphicsDevice.DrawArrays(PrimitiveType.Triangles, 0, vertexBuffer.StorageLength);

            window.SwapBuffers();
        }

        private void OnWindowResized(System.Drawing.Size size)
        {
            if (size.Width == 0 || size.Height == 0)
                return;

            graphicsDevice.SetViewport(0, 0, (uint)size.Width, (uint)size.Height);

            shaderProgram.Uniforms["Projection"].SetValueMat4(Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 2f, window.Size.Width / (float)window.Size.Height, 0.1f, 50f));
        }

        private void OnWindowClosing()
        {
            vertexBuffer.Dispose();
            shaderProgram.Dispose();
            graphicsDevice.Dispose();
        }
    }
}
