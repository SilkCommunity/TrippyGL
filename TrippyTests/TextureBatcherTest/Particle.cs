using System;
using System.Drawing;
using System.Numerics;
using TrippyGL;

namespace TextureBatcherTest
{
    class Particle
    {
        public static Texture2D texture;
        public static readonly Rectangle[] sources = new Rectangle[]
        {
            new Rectangle(0, 0, 3, 3),
            new Rectangle(3, 0, 4, 4),
            new Rectangle(7, 0, 4, 4),
            new Rectangle(5, 4, 5, 5),
            new Rectangle(0, 4, 5, 5),
            new Rectangle(11, 0, 7, 7),
            new Rectangle(18, 0, 9, 9),
        };

        Color4b color;
        Rectangle source;
        public Vector2 position;
        Vector2 velocity;
        float rotation;
        float rotSpeed;

        public Particle(Vector2 position)
        {
            Random r = TextureBatcherTest.random;
            source = sources[TextureBatcherTest.random.Next(sources.Length)];
            this.position = position;
            const float spd = 300;
            velocity = new Vector2(r.NextFloat(-spd, spd), r.NextFloat(-spd, spd));
            color = Color4b.RandomFullAlpha(r);
            rotation = r.NextFloat(MathF.PI * 2);
            rotSpeed = r.NextFloat(-10, 10);
        }

        public Particle()
        {
            Random r = TextureBatcherTest.random;
            source = sources[TextureBatcherTest.random.Next(sources.Length)];
            position = new Vector2(r.NextFloat(TextureBatcherTest.MaxX), r.NextFloat(TextureBatcherTest.MaxY));
            const float spd = 300;
            velocity = new Vector2(r.NextFloat(-spd, spd), r.NextFloat(-spd, spd));
            color = Color4b.RandomFullAlpha(r);
            rotation = r.NextFloat(MathF.PI * 2);
            rotSpeed = r.NextFloat(-10, 10);
        }

        public void Update(float dtSeconds)
        {
            position += velocity * dtSeconds;
            rotation += rotSpeed * dtSeconds;
        }

        public void Draw(TextureBatcher textureBatcher)
        {
            textureBatcher.Draw(texture, position, source, color, 5f, rotation, new Vector2(0.5f, 0.5f));
        }

        public bool IsOffscreen()
        {
            return position.X < -100 || position.X - 100 > TextureBatcherTest.MaxX
                || position.Y < -100 || position.Y - 100 > TextureBatcherTest.MaxY;
        }
    }
}
