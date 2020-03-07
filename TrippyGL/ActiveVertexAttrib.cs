using System;
using System.Diagnostics.Contracts;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    /// <summary>
    /// Stores data about an active vertex attribute from a <see cref="ShaderProgram"/>.
    /// </summary>
    public readonly struct ActiveVertexAttrib : IEquatable<ActiveVertexAttrib>
    {
        /// <summary>The attribute's location in the shader.</summary>
        public readonly int Location;

        /// <summary>The name with which the attribute is declared.</summary>
        public readonly string Name;

        /// <summary>The size of the attribute, measured in <see cref="AttribType"/>-s.</summary>
        public readonly int Size;

        /// <summary>The type of the attribute declared in the shader.</summary>
        public readonly ActiveAttribType AttribType;

        /// <summary>
        /// Creates an <see cref="ActiveVertexAttrib"/> and queries the attribute
        /// data from a specified attrib index in a <see cref="ShaderProgram"/>.
        /// </summary>
        /// <param name="program">The <see cref="ShaderProgram"/> from which to query the attrib data.</param>
        /// <param name="attribIndex">The attribute index to query the data from.</param>
        internal ActiveVertexAttrib(ShaderProgram program, int attribIndex)
        {
            // OpenTK has this glGetActiveAttrib that easily turns the required Name parameters into the function's return value
            // So we use that one to get the name, size and type and then query the location separately
            Name = GL.GetActiveAttrib(program.Handle, attribIndex, out Size, out AttribType);
            Location = GL.GetAttribLocation(program.Handle, Name);
        }

        public static bool operator ==(ActiveVertexAttrib left, ActiveVertexAttrib right) => left.Equals(right);

        public static bool operator !=(ActiveVertexAttrib left, ActiveVertexAttrib right) => !left.Equals(right);

        public override string ToString()
        {
            return string.Concat(
                nameof(Location) + "=", Location.ToString(),
                ", " + nameof(Name) + "=\"", Name, "\"",
                ", " + nameof(Size) + "=", Size.ToString(),
                ", " + nameof(AttribType) + "=", AttribType.ToString()
            );
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Location.GetHashCode();
                hashCode = (hashCode * 397) ^ Size.GetHashCode();
                hashCode = (hashCode * 397) ^ AttribType.GetHashCode();
                hashCode = (hashCode * 397) ^ Name.GetHashCode();
                return hashCode;
            }
        }

        public bool Equals(ActiveVertexAttrib other)
        {
            return Location == other.Location
                && Size == other.Size
                && AttribType == other.AttribType
                && Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            if (obj is ActiveVertexAttrib activeVertexAttrib)
                return Equals(activeVertexAttrib);
            return false;
        }
    }
}
