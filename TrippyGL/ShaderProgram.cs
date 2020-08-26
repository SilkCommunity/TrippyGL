using System;
using Silk.NET.OpenGL;

namespace TrippyGL
{
    /// <summary>
    /// Encapsulates an OpenGL program object for using shaders.
    /// Shaders define how things are processed in the graphics pipeline,
    /// from calculating vertex positions to choosing the color of each fragment.
    /// </summary>
    public class ShaderProgram : GraphicsResource
    {
        /// <summary>The handle for the OpenGL Program object.</summary>
        public readonly uint Handle;

        /// <summary>Gets data about the geometry shader in this <see cref="ShaderProgram"/>, if there is one.</summary>
        public readonly GeometryShaderData GeometryShader;

        /// <summary>The list of uniforms in this <see cref="ShaderProgram"/>.</summary>
        public readonly ShaderUniformList Uniforms;

        /// <summary>The list of block uniforms in this <see cref="ShaderProgram"/>.</summary>
        public readonly ShaderBlockUniformList BlockUniforms;

        /// <summary>The vertex attributes for this <see cref="ShaderProgram"/> queried from OpenGL.</summary>
        private readonly ActiveVertexAttrib[] activeAttribs;

        /// <summary>Gets the input attributes on this program.</summary>
        public ReadOnlySpan<ActiveVertexAttrib> ActiveAttribs => new ReadOnlySpan<ActiveVertexAttrib>(activeAttribs);

        /// <summary>Whether this <see cref="ShaderProgram"/> has a vertex shader attached.</summary>
        public readonly bool HasVertexShader;

        /// <summary>Whether this <see cref="ShaderProgram"/> has a geometry shader attached.</summary>
        public readonly bool HasGeometryShader;

        /// <summary>Whether this <see cref="ShaderProgram"/> has a fragment shader attached.</summary>
        public readonly bool HasFragmentShader;

        /// <summary>
        /// Used internally. When a sampler-type <see cref="ShaderUniform"/> is modified, this value
        /// is set to true. The value is then used on <see cref="ShaderUniformList.EnsureSamplerUniformsSet"/>
        /// to know whether the textures need to be bound and set
        /// </summary>
        internal bool areSamplerUniformsDirty;

        /// <summary>
        /// Creates a <see cref="ShaderProgram"/> from an already compiled GL Program Object.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> this resource will use.</param>
        /// <param name="programHandle">The GL Program Object's handle.</param>
        /// <param name="activeAttribs">The active attributes, already queried from the program.</param>
        /// <remarks>The active attributes must have already been checked to be valid.</remarks>
        internal ShaderProgram(GraphicsDevice graphicsDevice, uint programHandle, ActiveVertexAttrib[] activeAttribs)
            : base(graphicsDevice)
        {
            Handle = programHandle;
            this.activeAttribs = activeAttribs;
            BlockUniforms = new ShaderBlockUniformList(this);
            Uniforms = ShaderUniformList.CreateForProgram(this);
            areSamplerUniformsDirty = Uniforms.hasSamplerUniforms;
        }

        /// <summary>
        /// Ensures this program is the one currently in use for it's <see cref="GraphicsDevice"/>.
        /// </summary>
        internal void EnsureInUse()
        {
            GraphicsDevice.ShaderProgram = this;
        }

        /// <summary>
        /// Ensures all necessary states are set for a draw command to use this program,
        /// such as making sure sampler or block uniforms are properly set.<para/>
        /// This should always be called before a draw operation and assumes this
        /// <see cref="ShaderProgram"/> is the one currently in use.
        /// </summary>
        internal void EnsurePreDrawStates()
        {
            Uniforms.EnsureSamplerUniformsSet();
            BlockUniforms.EnsureBufferBindingsSet();
        }

        /// <summary>
        /// Tries to find an <see cref="ActiveVertexAttrib"/> in this <see cref="ShaderProgram"/> with
        /// the specified location.
        /// </summary>
        /// <param name="location">The location to look for an <see cref="ActiveVertexAttrib"/> at.</param>
        /// <param name="attrib">The <see cref="ActiveVertexAttrib"/> that was found, if one was found.</param>
        /// <returns>Whether an attribute was found in said location.</returns>
        public bool TryFindAttributeByLocation(int location, out ActiveVertexAttrib attrib)
        {
            for(int i=0; i<activeAttribs.Length; i++)
                if(activeAttribs[i].Location == location)
                {
                    attrib = activeAttribs[i];
                    return true;
                }

            attrib = default;
            return false;
        }

        protected override void Dispose(bool isManualDispose)
        {
            if (isManualDispose && GraphicsDevice.ShaderProgram == this)
                GraphicsDevice.ShaderProgram = null;

            GL.DeleteProgram(Handle);
            base.Dispose(isManualDispose);
        }
    }
}
