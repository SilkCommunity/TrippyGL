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
        float Z;
        Vector4 oneTwoThreeFour;
        int alwaysZero;
        byte padding6;
        short padding7;
        byte alsoZero;
        Matrix4x4 padding8;

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
        }

        public int AttribDescriptionCount => 22;

        public void WriteAttribDescriptions(Span<VertexAttribDescription> descriptions)
        {
            descriptions[0] = VertexAttribDescription.CreatePadding(AttributeType.Float); // padding0
            descriptions[1] = VertexAttribDescription.CreatePadding(VertexAttribPointerType.Short, 1); // padding1
            descriptions[2] = VertexAttribDescription.CreatePadding(VertexAttribPointerType.UnsignedByte, 1); // padding2
            descriptions[3] = new VertexAttribDescription(AttributeType.Float, false, VertexAttribPointerType.Byte); // sixtyThree
            descriptions[4] = new VertexAttribDescription(AttributeType.Float); // X
            descriptions[5] = VertexAttribDescription.CreatePadding(VertexAttribPointerType.Short, 1); // padding3
            descriptions[6] = new VertexAttribDescription(AttributeType.Float, true, VertexAttribPointerType.UnsignedByte); // nothing0
            descriptions[7] = new VertexAttribDescription(AttributeType.Float, true, VertexAttribPointerType.UnsignedByte); //colorR
            descriptions[8] = new VertexAttribDescription(AttributeType.FloatMat4); // mat1
            descriptions[9] = new VertexAttribDescription(AttributeType.Float, false, VertexAttribPointerType.UnsignedShort); // colorG
            descriptions[10] = new VertexAttribDescription(AttributeType.Float, false, VertexAttribPointerType.Short); // sixtyFour
            descriptions[11] = new VertexAttribDescription(AttributeType.Float); // Y
            descriptions[12] = VertexAttribDescription.CreatePadding(VertexAttribPointerType.Short, 1); // padding4
            descriptions[13] = new VertexAttribDescription(AttributeType.Float, true, VertexAttribPointerType.UnsignedByte); // colorB
            descriptions[14] = VertexAttribDescription.CreatePadding(AttributeType.FloatMat3x2); // padding5
            descriptions[15] = new VertexAttribDescription(AttributeType.Float); // Z
            descriptions[16] = new VertexAttribDescription(AttributeType.FloatVec4); // oneTwoThreeFour
            descriptions[17] = new VertexAttribDescription(AttributeType.Int); // alwaysZero
            descriptions[18] = VertexAttribDescription.CreatePadding(VertexAttribPointerType.UnsignedByte, 1); // padding6
            descriptions[19] = VertexAttribDescription.CreatePadding(VertexAttribPointerType.Short, 1); // padding7
            descriptions[20] = new VertexAttribDescription(AttributeType.Float, true, VertexAttribPointerType.UnsignedByte); // alsoZero
            descriptions[21] = VertexAttribDescription.CreatePadding(AttributeType.FloatMat4); // padding8
        }
    }
}
