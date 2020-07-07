using System;
using Silk.NET.Input.Common;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Common;
using System.Drawing;
using Silk.NET.Input;
using TrippyGL;
using Silk.NET.OpenGL;

namespace TrippyTestBase
{
    /// <summary>
    /// A base for all test projects that contains shared code.
    /// </summary>
    public abstract class TestBase
    {
        public IWindow Window { private set; get; }
        public IInputContext InputContext { private set; get; }

        public bool AllowToggleFullscreen = true;
        private Size preFullscreenSize;
        private Point preFullscreenPosition;

        public bool IsFullscreen
        {
            get { return Window.WindowState == WindowState.Fullscreen; }
            set
            {
                if (value == IsFullscreen)
                    return;

                if (value)
                {
                    Size screenSize;
                    if (Window.Monitor.VideoMode.Resolution.HasValue)
                        screenSize = Window.Monitor.VideoMode.Resolution.Value;
                    else
                        screenSize = new Size(Window.Monitor.Bounds.Width, Window.Monitor.Bounds.Height);
                    preFullscreenSize = Window.Size;
                    preFullscreenPosition = Window.Position;
                    Window.WindowState = WindowState.Fullscreen;
                    Window.Size = screenSize;
                }
                else
                {
                    Window.WindowState = WindowState.Normal;
                    if (preFullscreenSize.Width < 10 || preFullscreenSize.Height < 10)
                    {
                        preFullscreenSize = GetNewWindowSize(Window.Monitor);
                        preFullscreenPosition = new Point(50, 50);
                    }

                    Window.Size = preFullscreenSize;
                    Window.Position = preFullscreenPosition;
                }
            }
        }

        public GraphicsDevice graphicsDevice;

        public TestBase(string title = null, int preferredDepthBufferBits = 0)
        {
            GraphicsAPI graphicsApi = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.Debug, new APIVersion(3, 3));
            VideoMode videoMode = new VideoMode(new Size(1280, 720));
            ViewOptions viewOpts = new ViewOptions(true, 60.0, 60.0, graphicsApi, VSyncMode.On, 30, false, videoMode, 24);
            WindowOptions fuckme = new WindowOptions(viewOpts);


            Size windowSize = GetNewWindowSize(Monitor.GetMainMonitor());
            WindowOptions windowOpts = new WindowOptions()
            {
                API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.Debug, new APIVersion(3, 3)),
                VSync = VSyncMode.On,
                UseSingleThreadedWindow = true,
                RunningSlowTolerance = 30,
                Size = windowSize,
                VideoMode = new VideoMode(windowSize),
                PreferredDepthBufferBits = preferredDepthBufferBits,
                ShouldSwapAutomatically = false,
                Title = title ?? System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "Title",
                Position = new Point(50, 50)
            };

            Window = Silk.NET.Windowing.Window.Create(windowOpts);
        }

        public void Run()
        {
            Window.Load += Window_Load;
            Window.Update += OnUpdate;
            Window.Render += Window_Render;
            Window.Resize += OnResized;
            Window.Closing += OnUnload;

            Window.Run();
        }

        private void Window_Load()
        {
            InputContext = Window.CreateInput();
            InputContext.ConnectionChanged += InputContext_ConnectionChanged;
            foreach (IKeyboard keyboard in InputContext.Keyboards)
                InputContext_ConnectionChanged(keyboard, keyboard.IsConnected);
            foreach (IMouse mouse in InputContext.Mice)
                InputContext_ConnectionChanged(mouse, mouse.IsConnected);

            graphicsDevice = new GraphicsDevice(GL.GetApi(Window));
            graphicsDevice.DebugMessagingEnabled = true;
            graphicsDevice.DebugMessage += OnDebugMessage;

            Console.WriteLine(string.Concat("GL Version: ", graphicsDevice.GLMajorVersion, ".", graphicsDevice.GLMinorVersion));
            Console.WriteLine("GL Version String: " + graphicsDevice.GLVersion);
            Console.WriteLine("GL Vendor: " + graphicsDevice.GLVendor);
            Console.WriteLine("GL Renderer: " + graphicsDevice.GLRenderer);
            Console.WriteLine("GL ShadingLanguageVersion: " + graphicsDevice.GLShadingLanguageVersion);
            Console.WriteLine("GL TextureUnits: " + graphicsDevice.MaxTextureImageUnits);
            Console.WriteLine("GL MaxTextureSize: " + graphicsDevice.MaxTextureSize);
            Console.WriteLine("GL MaxSamples: " + graphicsDevice.MaxSamples);

            OnLoad();
            OnResized(Window.Size);
        }

        private void InputContext_ConnectionChanged(IInputDevice device, bool status)
        {
            if (device is IKeyboard keyboard)
            {
                if (device.IsConnected)
                {
                    keyboard.KeyDown += Keyboard_KeyDown;
                    keyboard.KeyUp += OnKeyUp;
                    keyboard.KeyChar += OnKeyChar;
                }
                else
                {
                    keyboard.KeyDown -= Keyboard_KeyDown;
                    keyboard.KeyUp -= OnKeyUp;
                    keyboard.KeyChar -= OnKeyChar;
                }
            }
            else if (device is IMouse mouse)
            {
                if (device.IsConnected)
                {
                    mouse.MouseDown += OnMouseDown;
                    mouse.MouseMove += OnMouseMove;
                    mouse.MouseUp += OnMouseUp;
                    mouse.Scroll += OnMouseScroll;
                }
                else
                {
                    mouse.MouseDown -= OnMouseDown;
                    mouse.MouseMove -= OnMouseMove;
                    mouse.MouseUp -= OnMouseUp;
                    mouse.Scroll -= OnMouseScroll;
                }
            }
        }

        private void Keyboard_KeyDown(IKeyboard sender, Key key, int n)
        {
            if (key == Key.F11 && AllowToggleFullscreen)
                IsFullscreen = !IsFullscreen;

            if (key == Key.Escape)
                Window.Close();

            OnKeyDown(sender, key, n);
        }

        private void Window_Render(double dt)
        {
            if (!Window.IsClosing)
                OnRender(dt);
        }

        protected abstract void OnLoad();
        protected abstract void OnRender(double dt);
        protected abstract void OnResized(Size size);
        protected abstract void OnUnload();

        protected virtual void OnUpdate(double dt)
        {
            GLEnum c;
            while ((c = graphicsDevice.GL.GetError()) != GLEnum.NoError)
            {
                Console.WriteLine("GL Error found: " + c);
            }
        }

        protected virtual void OnKeyDown(IKeyboard sender, Key key, int n) { }
        protected virtual void OnKeyUp(IKeyboard sender, Key key, int n) { }
        protected virtual void OnKeyChar(IKeyboard sender, char key) { }

        protected virtual void OnMouseDown(IMouse sender, MouseButton button) { }
        protected virtual void OnMouseMove(IMouse sender, PointF position) { }
        protected virtual void OnMouseUp(IMouse sender, MouseButton button) { }
        protected virtual void OnMouseScroll(IMouse sender, ScrollWheel scroll) { }

        /// <summary>
        /// Calculates the size to use for a new window as two thirds the size of the main monitor.
        /// </summary>
        /// <param name="monitor">The monitor in which the window will be located.</param>
        private static Size GetNewWindowSize(IMonitor monitor)
        {
            if (monitor.VideoMode.Resolution.HasValue)
            {
                Size s = monitor.VideoMode.Resolution.Value;
                return new Size(s.Width * 2 / 3, s.Height * 2 / 3);
            }
            return new Size(monitor.Bounds.Width * 2 / 3, monitor.Bounds.Height * 2 / 3);
        }

        private static void OnDebugMessage(DebugSource debugSource, DebugType debugType, int messageId, DebugSeverity debugSeverity, string message)
        {
            if (messageId != 131185 && messageId != 131186)
                Console.WriteLine(string.Concat("Debug message: source=", debugSource.ToString(), " type=", debugType.ToString(), " id=", messageId.ToString(), " severity=", debugSeverity.ToString(), " message=\"", message, "\""));
        }
    }
}
