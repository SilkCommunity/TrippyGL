using System;
using TrippyGL;

namespace TerrainMaker
{
    readonly struct TerrainChunkData
    {
        public readonly int GridX;
        public readonly int GridY;

        private readonly TerrainVertex[] terrainMesh;
        private readonly TerrainVertex[] underwaterMesh;

        public readonly int TerrainVertexCount;
        public readonly int UnderwaterVertexCount;

        public ReadOnlySpan<TerrainVertex> TerrainMesh => new ReadOnlySpan<TerrainVertex>(terrainMesh, 0, TerrainVertexCount);
        public ReadOnlySpan<TerrainVertex> UnderwaterMesh => new ReadOnlySpan<TerrainVertex>(underwaterMesh, 0, UnderwaterVertexCount);

        public TerrainChunkData(int gridX, int gridY, TerrainVertex[] terrainMesh, int terrainVertexCount,
            TerrainVertex[] underwaterMesh, int underwaterVertexCount)
        {
            GridX = gridX;
            GridY = gridY;
            this.terrainMesh = terrainMesh;
            this.underwaterMesh = underwaterMesh;
            TerrainVertexCount = terrainVertexCount;
            UnderwaterVertexCount = underwaterVertexCount;
        }

        public void RetrieveBackingArrays(out TerrainVertex[] arr1, out TerrainVertex[] arr2)
        {
            arr1 = terrainMesh;
            arr2 = underwaterMesh;
        }
    }
}
