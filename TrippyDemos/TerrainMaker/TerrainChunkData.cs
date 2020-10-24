using System;
using TrippyGL;

namespace TerrainMaker
{
    readonly struct TerrainChunkData
    {
        public readonly int GridX;
        public readonly int GridY;

        private readonly VertexNormalColor[] terrainMesh;
        private readonly VertexNormalColor[] underwaterMesh;

        public readonly int TerrainVertexCount;
        public readonly int UnderwaterVertexCount;

        public ReadOnlySpan<VertexNormalColor> TerrainMesh => new ReadOnlySpan<VertexNormalColor>(terrainMesh, 0, TerrainVertexCount);
        public ReadOnlySpan<VertexNormalColor> UnderwaterMesh => new ReadOnlySpan<VertexNormalColor>(underwaterMesh, 0, UnderwaterVertexCount);

        public TerrainChunkData(int gridX, int gridY, VertexNormalColor[] terrainMesh, int terrainVertexCount,
            VertexNormalColor[] underwaterMesh, int underwaterVertexCount)
        {
            GridX = gridX;
            GridY = gridY;
            this.terrainMesh = terrainMesh;
            this.underwaterMesh = underwaterMesh;
            TerrainVertexCount = terrainVertexCount;
            UnderwaterVertexCount = underwaterVertexCount;
        }

        public void RetrieveBackingArrays(out VertexNormalColor[] arr1, out VertexNormalColor[] arr2)
        {
            arr1 = terrainMesh;
            arr2 = underwaterMesh;
        }
    }
}
