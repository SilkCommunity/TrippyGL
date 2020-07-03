using Silk.NET.Input;
using Silk.NET.Input.Common;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Common;
using System;
using System.IO;
using System.Numerics;
using TrippyGL;

namespace SimpleCubemap
{
    // Loads the images in the cubemap folder into a TextureCubemap and displays it as a skybox.
    // The camera can be moved to look around by moving the mouse while holding the left button.
    // (the images are somewhat low-res)

    class SimpleCubemap
    {
        IWindow window;
        IInputContext inputContext;

        GraphicsDevice graphicsDevice;

        TextureCubemap cubemap;
        ShaderProgram shaderProgram;
        VertexBuffer<VertexPosition> vertexBuffer;

        Vector2 cameraRot;
        System.Drawing.PointF lastMousePos;

        public SimpleCubemap()
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
            ViewOptions viewOpts = new ViewOptions(true, 60.0, 60.0, graphicsApi, VSyncMode.On, 30, false, videoMode, 0);
            return Window.Create(new WindowOptions(viewOpts));
        }

        public void Run()
        {
            window.Run();
        }

        private void OnWindowLoad()
        {
            inputContext = window.CreateInput();
            inputContext.Keyboards[0].KeyDown += OnKeyDown;
            inputContext.Mice[0].MouseDown += OnMouseDown;
            inputContext.Mice[0].MouseUp += OnMouseUp;
            inputContext.Mice[0].MouseMove += OnMouseMove;

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

            cubemap = TextureCubemapExtensions.FromFiles(
                graphicsDevice,
                "cubemap/back.png", "cubemap/front.png",
                "cubemap/bottom.png", "cubemap/top.png",
                "cubemap/left.png", "cubemap/right.png"
            );
            cubemap.SetTextureFilters(TextureMinFilter.Linear, TextureMagFilter.Linear);

            ShaderProgramBuilder programBuilder = new ShaderProgramBuilder();
            programBuilder.VertexShaderCode = File.ReadAllText("vs.glsl");
            programBuilder.FragmentShaderCode = File.ReadAllText("fs.glsl");
            programBuilder.SpecifyVertexAttribs<VertexPosition>(new string[] { "vPosition" });
            shaderProgram = programBuilder.Create(graphicsDevice, true);
            Console.WriteLine("VS Log: " + programBuilder.VertexShaderLog);
            Console.WriteLine("FS Log: " + programBuilder.FragmentShaderLog);
            Console.WriteLine("Program Log: " + programBuilder.ProgramLog);

            shaderProgram.Uniforms["cubemap"].SetValueTexture(cubemap);
            shaderProgram.Uniforms["World"].SetValueMat4(Matrix4x4.Identity);
            shaderProgram.Uniforms["View"].SetValueMat4(Matrix4x4.Identity);

            Span<VertexPosition> vertexData = stackalloc VertexPosition[]
            {
                new Vector3(-0.5f, -0.5f, -0.5f),//4
                new Vector3(-0.5f, -0.5f, 0.5f),//3
                new Vector3(-0.5f, 0.5f, -0.5f),//7
                new Vector3(-0.5f, 0.5f, 0.5f),//8
                new Vector3(0.5f, 0.5f, 0.5f),//5
                new Vector3(-0.5f, -0.5f, 0.5f),//3
                new Vector3(0.5f, -0.5f, 0.5f),//1
                new Vector3(-0.5f, -0.5f, -0.5f),//4
                new Vector3(0.5f, -0.5f, -0.5f),//2
                new Vector3(-0.5f, 0.5f, -0.5f),//7
                new Vector3(0.5f, 0.5f, -0.5f),//6
                new Vector3(0.5f, 0.5f, 0.5f),//5
                new Vector3(0.5f, -0.5f, -0.5f),//2
                new Vector3(0.5f, -0.5f, 0.5f),//1
            };

            vertexBuffer = new VertexBuffer<VertexPosition>(graphicsDevice, vertexData, BufferUsageARB.StaticDraw);

            graphicsDevice.DepthTestingEnabled = false;
            graphicsDevice.BlendingEnabled = false;

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

            graphicsDevice.ShaderProgram = shaderProgram;
            graphicsDevice.VertexArray = vertexBuffer;
            graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, vertexBuffer.StorageLength);

            window.SwapBuffers();
        }

        private void OnMouseMove(IMouse sender, System.Drawing.PointF position)
        {
            if (sender.IsButtonPressed(MouseButton.Left))
            {
                const float sensitivity = 1f / 200f;
                cameraRot.Y += (position.X - lastMousePos.X) * sensitivity;
                cameraRot.X = Math.Clamp(cameraRot.X + (position.Y - lastMousePos.Y) * sensitivity, -1.57f, 1.57f);

                lastMousePos = position;

                //shaderProgram.Uniforms["View"].SetValueMat4(Matrix4x4.CreateFromYawPitchRoll(cameraRot.Y, cameraRot.X, 0f));
                shaderProgram.Uniforms["View"].SetValueMat4(Matrix4x4.CreateRotationY(cameraRot.Y) * Matrix4x4.CreateRotationX(cameraRot.X));
            }
        }

        private void OnMouseUp(IMouse sender, MouseButton btn)
        {

        }

        private void OnMouseDown(IMouse sender, MouseButton btn)
        {
            if (btn == MouseButton.Left)
                lastMousePos = sender.Position;
        }

        private void OnKeyDown(IKeyboard sender, Key key, int idk)
        {

        }

        private void OnWindowResized(System.Drawing.Size size)
        {
            if (size.Width == 0 || size.Height == 0)
                return;

            graphicsDevice.SetViewport(0, 0, (uint)size.Width, (uint)size.Height);
            shaderProgram.Uniforms["Projection"].SetValueMat4(Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 2f, window.Size.Width / (float)window.Size.Height, 0.01f, 10f));
        }

        private void OnWindowClosing()
        {
            vertexBuffer.Dispose();
            shaderProgram.Dispose();
            cubemap.Dispose();
            graphicsDevice.Dispose();
        }
    }
}
