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
    // This test makes a 7 segment display using indexed rendering.
    // The number displayed can be changed by pressing a number on the keyboard.

    // All the vertices for the display are stored in a buffer and indices are used to define
    // the order in which they are taken. By using different indices, we can draw the different numbers.

    class IndexedRendering
    {
        IWindow window;
        IInputContext inputContext;

        GraphicsDevice graphicsDevice;

        VertexBuffer<SimpleVertex> vertexBuffer;
        ShaderProgram shaderProgram;

        /// <summary>The location in the index buffer where the index data for each number starts.</summary>
        int[] indicesStart;
        /// <summary>The number the 7 segment display is currently rendering.</summary>
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
            VideoMode videoMode = new VideoMode(new System.Drawing.Size(400, 60), 60);
            ViewOptions viewOpts = new ViewOptions(true, 60.0, 60.0, graphicsApi, VSyncMode.Adaptive, 30, false, videoMode, 0);
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

            // We create a VertexBuffer with just enough vertex storage for all the vertices
            // and enough index storage for all the indices, and give it vertexData as initial vertex data.
            vertexBuffer = new VertexBuffer<SimpleVertex>(graphicsDevice, (uint)vertexData.Length, (uint)Indices.TotalIndicesLength, DrawElementsType.UnsignedByte, BufferUsageARB.StaticDraw, vertexData);

            // We will store the location in the index subset where each number's indices start in this array
            indicesStart = new int[Indices.AllNumbersIndices.Length];

            // We will copy all the data from all the number's indices over to the index buffer subset.
            // We copy them in order (that is, Number0 followed by Number1 followed by Number2 etc)
            // and in indicesStart we store in which location of the subset the indices of each number start.
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
            Console.WriteLine("VS Log: " + programBuilder.VertexShaderLog);
            Console.WriteLine("FS Log: " + programBuilder.FragmentShaderLog);
            Console.WriteLine("Program Log: " + programBuilder.ProgramLog);

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
