using System;
using Silk.NET.OpenGL;
using TrippyGL;

namespace TerrainMaker
{
    class TerrainChunk : IDisposable
    {
        public readonly int GridX;
        public readonly int GridY;

        public VertexBuffer<TerrainVertex> TerrainBuffer { get; private set; }
        public VertexBuffer<TerrainVertex> UnderwaterBuffer { get; private set; }

        public TerrainChunk(GraphicsDevice graphicsDevice, in TerrainChunkData chunkData)
        {
            GridX = chunkData.GridX;
            GridY = chunkData.GridY;
            if (chunkData.TerrainVertexCount > 0)
                TerrainBuffer = new VertexBuffer<TerrainVertex>(graphicsDevice, chunkData.TerrainMesh, BufferUsageARB.StaticDraw);

            if (chunkData.UnderwaterVertexCount > 0)
                UnderwaterBuffer = new VertexBuffer<TerrainVertex>(graphicsDevice, chunkData.UnderwaterMesh, BufferUsageARB.StaticDraw);
        }

        public void Dispose()
        {
            if (!TerrainBuffer.IsEmpty) TerrainBuffer.Dispose();
            if (!UnderwaterBuffer.IsEmpty) UnderwaterBuffer.Dispose();
        }

        public override string ToString()
        {
            return "Chunk (" + GridX + ", " + GridY + ")";
        }
    }
}
