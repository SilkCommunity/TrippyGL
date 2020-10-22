using System;
using System.Collections.Generic;
using System.Numerics;
using TrippyGL;

namespace TerrainMaker
{
    static class TerrainGenerator
    {
        /// <summary>The length of the side of a chunk (they are squares).</summary>
        public const int ChunkSize = 32;

        /// <summary>The amount of vertices per unit of position.</summary>
        public const int VertexDensity = 1;

        private const int VerticesPerChunk = (ChunkSize * VertexDensity) * (ChunkSize * VertexDensity);

        private static readonly object arraysListLock = new object();
        private static WeakReference<List<VertexNormalColor[]>> arraysList;

        private static PrimitiveBatcher<Vector3> terrTriangles = new PrimitiveBatcher<Vector3>(VerticesPerChunk, 0);
        private static PrimitiveBatcher<Vector3> waterTriangles = new PrimitiveBatcher<Vector3>(VerticesPerChunk, 0);
        private static PrimitiveBatcher<Vector3> underwaterTriangles = new PrimitiveBatcher<Vector3>(VerticesPerChunk, 0);

        private static VertexNormalColor[] GetArray(int requiredLength)
        {
            lock (arraysListLock)
            {
                if (arraysList == null || !arraysList.TryGetTarget(out List<VertexNormalColor[]> list) || list.Count == 0)
                    return new VertexNormalColor[Math.Max(requiredLength, VerticesPerChunk)];

                int indx = -1;
                for (int i = list.Count - 1; i >= 0; i--)
                    if (list[i].Length >= requiredLength && (indx == -1 || list[i].Length < list[indx].Length))
                        indx = i;

                if (indx == -1)
                    return new VertexNormalColor[Math.Max(requiredLength, VerticesPerChunk)];

                VertexNormalColor[] arr = list[indx];
                list.RemoveAt(indx);
                return arr;
            }
        }

        public static void ReturnArrays(in TerrainChunkData chunkData)
        {
            chunkData.RetrieveBackingArrays(out VertexNormalColor[] arr1, out VertexNormalColor[] arr2, out VertexNormalColor[] arr3);

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
                if (arr3 != null) list.Add(arr3);
            }
        }

        private static void SeparateAbove(in Vector3 a, in Vector3 b, in Vector3 c,
            out Vector3 above1, out Vector3 above2, out Vector3 below)
        {
            if (a.Y > 0)
            {
                above1 = a;
                if (b.Y > 0)
                {
                    above2 = b;
                    below = c;
                }
                else
                {
                    above2 = c;
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
                below1 = a;
                if (b.Y < 0)
                {
                    below2 = b;
                    above = c;
                }
                else
                {
                    below2 = c;
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

        private static void SliceTriangle(in Vector3 a, in Vector3 b, in Vector3 c)
        {
            bool aygo = a.Y > 0;
            bool bygo = b.Y > 0;
            bool cygo = c.Y > 0;

            if (aygo && bygo && cygo)
            {
                terrTriangles.AddTriangle(a, b, c);
                return;
            }

            if (!aygo && !bygo && !cygo)
            {
                underwaterTriangles.AddTriangle(a, b, c);
                waterTriangles.AddTriangle(new Vector3(a.X, 0, a.Z), new Vector3(b.X, 0, b.Z), new Vector3(c.X, 0, c.Z));
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

                terrTriangles.AddTriangle(above1, inter1, above2);
                terrTriangles.AddTriangle(inter1, inter2, above2);
                underwaterTriangles.AddTriangle(inter1, below, inter2);

                below.Y = 0;
                inter1.Y = 0;
                inter2.Y = 0;
                waterTriangles.AddTriangle(inter1, below, inter2);
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

                terrTriangles.AddTriangle(above, inter1, inter2);
                underwaterTriangles.AddTriangle(inter1, below1, inter2);
                underwaterTriangles.AddTriangle(inter2, below1, below2);

                below1.Y = 0;
                below2.Y = 0;
                inter1.Y = 0;
                inter2.Y = 0;
                waterTriangles.AddTriangle(inter1, below1, inter2);
                waterTriangles.AddTriangle(inter2, below1, below2);
            }
        }

        public static TerrainChunkData Generate(int gridX, int gridY)
        {
            terrTriangles.ClearTriangles();
            waterTriangles.ClearTriangles();
            underwaterTriangles.ClearTriangles();

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

                    SliceTriangle(pos00, pos10, pos11);
                    SliceTriangle(pos00, pos11, pos01);

                    h11 = h12;
                    h21 = h22;
                }
            }

            VertexNormalColor[] terr = terrTriangles.TriangleVertexCount == 0 ? null : GetArray(terrTriangles.TriangleVertexCount);
            if (terr != null) FormTriangles(terrTriangles.TriangleVertices, terr);

            VertexNormalColor[] water = waterTriangles.TriangleVertexCount == 0 ? null : GetArray(waterTriangles.TriangleVertexCount);
            if (water != null) FormTriangles(waterTriangles.TriangleVertices, water);

            VertexNormalColor[] under = underwaterTriangles.TriangleVertexCount == 0 ? null : GetArray(underwaterTriangles.TriangleVertexCount);
            if (under != null) FormTriangles(underwaterTriangles.TriangleVertices, under);

            return new TerrainChunkData(gridX, gridY, terr, terrTriangles.TriangleVertexCount, water,
                waterTriangles.TriangleVertexCount, under, underwaterTriangles.TriangleVertexCount);
        }

        private static void FormTriangles(ReadOnlySpan<Vector3> source, Span<VertexNormalColor> dest)
        {
            for (int i = 2; i < source.Length; i += 3)
            {
                Vector3 center = (source[i - 2] + source[i - 1] + source[i]) / 3f;
                Vector3 normal = Vector3.Normalize(Vector3.Cross(source[i - 1] - source[i - 2], source[i] - source[i - 2]));

                NoiseGenerator.GenPoint(new Vector2(center.X, center.Z), out float humidity, out float vegetation);

                //Color4b color = FormColor(center.Y, humidity, vegetation);
                float tmp = NoiseGenerator.Perlin(new Vector2(center.X, center.Z) / 16f, GeneratorSeed.HeightSeed, GeneratorSeed.VegetationSeed);
                Color4b color = new Color4b(tmp, tmp, tmp);

                dest[i - 2] = new VertexNormalColor(source[i - 2], normal, color);
                dest[i - 1] = new VertexNormalColor(source[i - 1], normal, color);
                dest[i] = new VertexNormalColor(source[i], normal, color);
            }
        }

        private static Color4b FormColor(float altitude, float humidity, float vegetation)
        {
            return new Color4b(humidity, humidity, vegetation);
        }
    }
}
