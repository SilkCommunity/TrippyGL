using Silk.NET.Input;
using Silk.NET.Input.Common;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Common;
using System;
using System.IO;
using System.Numerics;
using TrippyGL;

namespace IndexedRendering
{
    class IndexedRendering
    {
        IWindow window;
        IInputContext inputContext;

        GraphicsDevice graphicsDevice;

        VertexBuffer<SimpleVertex> vertexBuffer;
        ShaderProgram shaderProgram;

        int[] indicesStart;
        int currentSelectedIndex = 0;

        public IndexedRendering()
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
            ViewOptions viewOpts = new ViewOptions(true, 60.0, 60.0, graphicsApi, VSyncMode.Adaptive, 30, false, videoMode, 8);
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

            SimpleVertex[] vertexData = Indices.Vertices;

            vertexBuffer = new VertexBuffer<SimpleVertex>(graphicsDevice, (uint)vertexData.Length, (uint)Indices.TotalIndicesLength, DrawElementsType.UnsignedByte, BufferUsageARB.StaticDraw, vertexData);

            indicesStart = new int[Indices.AllNumbersIndices.Length];
            int indexStart = 0;
            for (int i = 0; i < Indices.AllNumbersIndices.Length; i++)
            {
                indicesStart[i] = indexStart;
                vertexBuffer.IndexSubset.SetData(Indices.AllNumbersIndices[i], (uint)indexStart);
                indexStart += Indices.AllNumbersIndices[i].Length;
            }

            ShaderProgramBuilder programBuilder = new ShaderProgramBuilder();
            programBuilder.VertexShaderCode = File.ReadAllText("vs.glsl");
            programBuilder.FragmentShaderCode = File.ReadAllText("fs.glsl");
            programBuilder.SpecifyVertexAttribs<SimpleVertex>(new string[] { "vPosition" });
            shaderProgram = programBuilder.Create(graphicsDevice, true);
            shaderProgram.Uniforms["Projection"].SetValueMat4(Matrix4x4.Identity);
            shaderProgram.Uniforms["color"].SetValueVec4(1f, 0f, 0f, 1f);

            graphicsDevice.BlendingEnabled = false;
            graphicsDevice.DepthTestingEnabled = false;

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
            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit);

            graphicsDevice.VertexArray = vertexBuffer;
            graphicsDevice.ShaderProgram = shaderProgram;
            graphicsDevice.DrawElements(PrimitiveType.Triangles, indicesStart[currentSelectedIndex], (uint)Indices.AllNumbersIndices[currentSelectedIndex].Length);

            window.SwapBuffers();
        }

        private void OnMouseMove(IMouse sender, System.Drawing.PointF position)
        {

        }

        private void OnMouseUp(IMouse sender, MouseButton btn)
        {

        }

        private void OnMouseDown(IMouse sender, MouseButton btn)
        {

        }

        private void OnKeyDown(IKeyboard sender, Key key, int idk)
        {
            if (key >= Key.Number0 && key <= Key.Number9)
                currentSelectedIndex = key - Key.Number0;
            else if (key >= Key.Keypad0 && key <= Key.Keypad9)
                currentSelectedIndex = key - Key.Keypad0;
        }

        private void OnWindowResized(System.Drawing.Size size)
        {
            if (size.Width == 0 || size.Height == 0)
                return;

            graphicsDevice.SetViewport(0, 0, (uint)size.Width, (uint)size.Height);
            shaderProgram.Uniforms["Projection"].SetValueMat4(Matrix4x4.CreateOrthographic(window.Size.Width / (float)window.Size.Height * 2f, 2f, 0.1f, 10f));
        }

        private void OnWindowClosing()
        {
            graphicsDevice.Dispose();
        }
    }
}
