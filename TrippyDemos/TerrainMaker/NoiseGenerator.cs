using System;
using System.Numerics;
using TrippyGL.Utils;

namespace TerrainMaker
{
    static class NoiseGenerator
    {
        public static float Random(Vector2 position, in GeneratorSeed seed)
        {
            float r = MathF.Sin(Vector2.Dot(position, seed.DotSeed)) * seed.RandMultiplier;
            return r - MathF.Floor(r);
        }

        public static float Noise(Vector2 position, in GeneratorSeed seed)
        {
            Vector2 floor = new Vector2(MathF.Floor(position.X), MathF.Floor(position.Y));
            Vector2 fract = position - floor;

            return TrippyMath.Lerp(
                TrippyMath.Lerp(Random(floor, seed), Random(floor + Vector2.UnitX, seed), fract.X),
                TrippyMath.Lerp(Random(floor + Vector2.UnitY, seed), Random(floor + Vector2.One, seed), fract.X),
                fract.Y
            );
        }

        public static float Perlin(Vector2 position, in GeneratorSeed seed1, in GeneratorSeed seed2)
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

            return TrippyMath.SmootherStep(TrippyMath.SmootherStep(dbl, dbr, fract.X), TrippyMath.SmootherStep(dtl, dtr, fract.X), fract.Y);
        }

        public static float FractalNoise(Vector2 position, in GeneratorSeed seed)
        {
            float amp = 0.5f;
            float freq = 1;
            float v = 0;
            for (int i = 0; i < 12; i++)
            {
                v += Noise(position * freq, seed) * amp;
                amp /= 2;
                freq *= 2;
            }
            return v;
        }

        public static float GenHeight(Vector2 position)
        {
            float h = 0.1f;
            h += 2.1f * Perlin(position / 16f, GeneratorSeed.HeightmapSeed, GeneratorSeed.HumiditymapSeed);
            h += 0.3f * FractalNoise(position / 4f, GeneratorSeed.HeightmapSeed);
            //h = MathF.Sign(h) * MathF.Pow(Math.Abs(h), 1.3f);
            return h * 6;
        }

        public static float GenHumidity(Vector2 position)
        {
            //return Perlin(position, GeneratorSeed.HeightmapSeed, GeneratorSeed.HumiditymapSeed);
            return FractalNoise(position, GeneratorSeed.HumiditymapSeed);
        }
    }

    readonly struct GeneratorSeed
    {
        public static readonly GeneratorSeed HeightmapSeed = new GeneratorSeed(new Vector2(52.9258f, 76.3911f), 49164.7641f);
        public static readonly GeneratorSeed HumiditymapSeed = new GeneratorSeed(new Vector2(66.7943f, 33.1674f), 69761.6413f);

        public readonly Vector2 DotSeed;
        public readonly float RandMultiplier;

        public GeneratorSeed(Vector2 dotSeed, float randMult)
        {
            DotSeed = dotSeed;
            RandMultiplier = randMult;
        }
    }
}
