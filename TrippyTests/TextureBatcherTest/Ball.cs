using System;
using System.Numerics;
using TrippyGL;

namespace TextureBatcherTest
{
    class Ball
    {
        public static Texture2D texture;

        Vector2 position;
        Vector2 velocity;
        float startY;
        float minheight;

        public Ball()
        {
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
                if (position.X + texture.Width >= TextureBatcherTest.MaxX)
                    velocity.X = -velocity.X;
            }

            if (velocity.Y > 0)
            {
                if (position.Y + texture.Height > TextureBatcherTest.MaxY)
                {
                    velocity.Y = -Math.Abs(velocity.Y * (minheight < startY ? 0.993f : 1.05f));
                    position.Y = TextureBatcherTest.MaxY - texture.Height;
                    minheight = position.Y;
                }
                else
                    minheight = Math.Min(minheight, position.Y);
            }
        }

        public void Draw(TextureBatcher textureBatcher)
        {
            textureBatcher.Draw(texture, position, Color4b.White);
        }

        public void Reset()
        {
            Random random = TextureBatcherTest.random;
            position.X = random.NextFloat(TextureBatcherTest.MaxX - texture.Width);
            position.Y = random.NextFloat(TextureBatcherTest.MaxY / 8, TextureBatcherTest.MaxY * 0.75f);

            velocity.X = random.NextFloat(-300, 300);
            velocity.Y = random.NextFloat(40);

            startY = position.Y;
            minheight = TextureBatcherTest.MaxY;
        }
    }
}
