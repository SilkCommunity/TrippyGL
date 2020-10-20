using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;
using TrippyGL;

namespace TerrainMaker
{
    [StructLayout(LayoutKind.Sequential)]
    struct TerrainVertex : IVertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public float Humidity;
        public float Vegetation;

        public TerrainVertex(Vector3 position, Vector3 normal, float humidity, float vegetation)
        {
            Position = position;
            Normal = normal;
            Humidity = humidity;
            Vegetation = vegetation;
        }

        public int AttribDescriptionCount => 5;

        public void WriteAttribDescriptions(Span<VertexAttribDescription> descriptions)
        {
            descriptions[0] = new VertexAttribDescription(AttributeType.FloatVec3);
            descriptions[1] = new VertexAttribDescription(AttributeType.FloatVec3);
            descriptions[2] = new VertexAttribDescription(AttributeType.Float);
            descriptions[4] = new VertexAttribDescription(AttributeType.Float);
        }

        public override string ToString()
        {
            return "(" + Position.X + ", " + Position.Y + ", " + Position.Z
                + ") (" + Normal.X + ", " + Normal.Y + ", " + Normal.Z
                + ") (" + Humidity + ", " + Vegetation + ")";
        }

        public static TerrainVertex Lerp(in TerrainVertex min, in TerrainVertex max, float amount)
        {
            return new TerrainVertex(
                Vector3.Lerp(min.Position, max.Position, amount),
                Vector3.Lerp(min.Normal, max.Normal, amount),
                (1 - amount) * min.Humidity + amount * max.Humidity,
                (1 - amount) * min.Vegetation + amount * max.Vegetation
            );
        }
    }
}
