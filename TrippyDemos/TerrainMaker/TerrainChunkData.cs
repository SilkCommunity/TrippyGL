using System;

namespace TerrainMaker
{
    readonly struct TerrainChunkData
    {
        public readonly int GridX;
        public readonly int GridY;

        private readonly TerrainVertex[] terrainMesh;
        private readonly TerrainVertex[] waterMesh;
        private readonly TerrainVertex[] underwaterMesh;

        public readonly int TerrainVertexCount;
        public readonly int WaterVertexCount;
        public readonly int UnderwaterVertexCount;

        public ReadOnlySpan<TerrainVertex> TerrainMesh => new ReadOnlySpan<TerrainVertex>(terrainMesh, 0, TerrainVertexCount);
        public ReadOnlySpan<TerrainVertex> WaterMesh => new ReadOnlySpan<TerrainVertex>(waterMesh, 0, WaterVertexCount);
        public ReadOnlySpan<TerrainVertex> UnderwaterMesh => new ReadOnlySpan<TerrainVertex>(underwaterMesh, 0, UnderwaterVertexCount);

        public TerrainChunkData(int gridX, int gridY, TerrainVertex[] terrainMesh, int terrainVertexCount,
            TerrainVertex[] waterMesh, int waterVertexCount, TerrainVertex[] underwaterMesh, int underwaterVertexCount)
        {
            GridX = gridX;
            GridY = gridY;
            this.terrainMesh = terrainMesh;
            this.waterMesh = waterMesh;
            this.underwaterMesh = underwaterMesh;
            TerrainVertexCount = terrainVertexCount;
            WaterVertexCount = waterVertexCount;
            UnderwaterVertexCount = underwaterVertexCount;
        }

        public void RetrieveBackingArrays(out TerrainVertex[] arr1, out TerrainVertex[] arr2, out TerrainVertex[] arr3)
        {
            arr1 = terrainMesh;
            arr2 = waterMesh;
            arr3 = underwaterMesh;
        }
    }
}
