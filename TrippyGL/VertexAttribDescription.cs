using System;
using System.Text;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    /// <summary>
    /// Describes a vertex attribute. This is, both how it is declared in the shader and how it will be read from a buffer
    /// </summary>
    public struct VertexAttribDescription
    {
        /// <summary>The size of the attribute. A float or int would be 1, a vec2 would be 2, a vec3i would be 3, etc</summary>
        public readonly int Size;

        /// <summary>The base type of the attribute</summary>
        public readonly VertexAttribPointerType AttribBaseType;

        /// <summary>The size in bytes of the attribute. A float is 4, a ivec2 is 8, a vec4 is 16, a double is 8, a mat3 is 36, etc</summary>
        public readonly int SizeInBytes;

        /// <summary>Whether the attrib data should be normalized when loaded into shaders</summary>
        public readonly bool Normalized;

        /// <summary>The amount of attribute indices this specific attribute occupies. Usually 1, but float matrices for example use one for each row</summary>
        public readonly int AttribIndicesUseCount;

        /// <summary>The type of the attribute declared in the shader</summary>
        public readonly ActiveAttribType AttribType;

        /// <summary>
        /// Creates a VertexAttribDescription where the format of the data declared in the shader is the same as present in the buffer and no conversion needs to be done
        /// </summary>
        /// <param name="attribType">The type of attribute declared in the shader</param>
        public VertexAttribDescription(ActiveAttribType attribType)
        {
            this.AttribType = attribType;
            this.Normalized = false;
            TrippyUtils.GetVertexAttribTypeData(attribType, out this.AttribIndicesUseCount, out this.Size, out this.AttribBaseType);
            this.SizeInBytes = TrippyUtils.GetVertexAttribSizeInBytes(this.AttribBaseType) * this.Size * this.AttribIndicesUseCount;
        }

        /// <summary>
        /// Creates a VertexAttribDescription where the data format declared in the shader isn't the same as the format the data will be read as
        /// </summary>
        /// <param name="attribType">The type of the attribute declared in the shader</param>
        /// <param name="normalized">Whether the vertex data should be normalized before being loaded into the shader</param>
        /// <param name="dataBaseType">The base type in which the data will be read from the buffer</param>
        public VertexAttribDescription(ActiveAttribType attribType, bool normalized, VertexAttribPointerType dataBaseType)
        {
            this.AttribType = attribType;
            this.Normalized = normalized;
            this.AttribBaseType = dataBaseType;
            this.AttribType = attribType;
            this.Size = TrippyUtils.GetVertexAttribTypeSize(attribType);
            this.AttribIndicesUseCount = TrippyUtils.GetVertexAttribTypeIndexCount(attribType);
            this.SizeInBytes = TrippyUtils.GetVertexAttribSizeInBytes(dataBaseType) * this.Size * this.AttribIndicesUseCount;

            if (normalized && !TrippyUtils.IsVertexAttribIntegerType(dataBaseType))
                throw new ArgumentException("For normalized vertex attributes, the dataBaseType must be an integer", "dataBaseType");
        }

        public override string ToString()
        {
            return String.Concat(Normalized ? "Normalized " : "Unnormalized ", AttribType.ToString(), " baseType ", AttribBaseType.ToString());
        }
    }
}
