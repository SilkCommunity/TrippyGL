using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;
using TrippyGL;

namespace ComplexVertexFormats
{
    [StructLayout(LayoutKind.Sequential)]
    struct ComplexVertex : IVertex
    {
        float padding0;
        short padding1;
        byte padding2;
        sbyte sixtyThree;
        float X;
        short padding3;
        byte nothing0;
        byte colorR;
        Matrix4x4 matrix1;
        ushort colorG;
        short sixtyFour;
        float Y;
        short padding4;
        byte colorB;
        Matrix3x2 padding5;
        byte padding6;
        float Z;
        Vector4 oneTwoThreeFour;
        int alwaysZero;
        byte padding7;
        short padding8;
        byte alsoZero;
        Matrix4x4 padding9;
        byte padding10;

        public ComplexVertex(Vector3 position, Color4b color)
        {
            X = position.X;
            Y = position.Y;
            Z = position.Z;

            colorR = color.R;
            colorG = (ushort)(color.G * 2048 / 255);
            colorB = color.B;

            matrix1 = Matrix4x4.Identity;

            sixtyThree = 63;
            sixtyFour = 64;
            oneTwoThreeFour = new Vector4(1, 2, 3, 4);
            alwaysZero = 0;
            alsoZero = 0;

            nothing0 = default;

            padding0 = default;
            padding1 = default;
            padding2 = default;
            padding3 = default;
            padding4 = default;
            padding5 = default;
            padding6 = default;
            padding7 = default;
            padding8 = default;
            padding9 = default;
            padding10 = default;
        }

        public int AttribDescriptionCount => 20;

        public void WriteAttribDescriptions(Span<VertexAttribDescription> descriptions)
        {
            descriptions[0] = new VertexAttribDescription(7); // padding0-2
            descriptions[1] = new VertexAttribDescription(AttributeType.Float, false, VertexAttribPointerType.Byte); // sixtyThree
            descriptions[2] = new VertexAttribDescription(AttributeType.Float); // X
            descriptions[3] = VertexAttribDescription.CreatePadding(VertexAttribPointerType.Short, 1); // padding3
            descriptions[4] = new VertexAttribDescription(AttributeType.Float, true, VertexAttribPointerType.UnsignedByte); // nothing0
            descriptions[5] = new VertexAttribDescription(AttributeType.Float, true, VertexAttribPointerType.UnsignedByte); //colorR
            descriptions[6] = new VertexAttribDescription(AttributeType.FloatMat4); // mat1
            descriptions[7] = new VertexAttribDescription(AttributeType.Float, false, VertexAttribPointerType.UnsignedShort); // colorG
            descriptions[8] = new VertexAttribDescription(AttributeType.Float, false, VertexAttribPointerType.Short); // sixtyFour
            descriptions[9] = new VertexAttribDescription(AttributeType.Float); // Y
            descriptions[10] = VertexAttribDescription.CreatePadding(VertexAttribPointerType.Short, 1); // padding4
            descriptions[11] = new VertexAttribDescription(AttributeType.Float, true, VertexAttribPointerType.UnsignedByte); // colorB
            descriptions[12] = new VertexAttribDescription(3 * 2 * 4 + 2); // padding 5-6
            descriptions[13] = new VertexAttribDescription(AttributeType.Float); // Z
            descriptions[14] = new VertexAttribDescription(AttributeType.FloatVec4); // oneTwoThreeFour
            descriptions[15] = new VertexAttribDescription(AttributeType.Int); // alwaysZero
            descriptions[16] = VertexAttribDescription.CreatePadding(VertexAttribPointerType.UnsignedByte, 1); // padding7
            descriptions[17] = VertexAttribDescription.CreatePadding(VertexAttribPointerType.Short, 1); // padding8
            descriptions[18] = new VertexAttribDescription(AttributeType.Float, true, VertexAttribPointerType.UnsignedByte); // alsoZero
            descriptions[19] = new VertexAttribDescription(65); // padding9-10
        }
    }
}
