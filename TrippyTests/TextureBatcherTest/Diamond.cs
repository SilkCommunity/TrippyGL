using System;
using System.Numerics;
using TrippyGL;

namespace TextureBatcherTest
{
    class Diamond
    {
        public static Texture2D texture;

        Vector2 position;
        Vector2 velocity;

        public Diamond()
        {
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
            textureBatcher.Draw(texture, position, Color4b.White);
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
