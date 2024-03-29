using System;

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
        public readonly AttributeType AttribType;

        /// <summary>
        /// Creates an <see cref="ActiveVertexAttrib"/> and queries the attribute
        /// data from a specified attrib index in a <see cref="ShaderProgram"/>.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> that owns the shader program.</param>
        /// <param name="programHandle">The handle of the shader program.</param>
        /// <param name="attribIndex">The attribute index to query the data from.</param>
        internal ActiveVertexAttrib(GraphicsDevice graphicsDevice, uint programHandle, uint attribIndex)
        {
            // We use the glGetActiveAttrib that easily turns the required Name parameters into a string
            // Then we query the size and type and then the location separately
            Name = graphicsDevice.GL.GetActiveAttrib(programHandle, attribIndex, out Size, out Silk.NET.OpenGL.AttributeType attribType);
            AttribType = (AttributeType)attribType;
            Location = graphicsDevice.GL.GetAttribLocation(programHandle, Name);
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
            return HashCode.Combine(Location, Size, AttribType, Name);
        }

        public bool Equals(ActiveVertexAttrib other)
        {
            return Location == other.Location
                && Size == other.Size
                && AttribType == other.AttribType
                && Name == other.Name;
        }

        public override bool Equals(object? obj)
        {
            if (obj is ActiveVertexAttrib activeVertexAttrib)
                return Equals(activeVertexAttrib);
            return false;
        }
    }
}
