using Silk.NET.OpenGL;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using TrippyGL;
using TrippyTestBase;

namespace TextureBatcherTest
{
    class TextureBatcherTest : TestBase
    {
        Stopwatch stopwatch;

        SimpleShaderProgram shaderProgram;

        Texture2D rectangle;
        Texture2D ball;
        Texture2D diamond;

        TextureBatcher textureBatcher;

        protected override void OnLoad()
        {
            SimpleShaderProgramBuilder programBuilder = new SimpleShaderProgramBuilder()
            {
                TextureEnabled = true,
                VertexColorsEnabled = true,
            };
            programBuilder.ConfigureVertexAttribs<VertexColorTexture>();
            shaderProgram = programBuilder.Create(graphicsDevice, true);
            Console.WriteLine("VS Log: " + programBuilder.VertexShaderLog);
            Console.WriteLine("FS Log: " + programBuilder.FragmentShaderLog);
            Console.WriteLine("Program Log: " + programBuilder.ProgramLog);

            rectangle = Texture2DExtensions.FromFile(graphicsDevice, "rectangle.png");
            ball = Texture2DExtensions.FromFile(graphicsDevice, "ball.png");
            diamond = Texture2DExtensions.FromFile(graphicsDevice, "diamond.png");

            textureBatcher = new TextureBatcher(graphicsDevice);
            textureBatcher.SetShaderProgram(shaderProgram);

            graphicsDevice.DepthState = DepthState.None;
            graphicsDevice.BlendState = BlendState.NonPremultiplied;
            graphicsDevice.ClearColor = new Vector4(0.1f, 0.85f, 0.7f, 1f);

            stopwatch = Stopwatch.StartNew();
        }

        protected override void OnRender(double dt)
        {
            float time = (float)stopwatch.Elapsed.TotalSeconds;
            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit);

            PointF mousePos = InputContext.Mice[0].Position;

            textureBatcher.Begin(BatcherBeginMode.OnTheFly);

            const float meh = 70;
            for (float x = -meh; x <= meh; x += meh)
                for (float y = -meh; y <= meh; y += meh)
                    textureBatcher.Draw(rectangle, new Vector2(mousePos.X + x, mousePos.Y + y), null, Color4b.White, new Vector2(1, 1), time, new Vector2(0.5f, 0.5f), (-x - y) / 500f);
            textureBatcher.End();

            Window.SwapBuffers();
        }

        protected override void OnResized(Size size)
        {
            if (size.Width == 0 || size.Height == 0)
                return;

            graphicsDevice.SetViewport(0, 0, (uint)size.Width, (uint)size.Height);

            shaderProgram.Projection = Matrix4x4.CreateOrthographicOffCenter(0, size.Width, size.Height, 0, 0, 1);
        }

        protected override void OnUnload()
        {
            shaderProgram.Dispose();
            rectangle.Dispose();
            ball.Dispose();
            diamond.Dispose();
            textureBatcher.Dispose();
            graphicsDevice.Dispose();
        }
    }
}
