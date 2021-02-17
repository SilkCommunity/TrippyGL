using System;
using System.Runtime.InteropServices;
using TrippyGL;

namespace IndexedRendering
{
    /// <summary>
    /// A simple vertex type with only one vertex attribute, a float vec2.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct SimpleVertex : IVertex
    {
        public float X, Y;

        public SimpleVertex(float x, float y)
        {
            X = x;
            Y = y;
        }

        public int AttribDescriptionCount => 1;

        public void WriteAttribDescriptions(Span<VertexAttribDescription> descriptions)
        {
            descriptions[0] = new VertexAttribDescription(AttributeType.FloatVec2);
        }
    }
}
