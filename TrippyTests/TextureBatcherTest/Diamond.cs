using System;
using System.Numerics;
using TrippyGL;
using TrippyGL.Utils;

namespace TextureBatcherTest
{
    class Diamond
    {
        public static Texture2D texture;

        Vector2[] trail;
        Vector2 position;
        Vector2 velocity;

        public Diamond()
        {
            trail = new Vector2[5];
            Reset();
        }

        public void Update(float dtSeconds)
        {
            position += dtSeconds * velocity;

            if (velocity.X < 0)
            {
                if (position.X <= 0)
                    velocity.X = -velocity.X;
            }
            else
            {
                if (position.X + texture.Width >= TextureBatcherTest.MaxX)
                    velocity.X = -velocity.X;
            }

            if (velocity.Y < 0)
            {
                if (position.Y <= 0)
                    velocity.Y = -velocity.Y;
            }
            else
            {
                if (position.Y + texture.Height >= TextureBatcherTest.MaxY)
                    velocity.Y = -velocity.Y;
            }
        }

        public void Draw(TextureBatcher textureBatcher)
        {
            for (int i = trail.Length - 1; i >= 0; i--)
            {
                byte alpha = (byte)(127 - i * 127 / trail.Length);
                textureBatcher.Draw(texture, trail[i], new Color4b(255, 255, 255, alpha));
            }

            textureBatcher.Draw(texture, position);

            for (int i = trail.Length - 1; i != 0; i--)
                trail[i] = trail[i - 1];
            trail[0] = position;
        }

        public void Reset()
        {
            Random random = TextureBatcherTest.random;

            position.X = random.NextFloat(TextureBatcherTest.MaxX - texture.Width);
            position.Y = random.NextFloat(TextureBatcherTest.MaxY - texture.Height);

            const float spd = 400;
            velocity.X = random.NextFloat(-spd, spd);
            velocity.Y = random.NextFloat(-spd, spd);
        }
    }
}
