using System;
using System.Numerics;
using System.Xml.Serialization;
using TrippyGL.Utils;

namespace TerrainMaker
{
    static class NoiseGenerator
    {
        public static float Random(in Vector2 position, in GeneratorSeed seed)
        {
            float r = MathF.Sin(Vector2.Dot(position, seed.DotSeed)) * seed.RandMultiplier;
            return r - MathF.Floor(r);
        }

        public static float Noise(in Vector2 position, in GeneratorSeed seed)
        {
            Vector2 floor = new Vector2(MathF.Floor(position.X), MathF.Floor(position.Y));
            Vector2 fract = position - floor;

            return TrippyMath.Lerp(
                TrippyMath.Lerp(Random(floor, seed), Random(floor + Vector2.UnitX, seed), fract.X),
                TrippyMath.Lerp(Random(floor + Vector2.UnitY, seed), Random(floor + Vector2.One, seed), fract.X),
                fract.Y
            );
        }

        public static float Perlin(in Vector2 position, in GeneratorSeed seed1, in GeneratorSeed seed2)
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

        public static float FractalNoise(in Vector2 position, int loops, in GeneratorSeed seed)
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

        public static void GenPoint(in Vector2 position, out float humidity, out float vegetation)
        {
            humidity = FractalNoise(position / 64f, 9, GeneratorSeed.HumiditySeed);

            vegetation = Noise(position / 32f, GeneratorSeed.VegetationSeed);
        }

        public static float GenHeight(in Vector2 position)
        {
            GenPoint(position, out float humidity, out float vegetation);

            float height = FractalNoise(position / 200f, 12, GeneratorSeed.HeightSeed) * 128f - 56.25f;
            if (height > 0)
            {
                float m = Math.Clamp(MathF.Pow(height / 50f, 2), 0, 1);
                float a = FractalNoise(position / 32f, 8, GeneratorSeed.HeightSeed) * (3.1f - vegetation * 3.1f);
                a += Perlin(position / 32f, GeneratorSeed.HeightSeed, GeneratorSeed.ExtraSeed1) * (2.5f + vegetation);
                height += m * 38 * a;
            }
            else
            {
                float m = -height / 10f;
                float a = FractalNoise(position / 32f, 10, GeneratorSeed.HeightSeed);
                height -= MathF.Pow(a, 8) * m * 100;
            }

            return height;

            /*float height = 0;
            float mainAtt = (Perlin(position / 512f, GeneratorSeed.HeightSeed, GeneratorSeed.ExtraSeed1) * 2 - 1) * 2.5f;
            mainAtt += (Perlin(position / 256f, GeneratorSeed.ExtraSeed2, GeneratorSeed.VegetationSeed) * 2 - 1) * 1.1f;
            mainAtt += (Perlin(position / 128f, GeneratorSeed.HumiditySeed, GeneratorSeed.HeightSeed) * 2 - 1) * 0.6f;
            mainAtt += (Perlin(position / 64f, GeneratorSeed.ExtraSeed1, GeneratorSeed.HumiditySeed) * 2 - 1) * 0.32f;
            if (mainAtt < 0) mainAtt = MathF.Exp(mainAtt) - 1;
            height += mainAtt * 64 - 20;

            if (mainAtt < 0)
            {
                height += FractalNoise(position / 12f, 12, GeneratorSeed.HeightSeed) * Math.Clamp(-0.5f * mainAtt, 0, 1) * 48;
                height -= 75 * (MathF.Tanh(25 * (-0.82f - mainAtt)) * 0.5f + 0.5f);
            }
            //height = height * MathF.Pow(MathF.Tanh(MathF.Abs(height / 10)), 1);
            return height;*/



            /*float height = 0;
            float h1 = FractalNoise(position / 768f, 8, GeneratorSeed.ExtraSeed1);
            h1 = MathF.Pow(2, h1) - 1;
            float h2 = FractalNoise(position / 35f, 8, GeneratorSeed.HeightSeed) * 96 - 43.2f;
            height += (h1 * 128 - 50) + MathF.Exp((h1 - 0.6f) * 3) * h2;
            return height;*/
        }
    }

    readonly struct GeneratorSeed
    {
        public static readonly GeneratorSeed HeightSeed = new GeneratorSeed(new Vector2(52.9258f, 76.3911f), 49164.7641f);
        public static readonly GeneratorSeed HumiditySeed = new GeneratorSeed(new Vector2(66.7943f, 33.1674f), 69761.6413f);
        public static readonly GeneratorSeed VegetationSeed = new GeneratorSeed(new Vector2(37.8254f, 53.2556f), 51338.1952f);
        public static readonly GeneratorSeed ExtraSeed1 = new GeneratorSeed(new Vector2(61.5137f, 49.4915f), 43913.5758f);
        public static readonly GeneratorSeed ExtraSeed2 = new GeneratorSeed(new Vector2(42.3791f, 54.3171f), 56123.2499f);
        public static readonly GeneratorSeed ExtraSeed3 = new GeneratorSeed(new Vector2(49.2583f, 69.4277f), 57912.5311f);

        public readonly Vector2 DotSeed;
        public readonly float RandMultiplier;

        public GeneratorSeed(Vector2 dotSeed, float randMult)
        {
            DotSeed = dotSeed;
            RandMultiplier = randMult;
        }
    }
}
