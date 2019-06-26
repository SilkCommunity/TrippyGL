using System;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    /// <summary>
    /// Stores data about an active vertex attribute from a ShaderProgram
    /// </summary>
    public class ActiveVertexAttrib
    {
        /// <summary>The attribute's location</summary>
        public readonly int Location;

        /// <summary>The name with which the attribute is declared</summary>
        public readonly string Name;

        /// <summary>The size of the attribute, measured in AttribType-s. Usually one</summary>
        public readonly int Size;

        /// <summary>The type of the attribute declared in the shader</summary>
        public readonly ActiveAttribType AttribType;

        internal ActiveVertexAttrib(ShaderProgram program, int attribIndex)
        {
            Name = GL.GetActiveAttrib(program.Handle, attribIndex, out Size, out AttribType);
            Location = GL.GetAttribLocation(program.Handle, Name);
        }

        public override string ToString()
        {
            return String.Concat("Location=", Location.ToString(), ", Name=\"", Name, "\", Size=", Size.ToString(), ", AttribType=", AttribType.ToString());
        }
    }
}
