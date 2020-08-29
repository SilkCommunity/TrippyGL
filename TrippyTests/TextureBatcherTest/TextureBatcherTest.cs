using Silk.NET.Input.Common;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using TrippyGL;
using TrippyTestBase;

namespace TextureBatcherTest
{
    class TextureBatcherTest : TestBase
    {
        public static int MaxX, MaxY;
        public static Random random = new Random();

        Stopwatch stopwatch;

        SimpleShaderProgram shaderProgram;

        Texture2D particleTexture;
        Texture2D rectangleTexture;
        Texture2D ballTexture;
        Texture2D diamondTexture;

        TextureBatcher textureBatcher;

        LinkedList<Particle> particles;
        Diamond[] diamonds;
        Ball[] balls;

        protected override void OnLoad()
        {
            SimpleShaderProgramBuilder programBuilder = new SimpleShaderProgramBuilder()
            {
                TextureEnabled = true,
                VertexColorsEnabled = true,
                ExcludeWorldMatrix = true
            };
            programBuilder.ConfigureVertexAttribs<VertexColorTexture>();
            shaderProgram = programBuilder.Create(graphicsDevice, true);
            Console.WriteLine("VS Log: " + programBuilder.VertexShaderLog);
            Console.WriteLine("FS Log: " + programBuilder.FragmentShaderLog);
            Console.WriteLine("Program Log: " + programBuilder.ProgramLog);

            particleTexture = Texture2DExtensions.FromFile(graphicsDevice, "particles.png");
            particleTexture.SetTextureFilters(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            rectangleTexture = Texture2DExtensions.FromFile(graphicsDevice, "rectangle.png");
            ballTexture = Texture2DExtensions.FromFile(graphicsDevice, "ball.png");
            diamondTexture = Texture2DExtensions.FromFile(graphicsDevice, "diamond.png");

            textureBatcher = new TextureBatcher(graphicsDevice);
            textureBatcher.SetShaderProgram(shaderProgram);

            graphicsDevice.DepthState = DepthState.None;
            graphicsDevice.BlendState = BlendState.NonPremultiplied;
            graphicsDevice.ClearColor = new Vector4(0.1f, 0.65f, 0.5f, 1f);

            MaxX = Window.Size.Width;
            MaxY = Window.Size.Height;

            Particle.texture = particleTexture;
            Diamond.texture = diamondTexture;
            Ball.texture = ballTexture;

            particles = new LinkedList<Particle>();

            diamonds = new Diamond[40];
            for (int i = 0; i < diamonds.Length; i++)
                diamonds[i] = new Diamond();

            balls = new Ball[40];
            for (int i = 0; i < balls.Length; i++)
                balls[i] = new Ball();

            stopwatch = Stopwatch.StartNew();
        }

        protected override void OnUpdate(double dt)
        {
            float dtSeconds = (float)dt;

            LinkedListNode<Particle> node = particles.First;
            while (node != null)
            {
                LinkedListNode<Particle> next = node.Next;
                if (node.Value.IsOffscreen())
                    particles.Remove(node);
                else
                    node.Value.Update(dtSeconds);
                node = next;
            }

            if (InputContext.Mice[0].IsButtonPressed(MouseButton.Middle))
            {
                PointF mousePos = InputContext.Mice[0].Position;
                for (int i = 0; i < 12; i++)
                    particles.AddLast(new Particle(new Vector2(mousePos.X, mousePos.Y)));
            }

            for (int i = random.Next(1, 4); i != 0; i--)
                particles.AddLast(new Particle());

            for (int i = 0; i < diamonds.Length; i++)
                diamonds[i].Update(dtSeconds);

            for (int i = 0; i < balls.Length; i++)
                balls[i].Update(dtSeconds);
        }

        protected override void OnRender(double dt)
        {
            float time = (float)stopwatch.Elapsed.TotalSeconds;
            graphicsDevice.Clear(ClearBufferMask.ColorBufferBit);

            PointF mousePos = InputContext.Mice[0].Position;

            textureBatcher.Begin(BatcherBeginMode.OnTheFly);

            foreach (Particle particle in particles)
                particle.Draw(textureBatcher);

            for (int i = 0; i < diamonds.Length; i++)
                diamonds[i].Draw(textureBatcher);

            for (int i = 0; i < balls.Length; i++)
                balls[i].Draw(textureBatcher);

            const float meh = 100;
            const int times = 3;
            for (float x = -meh * times; x <= meh * times; x += meh)
                for (float y = -meh * times; y <= meh * times; y += meh)
                {
                    byte alpha = (byte)Math.Max(0, 255 - Math.Abs(x) - Math.Abs(y));
                    textureBatcher.Draw(rectangleTexture, new Vector2(mousePos.X + x, mousePos.Y + y), null, new Color4b(255, 255, 255, alpha), new Vector2(1, 1), time, new Vector2(0.5f, 0.5f), (-x - y) / 500f);
                }
            textureBatcher.End();

            Window.SwapBuffers();
        }

        protected override void OnKeyDown(IKeyboard sender, Key key, int n)
        {
            if (key == Key.Space)
            {
                for (int i = 0; i < diamonds.Length; i++)
                    diamonds[i].Reset();
                for (int i = 0; i < balls.Length; i++)
                    balls[i].Reset();
            }
        }

        protected override void OnResized(Size size)
        {
            if (size.Width == 0 || size.Height == 0)
                return;

            MaxX = size.Width;
            MaxY = size.Height;
            graphicsDevice.SetViewport(0, 0, (uint)size.Width, (uint)size.Height);

            shaderProgram.Projection = Matrix4x4.CreateOrthographicOffCenter(0, size.Width, size.Height, 0, 0, 1);
        }

        protected override void OnUnload()
        {
            shaderProgram.Dispose();
            particleTexture.Dispose();
            rectangleTexture.Dispose();
            ballTexture.Dispose();
            diamondTexture.Dispose();
            textureBatcher.Dispose();
            graphicsDevice.Dispose();
        }
    }
}
