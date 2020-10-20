using System;
using System.Collections.Generic;
using System.Numerics;
using TrippyGL;

namespace TerrainMaker
{
    static class TerrainGenerator
    {
        /// <summary>The length of the side of a chunk (they are squares).</summary>
        public const int ChunkSize = 16;

        /// <summary>The amount of vertices per unit of position.</summary>
        public const int VertexDensity = 8;

        private const int VerticesPerChunk = (ChunkSize * VertexDensity) * (ChunkSize * VertexDensity);

        private static WeakReference<List<TerrainVertex[]>> arraysList;

        private static PrimitiveBatcher<TerrainVertex> terrTriangles = new PrimitiveBatcher<TerrainVertex>(VerticesPerChunk, 0);
        private static PrimitiveBatcher<TerrainVertex> waterTriangles = new PrimitiveBatcher<TerrainVertex>(VerticesPerChunk, 0);
        private static PrimitiveBatcher<TerrainVertex> underwaterTriangles = new PrimitiveBatcher<TerrainVertex>(VerticesPerChunk, 0);
        private static float[,] heights = new float[ChunkSize * VertexDensity + 2, ChunkSize * VertexDensity + 2];
        private static float[,] humidities = new float[ChunkSize * VertexDensity + 2, ChunkSize * VertexDensity + 2];

        private static TerrainVertex[] GetArray(int requiredLength)
        {
            if (arraysList == null || !arraysList.TryGetTarget(out List<TerrainVertex[]> list) || list.Count == 0)
                return new TerrainVertex[Math.Max(requiredLength, VerticesPerChunk)];

            int indx = -1;
            for (int i = list.Count - 1; i >= 0; i--)
                if (list[i].Length >= requiredLength && (indx == -1 || list[i].Length < list[indx].Length))
                    indx = i;

            if (indx == -1)
                return new TerrainVertex[Math.Max(requiredLength, VerticesPerChunk)];

            TerrainVertex[] arr = list[indx];
            list.RemoveAt(indx);
            return arr;
        }

        public static void ReturnArrays(in TerrainChunkData chunkData)
        {
            chunkData.RetrieveBackingArrays(out TerrainVertex[] arr1, out TerrainVertex[] arr2, out TerrainVertex[] arr3);

            List<TerrainVertex[]> list;
            if (arraysList == null)
            {
                list = new List<TerrainVertex[]>();
                arraysList = new WeakReference<List<TerrainVertex[]>>(list);
            }
            else if (!arraysList.TryGetTarget(out list))
            {
                list = new List<TerrainVertex[]>();
                arraysList.SetTarget(list);
            }

            if (arr1 != null) list.Add(arr1);
            if (arr2 != null) list.Add(arr2);
            if (arr3 != null) list.Add(arr3);
        }

        private static void SeparateAbove(in TerrainVertex a, in TerrainVertex b, in TerrainVertex c,
            out TerrainVertex above1, out TerrainVertex above2, out TerrainVertex below)
        {
            if (a.Position.Y > 0)
            {
                above1 = a;
                if (b.Position.Y > 0)
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

        private static void SeparateBelow(in TerrainVertex a, in TerrainVertex b, in TerrainVertex c,
            out TerrainVertex below1, out TerrainVertex below2, out TerrainVertex above)
        {
            if (a.Position.Y < 0)
            {
                below1 = a;
                if (b.Position.Y < 0)
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

        private static void SliceTriangle(in TerrainVertex a, in TerrainVertex b, in TerrainVertex c)
        {
            bool aygo = a.Position.Y > 0;
            bool bygo = b.Position.Y > 0;
            bool cygo = c.Position.Y > 0;

            if (aygo && bygo && cygo)
            {
                terrTriangles.AddTriangle(a, b, c);
                return;
            }

            if (!aygo && !bygo && !cygo)
            {
                underwaterTriangles.AddTriangle(a, b, c);
                TerrainVertex av = a;
                av.Position.Y = 0;
                TerrainVertex bv = b;
                bv.Position.Y = 0;
                TerrainVertex cv = c;
                cv.Position.Y = 0;
                waterTriangles.AddTriangle(av, bv, cv);
                return;
            }

            int aboveWaterCount = (aygo ? 1 : 0) + (bygo ? 1 : 0) + (cygo ? 1 : 0);

            if (aboveWaterCount == 2)
            {
                SeparateAbove(a, b, c, out TerrainVertex above1, out TerrainVertex above2, out TerrainVertex below);
                // 0 = (1-t)*AY + t*BY
                // 0 = AY - t*AY + t*BY
                // -AY = t*(BY-AY)
                // t = -AY/(BY-AY)
                // t = AY / (AY - BY)
                float t = above1.Position.Y / (above1.Position.Y - below.Position.Y);
                TerrainVertex inter1 = TerrainVertex.Lerp(above1, below, t);
                inter1.Position.Y = 0;

                t = above2.Position.Y / (above2.Position.Y - below.Position.Y);
                TerrainVertex inter2 = TerrainVertex.Lerp(above2, below, t);
                inter2.Position.Y = 0;

                terrTriangles.AddTriangle(above1, inter1, above2);
                terrTriangles.AddTriangle(inter1, inter2, above2);
                underwaterTriangles.AddTriangle(inter1, below, inter2);

                below.Position.Y = 0;
                inter1.Position.Y = 0;
                inter2.Position.Y = 0;
                waterTriangles.AddTriangle(inter1, below, inter2);
            }
            else // if(aboveWaterCount == 1)
            {
                SeparateBelow(a, b, c, out TerrainVertex below1, out TerrainVertex below2, out TerrainVertex above);
                float t = below1.Position.Y / (below1.Position.Y - above.Position.Y);
                TerrainVertex inter1 = TerrainVertex.Lerp(below1, above, t);
                inter1.Position.Y = 0;

                t = below2.Position.Y / (below2.Position.Y - above.Position.Y);
                TerrainVertex inter2 = TerrainVertex.Lerp(below2, above, t);
                inter2.Position.Y = 0;

                terrTriangles.AddTriangle(above, inter1, inter2);
                underwaterTriangles.AddTriangle(inter1, below1, inter2);
                underwaterTriangles.AddTriangle(inter2, below1, below2);

                below1.Position.Y = 0;
                below2.Position.Y = 0;
                inter1.Position.Y = 0;
                inter2.Position.Y = 0;
                waterTriangles.AddTriangle(inter1, below1, inter2);
                waterTriangles.AddTriangle(inter2, below1, below2);
            }
        }

        public static TerrainChunkData Generate(int gridX, int gridY)
        {
            terrTriangles.ClearTriangles();
            waterTriangles.ClearTriangles();
            underwaterTriangles.ClearTriangles();

            int firstX = gridX * ChunkSize;
            int firstY = gridY * ChunkSize;

            for (int hx = 0; hx < heights.GetLength(0); hx++)
                for (int hy = 0; hy < heights.GetLength(1); hy++)
                {
                    Vector2 position = new Vector2(hx - 1, hy - 1) / VertexDensity + new Vector2(firstX, firstY);
                    heights[hx, hy] = NoiseGenerator.GenHeight(position);
                    humidities[hx, hy] = NoiseGenerator.GenHumidity(position);
                }


            const float delta = 1f / VertexDensity;
            for (int cx = 0; cx < ChunkSize * VertexDensity; cx++)
                for (int cy = 0; cy < ChunkSize * VertexDensity; cy++)
                {
                    Vector3 pos00 = new Vector3(firstX + cx * delta, heights[cx + 1, cy + 1], firstY + cy * delta);
                    Vector3 pos10 = new Vector3(firstX + cx * delta + delta, heights[cx + 2, cy + 1], firstY + cy * delta);
                    Vector3 pos11 = new Vector3(firstX + cx * delta + delta, heights[cx + 2, cy + 2], firstY + cy * delta + delta);
                    Vector3 pos01 = new Vector3(firstX + cx * delta, heights[cx + 1, cy + 2], firstY + cy * delta + delta);

                    TerrainVertex v00 = new TerrainVertex(pos00, Vector3.Zero, humidities[cx + 1, cy + 1], 0);
                    TerrainVertex v10 = new TerrainVertex(pos10, Vector3.Zero, humidities[cx + 2, cy + 1], 0);
                    TerrainVertex v11 = new TerrainVertex(pos11, Vector3.Zero, humidities[cx + 2, cy + 2], 0);
                    TerrainVertex v01 = new TerrainVertex(pos01, Vector3.Zero, humidities[cx + 1, cy + 2], 0);

                    SliceTriangle(v00, v10, v11);
                    SliceTriangle(v00, v11, v01);
                }

            TerrainVertex[] terr = terrTriangles.TriangleVertexCount == 0 ? null : GetArray(terrTriangles.TriangleVertexCount);
            if (terr != null)
                terrTriangles.TriangleVertices.CopyTo(terr);

            TerrainVertex[] water = waterTriangles.TriangleVertexCount == 0 ? null : GetArray(waterTriangles.TriangleVertexCount);
            if (water != null)
                waterTriangles.TriangleVertices.CopyTo(water);

            TerrainVertex[] under = underwaterTriangles.TriangleVertexCount == 0 ? null : GetArray(underwaterTriangles.TriangleVertexCount);
            if (under != null)
                underwaterTriangles.TriangleVertices.CopyTo(under);

            return new TerrainChunkData(gridX, gridY, terr, terrTriangles.TriangleVertexCount, water, waterTriangles.TriangleVertexCount,
                under, underwaterTriangles.TriangleVertexCount);
        }
    }
}
