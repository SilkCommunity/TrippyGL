using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using Silk.NET.Input.Common;
using TrippyGL;
using TrippyGL.Fonts.Extensions;
using TrippyGL.ImageSharp;
using TrippyTestBase;

namespace TextureBatcherTest
{
    class TextureBatcherTest : TestBase
    {
        public static int MaxX, MaxY;
        public static Random random = new Random();

        const string loremIpsum = "Lorem ipsum dolor sit amet, consectetur adipiscing\nelit, sed eiusmod tempor incidunt ut labore et dolore\nmagna aliqua. Ut enim ad minim veniam, quis\nnostrud exercitation ullamco laboris nisi ut aliquid\nex ea commodi consequat. Quis aute iure\nreprehenderit in voluptate velit esse cillum dolore eu\nfugiat nulla pariatur. Excepteur sint obcaecat\ncupiditat non proident, sunt in culpa qui officia\ndeserunt mollit anim id est laborum.";

        Stopwatch stopwatch;

        SimpleShaderProgram shaderProgram;

        Texture2D particleTexture;
        Texture2D rectangleTexture;
        Texture2D ballTexture;
        Texture2D diamondTexture;

        TextureFont arialFont;
        TextureFont comicSansFont;

        TextureBatcher textureBatcher;

        LinkedList<Particle> particles;
        Diamond[] diamonds;
        Ball[] balls;

        protected override void OnLoad()
        {
            shaderProgram = SimpleShaderProgram.Create<VertexColorTexture>(graphicsDevice, 0, 0, true);

            particleTexture = Texture2DExtensions.FromFile(graphicsDevice, "particles.png");
            particleTexture.SetTextureFilters(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            rectangleTexture = Texture2DExtensions.FromFile(graphicsDevice, "rectangle.png");
            ballTexture = Texture2DExtensions.FromFile(graphicsDevice, "ball.png");
            diamondTexture = Texture2DExtensions.FromFile(graphicsDevice, "diamond.png");

            TextureFont[] fonts = TextureFontExtensions.FromFile(graphicsDevice, "font.tglf");
            comicSansFont = fonts[0];
            arialFont = fonts[1];

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

            graphicsDevice.CullFaceMode = CullingMode.CullBack;
            graphicsDevice.FaceCullingEnabled = true;
        }

        protected override void OnUpdate(double dt)
        {
            float dtSeconds = Math.Min(0.2f, (float)dt);

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
            graphicsDevice.Clear(ClearBuffer.Color);

            Vector2 mousePos = new Vector2(InputContext.Mice[0].Position.X, InputContext.Mice[0].Position.Y);

            textureBatcher.Begin(BatcherBeginMode.OnTheFly);

            float loreIpsumScale = 1 + 0.1f * MathF.Sin(time * 4.32f);
            textureBatcher.DrawString(arialFont, loremIpsum, new Vector2(50, 25), Color4b.White, loreIpsumScale, Vector2.Zero);

            string comicText = "You can draw text!!\nIt's very nice :)";
            Vector2 comicTextSize = comicSansFont.Measure(comicText);
            Color4b comicTextColor = Color4b.FromHSV(time % 1, 1f, 1f);
            float comicTextScale = 1 + 0.1f * MathF.Sin(time);
            float comicTextRot = 0.3f * MathF.Sin(time * 4.32f);
            textureBatcher.DrawString(comicSansFont, comicText, new Vector2(500, 610), comicTextColor, comicTextScale, comicTextRot, comicTextSize / 2f);

            foreach (Particle particle in particles)
                particle.Draw(textureBatcher);
            for (int i = 0; i < diamonds.Length; i++)
                diamonds[i].Draw(textureBatcher);
            for (int i = 0; i < balls.Length; i++)
                balls[i].Draw(textureBatcher);

            textureBatcher.End();


            textureBatcher.Begin(BatcherBeginMode.SortBackToFront);
            Vector2 rectOrigin = new Vector2(rectangleTexture.Width / 2f, rectangleTexture.Height / 2f);
            const float meh = 100;
            const int times = 6;
            for (float x = -meh * times; x <= meh * times; x += meh)
                for (float y = -meh * times; y <= meh * times; y += meh)
                {
                    float dist = Math.Abs(x) + Math.Abs(y);
                    byte alpha = (byte)Math.Max(0, 255 - (3f / times) * dist);
                    float dep = dist / 500f;
                    float sc = dist / meh * 0.175f;
                    sc = 1 - sc * sc;
                    Vector2 pos = mousePos + new Vector2(MathF.Sign(x), MathF.Sign(y)) * Vector2.SquareRoot(new Vector2(MathF.Abs(x), MathF.Abs(y)) / meh) * meh;
                    textureBatcher.Draw(rectangleTexture, pos, null, new Color4b(255, 255, 255, alpha), sc, time, rectOrigin, dep);
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
            arialFont.Dispose(); // both fonts share texture, disposing one disposes both.
            textureBatcher.Dispose();
        }
    }
}
