using System;
using System.Numerics;
using System.Runtime.InteropServices;
using TrippyGL;

namespace TerrainMaker
{
    [StructLayout(LayoutKind.Sequential)]
    struct TerrainVertex : IVertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Color4b Color;
        public Vector2 LightingConfig;

        public TerrainVertex(Vector3 position, Vector3 normal, Color4b color, Vector2 lightingConfig)
        {
            Position = position;
            Normal = normal;
            Color = color;
            LightingConfig = lightingConfig;
        }

        public int AttribDescriptionCount => 4;

        public void WriteAttribDescriptions(Span<VertexAttribDescription> descriptions)
        {
            descriptions[0] = new VertexAttribDescription(AttributeType.FloatVec3);
            descriptions[1] = new VertexAttribDescription(AttributeType.FloatVec3);
            descriptions[2] = new VertexAttribDescription(AttributeType.FloatVec4, true, AttributeBaseType.UnsignedByte);
            descriptions[3] = new VertexAttribDescription(AttributeType.FloatVec2);
        }
    }
}
