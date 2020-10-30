using System;
using System.Numerics;
using TrippyGL.Utils;

namespace TerrainMaker
{
    static class NoiseGenerator
    {
        public static float Random(in Vector2 position, in NoiseSeed seed)
        {
            float r = MathF.Sin(Vector2.Dot(position, seed.DotSeed)) * seed.RandMultiplier;
            return r - MathF.Floor(r);
        }

        public static float Noise(in Vector2 position, in NoiseSeed seed)
        {
            Vector2 floor = new Vector2(MathF.Floor(position.X), MathF.Floor(position.Y));
            Vector2 fract = position - floor;

            return TrippyMath.Lerp(
                TrippyMath.Lerp(Random(floor, seed), Random(floor + Vector2.UnitX, seed), fract.X),
                TrippyMath.Lerp(Random(floor + Vector2.UnitY, seed), Random(floor + Vector2.One, seed), fract.X),
                fract.Y
            );
        }

        public static float Perlin(in Vector2 position, in NoiseSeed seed1, in NoiseSeed seed2)
        {
            Vector2 floor = new Vector2(MathF.Floor(position.X), MathF.Floor(position.Y));
            Vector2 fract = position - floor;

            Vector2 blvec = new Vector2(Random(floor, seed1), Random(floor, seed2)) * 2f - Vector2.One;
            Vector2 brvec = new Vector2(Random(floor + Vector2.UnitX, seed1), Random(floor + Vector2.UnitX, seed2)) * 2f - Vector2.One;
            Vector2 tlvec = new Vector2(Random(floor + Vector2.UnitY, seed1), Random(floor + Vector2.UnitY, seed2)) * 2f - Vector2.One;
            Vector2 trvec = new Vector2(Random(floor + Vector2.One, seed1), Random(floor + Vector2.One, seed2)) * 2f - Vector2.One;

            Vector2 bldiff = fract;
            Vector2 trdiff = fract - Vector2.One;
            Vector2 brdiff = new Vector2(trdiff.X, bldiff.Y);
            Vector2 tldiff = new Vector2(bldiff.X, trdiff.Y);

            float dbl = Vector2.Dot(blvec, bldiff);
            float dbr = Vector2.Dot(brvec, brdiff);
            float dtl = Vector2.Dot(tlvec, tldiff);
            float dtr = Vector2.Dot(trvec, trdiff);

            return TrippyMath.SmootherStep(TrippyMath.SmootherStep(dbl, dbr, fract.X), TrippyMath.SmootherStep(dtl, dtr, fract.X), fract.Y) * 0.5f + 0.5f;
        }

        public static float FractalNoise(in Vector2 position, int loops, in NoiseSeed seed)
        {
            float amp = 0.5f;
            float freq = 1;
            float v = 0;
            for (int i = 0; i < loops; i++)
            {
                v += Noise(position * freq, seed) * amp;
                amp /= 2;
                freq *= 2;
            }
            return v;
        }

        public static void GenPoint(GeneratorSeed seed, in Vector2 position, out float humidity, out float vegetation)
        {
            humidity = FractalNoise(position / 64f, 9, seed.HumiditySeed);

            vegetation = Noise(position / 32f, seed.VegetationSeed);
        }

        public static float GenHeight(GeneratorSeed seed, in Vector2 position)
        {
            GenPoint(seed, position, out float humidity, out float vegetation);

            float height = FractalNoise(position / 200f, 12, seed.HeightSeed) * 128f - 56.25f;
            if (height > 0)
            {
                float m = Math.Clamp(MathF.Pow(height / 50f, 2), 0, 1);
                float a = FractalNoise(position / 32f, 8, seed.HeightSeed) * (3.1f - vegetation * 3.1f);
                a += Perlin(position / 32f, seed.HeightSeed, seed.ExtraSeed1) * (2.5f + vegetation);
                height += m * 38 * a;
            }
            else
            {
                float m = -height / 10f;
                float a = FractalNoise(position / 32f, 10, seed.HeightSeed);
                height -= MathF.Pow(a, 8) * m * 100;
            }

            return height;
        }
    }
}
