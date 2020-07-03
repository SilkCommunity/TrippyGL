using Silk.NET.Input;
using Silk.NET.Input.Common;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Common;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Processing;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Numerics;
using TrippyGL;

namespace ShaderFractals
{
    // Renders a moving Julia fractal with purple colors all around.
    // The view can be moved around by moving the mouse while holding the left button and
    // the scale can be changed with the scroll wheel.
    // Pressing the spacebar pauses the animation and pressing the S key saves a screenshot as png.
    // You can also toggle fullscreen with F11 and reset the camera with the "Home" key

    class ShaderFractals
    {
        Stopwatch stopwatch;
        IWindow window;
        IInputContext inputContext;

        GraphicsDevice graphicsDevice;

        VertexBuffer<VertexPosition> vertexBuffer;
        ShaderProgram shaderProgram;
        ShaderUniform transformUniform;
        ShaderUniform cUniform;

        PointF lastMousePos;
        float mouseMoveScale;

        Vector2 offset;
        float scaleExponent;
        float scale;

        public ShaderFractals()
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
            VideoMode videoMode = new VideoMode(new Size(1280, 720));
            ViewOptions viewOpts = new ViewOptions(true, 60.0, 60.0, graphicsApi, VSyncMode.On, 30, false, videoMode, 8);
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
            inputContext.Mice[0].Scroll += OnMouseScroll;

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

            Span<VertexPosition> vertexData = stackalloc VertexPosition[]
            {
                new Vector3(-1f, -1f, 0),
                new Vector3(-1f, 1f, 0),
                new Vector3(1f, -1f, 0),
                new Vector3(1f, 1f, 0),
            };

            vertexBuffer = new VertexBuffer<VertexPosition>(graphicsDevice, vertexData, BufferUsageARB.StaticDraw);

            ShaderProgramBuilder programBuilder = new ShaderProgramBuilder();
            programBuilder.VertexShaderCode = File.ReadAllText("vs.glsl");
            programBuilder.FragmentShaderCode = File.ReadAllText("fs.glsl");
            programBuilder.SpecifyVertexAttribs<VertexPosition>(new string[] { "vPosition" });
            shaderProgram = programBuilder.Create(graphicsDevice, true);
            Console.WriteLine("VS Log: " + programBuilder.VertexShaderLog);
            Console.WriteLine("FS Log: " + programBuilder.FragmentShaderLog);
            Console.WriteLine("Program Log: " + programBuilder.ProgramLog);

            transformUniform = shaderProgram.Uniforms["Transform"];
            cUniform = shaderProgram.Uniforms["c"];

            graphicsDevice.DepthTestingEnabled = false;
            graphicsDevice.BlendingEnabled = false;

            stopwatch = Stopwatch.StartNew();

            OnKeyDown(null, Key.Home, 0);
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

            const float min = 0.27f, max = 0.264f, spd = 0.5f;
            float cx = min + (max - min) * ((float)Math.Sin(stopwatch.Elapsed.TotalSeconds * spd) + 1) * 0.5f;
            cUniform.SetValueVec2(new Vector2(cx, 0.0f));

            graphicsDevice.ShaderProgram = shaderProgram;
            graphicsDevice.VertexArray = vertexBuffer;
            graphicsDevice.DrawArrays(PrimitiveType.TriangleStrip, 0, vertexBuffer.StorageLength);

            window.SwapBuffers();
        }

        private void OnMouseMove(IMouse sender, PointF position)
        {
            if (sender.IsButtonPressed(MouseButton.Left))
            {
                offset.X += (lastMousePos.X - position.X) * mouseMoveScale * scale;
                offset.Y += (position.Y - lastMousePos.Y) * mouseMoveScale * scale;
                lastMousePos = position;
                UpdateTransformMatrix();
            }
        }

        private void OnMouseDown(IMouse sender, MouseButton btn)
        {
            if (btn == MouseButton.Left)
                lastMousePos = sender.Position;
        }

        private void OnMouseUp(IMouse sender, MouseButton btn)
        {

        }

        private void OnMouseScroll(IMouse sender, ScrollWheel scroll)
        {
            scaleExponent = Math.Clamp(scaleExponent + scroll.Y * 0.05f, -100f, 100f);
            scale = (float)Math.Pow(10, scaleExponent);
            UpdateTransformMatrix();
        }

        private void OnKeyDown(IKeyboard sender, Key key, int idk)
        {
            switch (key)
            {
                case Key.F11:
                    if (window.WindowState == WindowState.Fullscreen)
                    {
                        window.WindowState = WindowState.Normal;
                        Size mSize = window.Monitor.VideoMode.Resolution.Value;
                        window.Size = new Size(mSize.Width * 2 / 3, mSize.Height * 2 / 3);
                    }
                    else
                    {
                        Size size = window.Monitor.VideoMode.Resolution.Value;
                        window.WindowState = WindowState.Fullscreen;
                        window.Size = size;
                    }
                    break;

                case Key.Home:
                    offset = new Vector2(-0.0504f, 0.2522f);
                    scaleExponent = 0.4f;
                    scale = (float)Math.Pow(10, scaleExponent);
                    UpdateTransformMatrix();
                    break;

                case Key.Space:
                    if (stopwatch.IsRunning)
                        stopwatch.Stop();
                    else
                        stopwatch.Start();
                    break;

                case Key.S:
                    TakeScreenshot();
                    break;
            }
        }

        private void UpdateTransformMatrix()
        {
            Matrix4x4 mat = Matrix4x4.CreateScale(scale) * Matrix4x4.CreateTranslation(offset.X, offset.Y, 0f);
            transformUniform.SetValueMat4(mat);
            //window.Title = "offset=" + offset.ToString() + ", scale=" + scale.ToString() + ", scaleExponent=" + scaleExponent.ToString();
        }

        private unsafe void TakeScreenshot()
        {
            // We could normally use the FramebufferObject.SaveAsImage() extension from
            // TrippyGL.ImageSharp, but since we want to save the default framebuffer we
            // have to do this manually.

            using Image<SixLabors.ImageSharp.PixelFormats.Rgba32> image = new Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(window.Size.Width, window.Size.Height);

            graphicsDevice.Framebuffer = null;
            fixed (void* ptr = image.GetPixelSpan())
                graphicsDevice.GL.ReadPixels(0, 0, (uint)window.Size.Width, (uint)window.Size.Height, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
            image.Mutate(x => x.Flip(FlipMode.Vertical));

            string file = GetFileName();
            using FileStream fileStream = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Read);
            image.SaveAsPng(fileStream);

            static string GetFileName()
            {
                const string name = "screenshot";
                const string ext = ".png";

                if (!File.Exists(name + ext))
                    return name + ext;

                int i = 1;
                while (true)
                {
                    string n = name + i.ToString() + ext;
                    if (!File.Exists(n))
                        return n;
                    i++;
                }
            }
        }

        private void OnWindowResized(Size size)
        {
            if (size.Width == 0 || size.Height == 0)
                return;

            graphicsDevice.SetViewport(0, 0, (uint)size.Width, (uint)size.Height);
            if (window.Size.Width < window.Size.Height)
            {
                shaderProgram.Uniforms["Projection"].SetValueMat4(Matrix4x4.CreateOrthographic(2f * window.Size.Width / window.Size.Height, 2f, 0.01f, 10f));
                mouseMoveScale = 2f / window.Size.Height;
            }
            else
            {
                shaderProgram.Uniforms["Projection"].SetValueMat4(Matrix4x4.CreateOrthographic(2f, 2f * window.Size.Height / window.Size.Width, 0.01f, 10f));
                mouseMoveScale = 2f / window.Size.Width;
            }
        }

        private void OnWindowClosing()
        {
            vertexBuffer.Dispose();
            shaderProgram.Dispose();
            graphicsDevice.Dispose();
        }
    }
}
