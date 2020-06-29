using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Silk.NET.OpenGL;
using TrippyGL;

namespace IndexedRendering
{
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
