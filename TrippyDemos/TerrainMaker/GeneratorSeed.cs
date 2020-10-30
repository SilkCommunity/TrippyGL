using System;
using System.Numerics;
using TrippyGL.Utils;

namespace TerrainMaker
{
    class GeneratorSeed
    {
        public readonly NoiseSeed HeightSeed;
        public readonly NoiseSeed HumiditySeed;
        public readonly NoiseSeed VegetationSeed;
        public readonly NoiseSeed ExtraSeed1;

        public GeneratorSeed(in NoiseSeed heightSeed, in NoiseSeed humiditySeed, in NoiseSeed vegetationSeed, in NoiseSeed extraSeed1)
        {
            HeightSeed = heightSeed;
            HumiditySeed = humiditySeed;
            VegetationSeed = vegetationSeed;
            ExtraSeed1 = extraSeed1;
        }

        public GeneratorSeed(Random random)
        {
            HeightSeed = new NoiseSeed(random);
            HumiditySeed = new NoiseSeed(random);
            VegetationSeed = new NoiseSeed(random);
            ExtraSeed1 = new NoiseSeed(random);
        }

        public GeneratorSeed(int seed) : this(new Random(seed)) { }

        public static GeneratorSeed Default
        {
            get
            {
                return new GeneratorSeed(
                    new NoiseSeed(new Vector2(52.9258f, 76.3911f), 49164.7641f),
                    new NoiseSeed(new Vector2(66.7943f, 33.1674f), 69761.6413f),
                    new NoiseSeed(new Vector2(37.8254f, 53.2556f), 51338.1952f),
                    new NoiseSeed(new Vector2(61.5137f, 49.4915f), 43913.5758f)
                );
            }
        }
    }

    readonly struct NoiseSeed
    {
        //public static readonly NoiseSeed HeightSeed = new NoiseSeed(new Vector2(52.9258f, 76.3911f), 49164.7641f);
        //public static readonly NoiseSeed HumiditySeed = new NoiseSeed(new Vector2(66.7943f, 33.1674f), 69761.6413f);
        //public static readonly NoiseSeed VegetationSeed = new NoiseSeed(new Vector2(37.8254f, 53.2556f), 51338.1952f);
        //public static readonly NoiseSeed ExtraSeed1 = new NoiseSeed(new Vector2(61.5137f, 49.4915f), 43913.5758f);
        //public static readonly NoiseSeed ExtraSeed2 = new NoiseSeed(new Vector2(42.3791f, 54.3171f), 56123.2499f);
        //public static readonly NoiseSeed ExtraSeed3 = new NoiseSeed(new Vector2(49.2583f, 69.4277f), 57912.5311f);

        public readonly Vector2 DotSeed;
        public readonly float RandMultiplier;

        public NoiseSeed(Vector2 dotSeed, float randMult)
        {
            DotSeed = dotSeed;
            RandMultiplier = randMult;
        }

        public NoiseSeed(Random r)
        {
            DotSeed = new Vector2(r.NextFloat(40, 77), r.NextFloat(40, 77));
            RandMultiplier = r.NextFloat(40000, 70000);
        }
    }
}
