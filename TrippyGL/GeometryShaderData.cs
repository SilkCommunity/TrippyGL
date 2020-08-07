using System;
using Silk.NET.OpenGL;

namespace TrippyGL
{
    /// <summary>
    /// Stores data about a Geometry Shader.
    /// </summary>
    public readonly struct GeometryShaderData : IEquatable<GeometryShaderData>
    {
        /// <summary>The <see cref="PrimitiveType"/> the geometry shader takes as input.</summary>
        public readonly PrimitiveType GeometryInputType;

        /// <summary>The <see cref="PrimitiveType"/> the geometry shader takes as output.</summary>
        public readonly PrimitiveType GeometryOutputType;

        /// <summary>The amount of invocations the geometry shader will do.</summary>
        public readonly int GeometryShaderInvocations;

        /// <summary>The maximum amount of vertices the geometry shader can output.</summary>
        public readonly int GeometryVerticesOut;

        internal GeometryShaderData(GL gl, uint programHandle)
        {
            gl.GetProgram(programHandle, ProgramPropertyARB.GeometryInputType, out int tmp);
            GeometryInputType = (PrimitiveType)tmp;

            gl.GetProgram(programHandle, ProgramPropertyARB.GeometryOutputType, out tmp);
            GeometryOutputType = (PrimitiveType)tmp;

            gl.GetProgram(programHandle, GLEnum.GeometryShaderInvocations, out tmp);
            GeometryShaderInvocations = tmp;

            gl.GetProgram(programHandle, ProgramPropertyARB.GeometryVerticesOut, out tmp);
            GeometryVerticesOut = tmp;
        }

        public static bool operator ==(GeometryShaderData left, GeometryShaderData right) => left.Equals(right);

        public static bool operator !=(GeometryShaderData left, GeometryShaderData right) => !left.Equals(right);

        public override string ToString()
        {
            return string.Concat(
                nameof(GeometryInputType) + "=", GeometryInputType.ToString(),
                nameof(GeometryOutputType) + "=", GeometryOutputType.ToString(),
                nameof(GeometryShaderInvocations) + "=", GeometryShaderInvocations.ToString(),
                nameof(GeometryVerticesOut) + "=", GeometryVerticesOut.ToString()
            );
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(GeometryInputType, GeometryOutputType, GeometryShaderInvocations, GeometryVerticesOut);
        }

        public bool Equals(GeometryShaderData other)
        {
            return GeometryInputType == other.GeometryInputType
                && GeometryOutputType == other.GeometryOutputType
                && GeometryShaderInvocations == other.GeometryShaderInvocations
                && GeometryVerticesOut == other.GeometryVerticesOut;
        }

        public override bool Equals(object obj)
        {
            if (obj is GeometryShaderData geometryShaderData)
                return Equals(geometryShaderData);
            return false;
        }
    }
}
