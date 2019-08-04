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

        /// <summary>
        /// Creates an ActiveVertexAttrib and queries the attribute data from a specified attrib index in a ShaderProgram
        /// </summary>
        /// <param name="program">The ShaderProgram from which to query the attrib data</param>
        /// <param name="attribIndex">The attribute index to query the data from</param>
        internal ActiveVertexAttrib(ShaderProgram program, int attribIndex)
        {
            // OpenTK has this glGetActiveAttrib that easily turns the required Name parameters into the function's return value
            // So we use that one to get the name, size and type and then query the location separately
            Name = GL.GetActiveAttrib(program.Handle, attribIndex, out Size, out AttribType);
            Location = GL.GetAttribLocation(program.Handle, Name);
        }

        public override string ToString()
        {
            return String.Concat("Location=", Location.ToString(), ", Name=\"", Name, "\", Size=", Size.ToString(), ", AttribType=", AttribType.ToString());
        }
    }
}
