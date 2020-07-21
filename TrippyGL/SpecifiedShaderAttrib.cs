using System;
using Silk.NET.OpenGL;

namespace TrippyGL
{
    /// <summary>
    /// Used by <see cref="ShaderProgramBuilder"/> for specifying vertex attributes.
    /// </summary>
    public struct SpecifiedShaderAttrib : IEquatable<SpecifiedShaderAttrib>
    {
        /// <summary>The shader attribute's type.</summary>
        public AttributeType AttribType;
        /// <summary>The name with which the shader attribute is declared in the shader.</summary>
        public string Name;

        /// <summary>
        /// Creates a <see cref="SpecifiedShaderAttrib"/> with the given values.
        /// </summary>
        public SpecifiedShaderAttrib(string name, AttributeType type)
        {
            AttribType = type;
            Name = name;
        }

        public static bool operator ==(SpecifiedShaderAttrib left, SpecifiedShaderAttrib right) => left.Equals(right);

        public static bool operator !=(SpecifiedShaderAttrib left, SpecifiedShaderAttrib right) => !left.Equals(right);

        /// <summary>
        /// Whether this <see cref="SpecifiedShaderAttrib"/> and another <see cref="ActiveVertexAttrib"/> have matching data.
        /// </summary>
        public bool Matches(in ActiveVertexAttrib activeAttrib)
        {
            return AttribType == activeAttrib.AttribType
                && Name == activeAttrib.Name;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = AttribType.GetHashCode();
                hashCode = (hashCode * 397) ^ Name.GetHashCode(StringComparison.InvariantCulture);
                return hashCode;
            }
        }

        public bool Equals(SpecifiedShaderAttrib shaderAttrib)
        {
            return AttribType == shaderAttrib.AttribType && Name == shaderAttrib.Name;
        }

        public override bool Equals(object obj)
        {
            if (obj is SpecifiedShaderAttrib shaderAttrib)
                return Equals(shaderAttrib);
            return false;
        }

        public override string ToString()
        {
            return string.Concat(AttribType.ToString(), " ", Name);
        }
    }
}
