using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Input.Common;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Common;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using TrippyGL;
using TrippyGL.Fonts;
using TrippyGL.Fonts.Building;
using TrippyGL.Fonts.Extensions;

namespace TrippyTesting
{
    class TextGame
    {
        Stopwatch stopwatch;

        readonly Random r = new Random();
        readonly IWindow window;
        IInputContext inputContext;

        GraphicsDevice graphicsDevice;

        SimpleShaderProgram program;
        TextureBatcher batcher;

        string str = "Text!";
        Texture2D whitepx;
        TextureFont[] fonts;
        TextureFont font;
        Vector2 scale = new Vector2(1, 1);
        Vector2 origin = new Vector2(0, 0);

        public TextGame()
        {
            window = CreateWindow();

            window.Load += OnWindowLoad;
            window.Update += OnWindowUpdate;
            window.Render += OnWindowRender;
            window.Resize += OnWindowResized;
            window.Closing += OnWindowClosing;
        }

        private void TextGame_KeyChar(IKeyboard sender, char c)
        {
            if (c == '\b' && str.Length != 0)
            {
                str = str.Substring(0, str.Length - 1);
            }
            else if (c == '\n' || font.HasCharacter(c))
            {
                str += c;
            }
        }

        private void TextGame_KeyDown(IKeyboard sender, Key key, int i_dont_know_what_the_fuck_this_int_is_for)
        {
            if (key == Key.Backspace)
                TextGame_KeyChar(sender, '\b');
            else if (key == Key.Enter)
                TextGame_KeyChar(sender, '\n');
        }

        private IWindow CreateWindow()
        {
            GraphicsAPI graphicsApi = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.Debug, new APIVersion(4, 0));
            VideoMode videoMode = new VideoMode(new System.Drawing.Size(1280, 720));
            ViewOptions viewOpts = new ViewOptions(true, 60.0, 60.0, graphicsApi, VSyncMode.On, 30, false, videoMode, 0);
            return Window.Create(new WindowOptions(viewOpts));
        }

        public void OnWindowLoad()
        {
            inputContext = window.CreateInput();
            inputContext.Keyboards[0].KeyChar += TextGame_KeyChar;
            inputContext.Keyboards[0].KeyDown += TextGame_KeyDown;

            graphicsDevice = new GraphicsDevice(GL.GetApi(window));
            graphicsDevice.DebugMessagingEnabled = true;
            graphicsDevice.DebugMessage += Program.OnDebugMessage;

            program = SimpleShaderProgram.Create<VertexColorTexture>(graphicsDevice, 0, 0, true);
            batcher = new TextureBatcher(graphicsDevice);
            batcher.SetShaderProgram(program);

            whitepx = new Texture2D(graphicsDevice, 1, 1);
            whitepx.SetData<Color4b>(new Color4b[] { Color4b.White });

            string someFileName = "font.tglf";
            TrippyFontFile trippyFontFile;
            if (!File.Exists(someFileName))
            {
                FontFamily family = SystemFonts.Find("Arial");
                //Font fontFile = SystemFonts.CreateFont("Arial", 72f, FontStyle.Regular);
                //TrippyFontFile trippyFontFile = FontBuilder.CreateFontFile(fontFile);

                trippyFontFile = FontBuilderExtensions.CreateFontFile(
                    new Font[] { family.CreateFont(72f, FontStyle.Regular), family.CreateFont(64f, FontStyle.Italic),
                             family.CreateFont(56f, FontStyle.Bold), family.CreateFont(48f, FontStyle.BoldItalic)});

                trippyFontFile.WriteToFile(someFileName);
                trippyFontFile.Image.SaveAsPng("pitaiken.png");
            }
            else
                trippyFontFile = TrippyFontFile.FromFile(someFileName);

            fonts = trippyFontFile.CreateFonts(graphicsDevice);
            font = fonts[0];

            stopwatch = Stopwatch.StartNew();

            OnWindowResized(window.Size);
        }

        public void Run()
        {
            window.Run();
        }

        public void OnWindowUpdate(double dtSeconds)
        {
            GLEnum c;
            while ((c = graphicsDevice.GL.GetError()) != GLEnum.NoError)
            {
                Console.WriteLine("Error found: " + c);
            }

            if (inputContext.Keyboards[0].IsKeyPressed(Key.ShiftLeft))
            {
                const float pepe = 50f;
                if (inputContext.Keyboards[0].IsKeyPressed(Key.Up))
                    origin.Y += (float)dtSeconds * pepe;
                else if (inputContext.Keyboards[0].IsKeyPressed(Key.Down))
                    origin.Y -= (float)dtSeconds * pepe;
                if (inputContext.Keyboards[0].IsKeyPressed(Key.Left))
                    origin.X += (float)dtSeconds * pepe;
                else if (inputContext.Keyboards[0].IsKeyPressed(Key.Right))
                    origin.X -= (float)dtSeconds * pepe;
            }
            else
            {
                const float pito = 0.75f;
                if (inputContext.Keyboards[0].IsKeyPressed(Key.Up))
                    scale.Y += (float)dtSeconds * pito;
                else if (inputContext.Keyboards[0].IsKeyPressed(Key.Down))
                    scale.Y -= (float)dtSeconds * pito;
                if (inputContext.Keyboards[0].IsKeyPressed(Key.Left))
                    scale.X += (float)dtSeconds * pito;
                else if (inputContext.Keyboards[0].IsKeyPressed(Key.Right))
                    scale.X -= (float)dtSeconds * pito;
            }


            Console.WriteLine("scale=" + scale.ToString() + " origin=" + origin.ToString());
        }

        public void OnWindowRender(double dtSeconds)
        {
            if (window.IsClosing)
                return;

            font = fonts[(stopwatch.ElapsedMilliseconds / 500) % fonts.Length];
            font = fonts[0];

            graphicsDevice.BlendingEnabled = true;
            graphicsDevice.BlendState = BlendState.NonPremultiplied;
            graphicsDevice.DepthTestingEnabled = false;
            graphicsDevice.ClearColor = new Vector4(0, 0, 0, 1);
            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit);

            batcher.Begin(BatcherBeginMode.OnTheFly);
            Vector2 measured = font.Measure(str) * scale;
            Vector2 position = new Vector2(inputContext.Mice[0].Position.X, inputContext.Mice[0].Position.Y);


            batcher.Draw(whitepx, position, null, Color4b.Red, new Vector2(measured.X, 1), 0f);
            batcher.Draw(whitepx, position, null, Color4b.Red, new Vector2(1, measured.Y), 0f);
            batcher.Draw(whitepx, position + new Vector2(0, measured.Y), null, Color4b.Red, new Vector2(measured.X, 1), 0f);
            batcher.Draw(whitepx, position + new Vector2(measured.X, 0), null, Color4b.Red, new Vector2(1, measured.Y), 0f);


            batcher.DrawString(font, str, position, Color4b.DimGray, scale, origin);
            batcher.DrawString(font, str, position, Color4b.White, scale, (float)stopwatch.Elapsed.TotalSeconds, origin);

            batcher.Draw(whitepx, position, null, Color4b.Lime, new Vector2(500, 1), (float)stopwatch.Elapsed.TotalSeconds);
            batcher.End();

            window.SwapBuffers();
        }

        public void OnWindowResized(System.Drawing.Size size)
        {
            if (size.Width == 0 || size.Height == 0)
                return;

            graphicsDevice.SetViewport(0, 0, (uint)size.Width, (uint)size.Height);
            program.Projection = Matrix4x4.CreateOrthographicOffCenter(0, size.Width, size.Height, 0, 0, 1);
        }

        public void OnWindowClosing()
        {
            graphicsDevice.DisposeAllResources();
            graphicsDevice.Dispose();
        }
    }
}
