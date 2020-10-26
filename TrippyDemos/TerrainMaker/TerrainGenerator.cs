using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
using TrippyGL;
using TrippyGL.Utils;

namespace TerrainMaker
{
    static class TerrainGenerator
    {
        /// <summary>The length of the side of a chunk (they are squares).</summary>
        public const int ChunkSize = 32;

        /// <summary>The amount of vertices per unit of position.</summary>
        public const int VertexDensity = 1;

        private const int InitialArrayCapacity = (ChunkSize * VertexDensity) * (ChunkSize * VertexDensity);

        private static readonly Vector3 DrySand = new Vector3(0.94f, 0.87f, 0.56f);
        private static readonly Vector3 WetSand = new Vector3(0.7f, 0.6f, 0.4f);
        private static readonly Vector3 WetterSand = new Vector3(0.6f, 0.5f, 0.35f);
        private static readonly Vector3 DryGrass = new Vector3(0.1f, 0.7f, 0.0f);
        private static readonly Vector3 WetGrass = new Vector3(0.05f, 0.4f, 0.0f);
        private static readonly Vector3 Rock = new Vector3(0.5f, 0.55f, 0.52f);
        private static readonly Vector3 OceanRock = new Vector3(0.29f, 0.31f, 0.27f);
        private static readonly Vector3 Snow = new Vector3(0.84f, 0.9f, 1.0f);

        private static readonly object arraysListLock = new object();
        private static WeakReference<List<VertexNormalColor[]>> arraysList;

        private static readonly object batchersLock = new object();
        private static WeakReference<Stack<PrimitiveBatcher<Vector3>>> batchers;

        private static VertexNormalColor[] GetArray(int requiredLength)
        {
            lock (arraysListLock)
            {
                if (arraysList == null || !arraysList.TryGetTarget(out List<VertexNormalColor[]> list) || list.Count == 0)
                    return new VertexNormalColor[Math.Max(requiredLength, InitialArrayCapacity)];

                int indx = -1;
                for (int i = list.Count - 1; i >= 0; i--)
                    if (list[i].Length >= requiredLength && (indx == -1 || list[i].Length < list[indx].Length))
                        indx = i;

                if (indx == -1)
                    return new VertexNormalColor[Math.Max(requiredLength, InitialArrayCapacity)];

                VertexNormalColor[] arr = list[indx];
                list.RemoveAt(indx);
                return arr;
            }
        }

        public static void ReturnArrays(in TerrainChunkData chunkData)
        {
            chunkData.RetrieveBackingArrays(out VertexNormalColor[] arr1, out VertexNormalColor[] arr2);

            if (arr1 == null && arr2 == null)
                return;

            lock (arraysListLock)
            {
                List<VertexNormalColor[]> list;
                if (arraysList == null)
                {
                    list = new List<VertexNormalColor[]>();
                    arraysList = new WeakReference<List<VertexNormalColor[]>>(list);
                }
                else if (!arraysList.TryGetTarget(out list))
                {
                    list = new List<VertexNormalColor[]>();
                    arraysList.SetTarget(list);
                }

                if (arr1 != null) list.Add(arr1);
                if (arr2 != null) list.Add(arr2);
            }
        }

        private static void GetBatchers(out PrimitiveBatcher<Vector3> a, out PrimitiveBatcher<Vector3> b, out PrimitiveBatcher<Vector3> c)
        {
            lock (batchersLock)
            {
                if (batchers == null || !batchers.TryGetTarget(out Stack<PrimitiveBatcher<Vector3>> list) || list.Count == 0)
                {
                    a = new PrimitiveBatcher<Vector3>(InitialArrayCapacity, 0);
                    b = new PrimitiveBatcher<Vector3>(InitialArrayCapacity, 0);
                    c = new PrimitiveBatcher<Vector3>(InitialArrayCapacity, 0);
                    return;
                }

                list.TryPop(out a);

                if (list.TryPop(out b))
                {
                    if (!list.TryPop(out c))
                        c = new PrimitiveBatcher<Vector3>(InitialArrayCapacity, 0);
                }
                else
                {
                    b = new PrimitiveBatcher<Vector3>(InitialArrayCapacity, 0);
                    c = new PrimitiveBatcher<Vector3>(InitialArrayCapacity, 0);
                }
            }
        }

        private static void ReturnBatchers(PrimitiveBatcher<Vector3> a, PrimitiveBatcher<Vector3> b, PrimitiveBatcher<Vector3> c)
        {
            lock (batchersLock)
            {
                Stack<PrimitiveBatcher<Vector3>> stack;

                if (batchers == null)
                {
                    stack = new Stack<PrimitiveBatcher<Vector3>>();
                    batchers = new WeakReference<Stack<PrimitiveBatcher<Vector3>>>(stack);
                }
                else if (!batchers.TryGetTarget(out stack))
                {
                    stack = new Stack<PrimitiveBatcher<Vector3>>();
                    batchers.SetTarget(stack);
                }

                if (a != null) stack.Push(a);
                if (b != null) stack.Push(b);
                if (c != null) stack.Push(c);
            }
        }

        private static void SeparateAbove(in Vector3 a, in Vector3 b, in Vector3 c,
            out Vector3 above1, out Vector3 above2, out Vector3 below)
        {
            if (a.Y > 0)
            {
                if (b.Y > 0)
                {
                    above1 = a;
                    above2 = b;
                    below = c;
                }
                else
                {
                    above1 = c;
                    above2 = a;
                    below = b;
                }
            }
            else
            {
                below = a;
                above1 = b;
                above2 = c;
            }
        }

        private static void SeparateBelow(in Vector3 a, in Vector3 b, in Vector3 c,
            out Vector3 below1, out Vector3 below2, out Vector3 above)
        {
            if (a.Y < 0)
            {
                if (b.Y < 0)
                {
                    below1 = a;
                    below2 = b;
                    above = c;
                }
                else
                {
                    below1 = c;
                    below2 = a;
                    above = b;
                }
            }
            else
            {
                above = a;
                below1 = b;
                below2 = c;
            }
        }

        private static void SliceTriangle(in Vector3 a, in Vector3 b, in Vector3 c, PrimitiveBatcher<Vector3> terrTri,
            PrimitiveBatcher<Vector3> waterTri, PrimitiveBatcher<Vector3> underTri)
        {
            bool aygo = a.Y > 0;
            bool bygo = b.Y > 0;
            bool cygo = c.Y > 0;

            if (aygo && bygo && cygo)
            {
                terrTri.AddTriangle(a, b, c);
                return;
            }

            if (!aygo && !bygo && !cygo)
            {
                underTri.AddTriangle(a, b, c);
                waterTri.AddTriangle(new Vector3(a.X, 0, a.Z), new Vector3(b.X, 0, b.Z), new Vector3(c.X, 0, c.Z));
                return;
            }

            int aboveWaterCount = (aygo ? 1 : 0) + (bygo ? 1 : 0) + (cygo ? 1 : 0);

            if (aboveWaterCount == 2)
            {
                SeparateAbove(a, b, c, out Vector3 above1, out Vector3 above2, out Vector3 below);
                float t = above1.Y / (above1.Y - below.Y);
                Vector3 inter1 = Vector3.Lerp(above1, below, t);
                inter1.Y = 0;

                t = above2.Y / (above2.Y - below.Y);
                Vector3 inter2 = Vector3.Lerp(above2, below, t);
                inter2.Y = 0;

                terrTri.AddTriangle(above1, above2, inter1);
                terrTri.AddTriangle(inter1, above2, inter2);
                underTri.AddTriangle(inter1, inter2, below);

                below.Y = 0;
                inter1.Y = 0;
                inter2.Y = 0;
                waterTri.AddTriangle(inter1, inter2, below);
            }
            else // if(aboveWaterCount == 1)
            {
                SeparateBelow(a, b, c, out Vector3 below1, out Vector3 below2, out Vector3 above);
                float t = below1.Y / (below1.Y - above.Y);
                Vector3 inter1 = Vector3.Lerp(below1, above, t);
                inter1.Y = 0;

                t = below2.Y / (below2.Y - above.Y);
                Vector3 inter2 = Vector3.Lerp(below2, above, t);
                inter2.Y = 0;

                terrTri.AddTriangle(above, inter1, inter2);
                underTri.AddTriangle(inter1, below1, inter2);
                underTri.AddTriangle(inter2, below1, below2);

                below1.Y = 0;
                below2.Y = 0;
                inter1.Y = 0;
                inter2.Y = 0;
                waterTri.AddTriangle(inter1, below1, inter2);
                waterTri.AddTriangle(inter2, below1, below2);
            }
        }

        public static TerrainChunkData Generate(int gridX, int gridY)
        {
            GetBatchers(out PrimitiveBatcher<Vector3> terrTri, out PrimitiveBatcher<Vector3> waterTri, out PrimitiveBatcher<Vector3> underTri);

            try
            {
                terrTri.ClearTriangles();
                waterTri.ClearTriangles();
                underTri.ClearTriangles();

                Vector2 firstCoord = new Vector2(gridX, gridY) * ChunkSize;

                float h11, h21, h22, h12;
                for (int cx = 0; cx < ChunkSize * VertexDensity; cx++)
                {
                    h11 = NoiseGenerator.GenHeight(new Vector2(cx, 0) / VertexDensity + firstCoord);
                    h21 = NoiseGenerator.GenHeight(new Vector2(cx + 1, 0) / VertexDensity + firstCoord);

                    for (int cy = 0; cy < ChunkSize * VertexDensity; cy++)
                    {
                        h12 = NoiseGenerator.GenHeight(new Vector2(cx, cy + 1) / VertexDensity + firstCoord);
                        h22 = NoiseGenerator.GenHeight(new Vector2(cx + 1, cy + 1) / VertexDensity + firstCoord);

                        Vector3 pos00 = new Vector3(firstCoord.X + cx / VertexDensity, h11, firstCoord.Y + cy / VertexDensity);
                        Vector3 pos10 = new Vector3(firstCoord.X + (cx + 1) / VertexDensity, h21, firstCoord.Y + cy / VertexDensity);
                        Vector3 pos11 = new Vector3(firstCoord.X + (cx + 1) / VertexDensity, h22, firstCoord.Y + (cy + 1) / VertexDensity);
                        Vector3 pos01 = new Vector3(firstCoord.X + cx / VertexDensity, h12, firstCoord.Y + (cy + 1) / VertexDensity);

                        SliceTriangle(pos00, pos11, pos10, terrTri, waterTri, underTri);
                        SliceTriangle(pos00, pos01, pos11, terrTri, waterTri, underTri);

                        h11 = h12;
                        h21 = h22;
                    }
                }

                VertexNormalColor[] terr = terrTri.TriangleVertexCount == 0 ? null : GetArray(terrTri.TriangleVertexCount);
                if (terr != null) FormTriangles(terrTri.TriangleVertices, terr);

                VertexNormalColor[] under = underTri.TriangleVertexCount == 0 ? null : GetArray(underTri.TriangleVertexCount);
                if (under != null) FormTriangles(underTri.TriangleVertices, under);

                return new TerrainChunkData(gridX, gridY, terr, terrTri.TriangleVertexCount, under, underTri.TriangleVertexCount);
            }
            finally
            {
                ReturnBatchers(terrTri, waterTri, underTri);
            }
        }

        private static void FormTriangles(ReadOnlySpan<Vector3> source, Span<VertexNormalColor> dest)
        {
            for (int i = 2; i < source.Length; i += 3)
            {
                Vector3 center = (source[i - 2] + source[i - 1] + source[i]) / 3f;
                Vector3 normal = Vector3.Normalize(Vector3.Cross(source[i - 1] - source[i - 2], source[i] - source[i - 2]));

                NoiseGenerator.GenPoint(new Vector2(center.X, center.Z), out float humidity, out float vegetation);

                Color4b color = FormColor(center.Y, normal, humidity, vegetation);
                //float tmp = NoiseGenerator.Perlin(new Vector2(center.X, center.Z) / 16f, GeneratorSeed.HeightSeed, GeneratorSeed.VegetationSeed);
                //Color4b color = new Color4b(tmp, tmp, tmp);
                if (normal.Y < 0)
                    color.R = 255;

                dest[i - 2] = new VertexNormalColor(source[i - 2], normal, color);
                dest[i - 1] = new VertexNormalColor(source[i - 1], normal, color);
                dest[i] = new VertexNormalColor(source[i], normal, color);
            }
        }

        private static Color4b FormColor(float altitude, in Vector3 normal, float humidity, float vegetation)
        {
            Vector3 color;
            if (altitude < 0)
            {
                float sand = Math.Min(normal.Y, 1.2f) * (vegetation + 0.2f);
                sand = TrippyMath.LerpPrecise(1, sand, MathF.Min(altitude / -5f, 1));
                sand = MathF.Tanh(10 * sand - 5) * 0.5f + 0.5f;
                color = Vector3.Lerp(OceanRock, WetterSand, sand);
                color *= 1 + (humidity - 0.5f) * 0.7f;
                color *= 1 - Math.Clamp((altitude + 10) / -30f, 0, 0.8f);
            }
            else if (altitude < 9)
            { // sand (above-water)
                float m = altitude / 9f;
                color = Vector3.Lerp(WetSand, DrySand, m);
            }
            else
            {
                float grassTop = 35 + vegetation * 15f - humidity * 7f;
                if (altitude >= 9 && altitude < grassTop)
                { // grass
                    float m = (altitude - 9) / (grassTop - 9) + vegetation * 0.5f;
                    color = Vector3.Lerp(DryGrass, WetGrass, (int)MathF.Round(humidity * 6f + m) / 6f);
                }
                else
                {
                    float m = (altitude - grassTop) / 30;
                    float snow = (humidity * 0.7f + 0.2f) * (1 - MathF.Exp((50 - altitude) * 0.1f)) * Math.Min(normal.Y + 0.7f, 1.4f);
                    if (snow > 0.7f)
                    { // snow
                        color = Vector3.Min(Snow * (snow + 0.2f), Vector3.One);
                    }
                    else
                    { // mountain rock
                        float h = Math.Clamp((int)MathF.Round((m * 0.3f + 0.6f + (vegetation + humidity - 1) * 0.5f) * 6) / 6f, 0, 1.2f);
                        color = Rock * h;
                    }
                }
            }

            return new Color4b(color);
            //return new Color4b(humidity, 0f, vegetation);
        }
    }
}
