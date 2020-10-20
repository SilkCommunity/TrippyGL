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
        public VertexBuffer<TerrainVertex> WaterBuffer { get; private set; }
        public VertexBuffer<TerrainVertex> UnderwaterBuffer { get; private set; }

        public TerrainChunk(GraphicsDevice graphicsDevice, in TerrainChunkData chunkData)
        {
            GridX = chunkData.GridX;
            GridY = chunkData.GridY;
            if (chunkData.TerrainVertexCount > 0)
                TerrainBuffer = new VertexBuffer<TerrainVertex>(graphicsDevice, chunkData.TerrainMesh, BufferUsageARB.StaticDraw);

            if (chunkData.WaterVertexCount > 0)
                WaterBuffer = new VertexBuffer<TerrainVertex>(graphicsDevice, chunkData.WaterMesh, BufferUsageARB.StaticDraw);

            if (chunkData.UnderwaterVertexCount > 0)
                UnderwaterBuffer = new VertexBuffer<TerrainVertex>(graphicsDevice, chunkData.UnderwaterMesh, BufferUsageARB.StaticDraw);
        }

        public void RenderTerrain()
        {
            if (TerrainBuffer.IsEmpty)
                return;

            GraphicsDevice graphicsDevice = TerrainBuffer.Buffer.GraphicsDevice;
            graphicsDevice.VertexArray = TerrainBuffer;
            graphicsDevice.DrawArrays(PrimitiveType.Triangles, 0, TerrainBuffer.StorageLength);
        }

        public void RenderWater()
        {
            if (WaterBuffer.IsEmpty)
                return;

            GraphicsDevice graphicsDevice = WaterBuffer.Buffer.GraphicsDevice;
            graphicsDevice.VertexArray = WaterBuffer;
            graphicsDevice.DrawArrays(PrimitiveType.Triangles, 0, WaterBuffer.StorageLength);
        }

        public void RenderUnderwater()
        {
            if (UnderwaterBuffer.IsEmpty)
                return;

            GraphicsDevice graphicsDevice = UnderwaterBuffer.Buffer.GraphicsDevice;
            graphicsDevice.VertexArray = UnderwaterBuffer;
            graphicsDevice.DrawArrays(PrimitiveType.Triangles, 0, UnderwaterBuffer.StorageLength);
        }

        public void Dispose()
        {
            if (!TerrainBuffer.IsEmpty) TerrainBuffer.Dispose();
            if (!WaterBuffer.IsEmpty) WaterBuffer.Dispose();
            if (!UnderwaterBuffer.IsEmpty) UnderwaterBuffer.Dispose();
        }

        public override string ToString()
        {
            return "Chunk (" + GridX + ", " + GridY + ")";
        }
    }
}
