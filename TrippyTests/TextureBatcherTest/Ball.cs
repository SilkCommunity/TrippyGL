using System;
using System.Numerics;
using TrippyGL;
using TrippyGL.Utils;

namespace TextureBatcherTest
{
    class Ball
    {
        public static Texture2D texture;

        Vector2[] trail;
        Vector2 position;
        Vector2 velocity;
        float startY;
        float minheight;
        float scale;

        public Ball()
        {
            trail = new Vector2[5];
            Reset();
        }

        public void Update(float dtSeconds)
        {
            const float gravity = 2560;
            velocity.Y += gravity * dtSeconds;

            position += velocity * dtSeconds;

            if (velocity.X < 0)
            {
                if (position.X <= 0)
                    velocity.X = -velocity.X;
            }
            else
            {
                if (position.X + texture.Width * scale >= TextureBatcherTest.MaxX)
                    velocity.X = -velocity.X;
            }

            if (velocity.Y > 0)
            {
                if (position.Y + texture.Height * scale > TextureBatcherTest.MaxY)
                {
                    velocity.Y = -Math.Abs(velocity.Y * (minheight < startY ? 0.993f : 1.05f));
                    position.Y = TextureBatcherTest.MaxY - texture.Height * scale;
                    minheight = position.Y;
                }
                else
                    minheight = Math.Min(minheight, position.Y);
            }
        }

        public void Draw(TextureBatcher textureBatcher)
        {
            for (int i = trail.Length - 1; i >= 0; i--)
            {
                byte alpha = (byte)(127 - i * 127 / trail.Length);
                textureBatcher.Draw(texture, trail[i], null, new Color4b(255, 255, 255, alpha), scale, 0f);
            }

            textureBatcher.Draw(texture, position, null, Color4b.White, scale, 0f);

            for (int i = trail.Length - 1; i != 0; i--)
                trail[i] = trail[i - 1];
            trail[0] = position;
        }

        public void Reset()
        {
            Random random = TextureBatcherTest.random;
            position.X = random.NextFloat(TextureBatcherTest.MaxX - texture.Width);
            position.Y = random.NextFloat(TextureBatcherTest.MaxY / 8, TextureBatcherTest.MaxY * 0.75f);
            scale = random.NextFloat(0.6f, 1.4f);

            velocity.X = random.NextFloat(-300, 300);
            velocity.Y = random.NextFloat(40);

            startY = position.Y;
            minheight = TextureBatcherTest.MaxY;
        }
    }
}
