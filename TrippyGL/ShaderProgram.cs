using System;
using Silk.NET.OpenGL;

namespace TrippyGL
{
    /// <summary>
    /// Encapsulates an OpenGL program object for using shaders.
    /// Shaders define how things are processed in the graphics card,
    /// from calculating vertex positions to choosing the color of each fragment.
    /// </summary>
    public sealed class ShaderProgram : GraphicsResource
    {
        // TODO: Change this to internal!!!
        /// <summary>The handle for the OpenGL Program object.</summary>
        public readonly uint Handle;

        /// <summary>Gets data about the geometry shader in this <see cref="ShaderProgram"/>, if there is one.</summary>
        public readonly GeometryShaderData GeometryShader;

        /// <summary>The list of uniforms in this <see cref="ShaderProgram"/>.</summary>
        public readonly ShaderUniformList Uniforms;

        /// <summary>The list of block uniforms in this <see cref="ShaderProgram"/>.</summary>
        public readonly ShaderBlockUniformList BlockUniforms;

        /// <summary>The vertex attributes for this <see cref="ShaderProgram"/> queried from OpenGL at program linking.</summary>
        private ActiveVertexAttrib[] activeAttribs;

        /// <summary>Gets the input attributes on this program, once it's been linked.</summary>
        public ReadOnlySpan<ActiveVertexAttrib> ActiveAttribs => activeAttribs;

        /// <summary>Whether this <see cref="ShaderProgram"/> is the one currently in use on it's <see cref="GraphicsDevice"/>.</summary>
        public bool IsCurrentlyInUse => GraphicsDevice.ShaderProgram == this;

        /// <summary>Whether this <see cref="ShaderProgram"/> has a vertex shader attached.</summary>
        public readonly bool HasVertexShader;

        /// <summary>Whether this <see cref="ShaderProgram"/> has a geometry shader attached.</summary>
        public readonly bool HasGeometryShader;

        /// <summary>Whether this <see cref="ShaderProgram"/> has a fragment shader attached.</summary>
        public readonly bool HasFragmentShader;

        internal ShaderProgram(GraphicsDevice graphicsDevice, uint handle, ActiveVertexAttrib[] activeAttribs) : base(graphicsDevice)
        {
            Handle = handle;
            this.activeAttribs = activeAttribs;
            BlockUniforms = new ShaderBlockUniformList(this);
            Uniforms = ShaderUniformList.CreateForProgram(this);
        }

        /// <summary>
        /// Ensures this program is the one currently in use for it's <see cref="GraphicsDevice"/>.
        /// </summary>
        internal void EnsureInUse()
        {
            GraphicsDevice.ShaderProgram = this;
        }

        /// <summary>
        /// Ensures all necessary states are set for a draw command to use this program, such as making
        /// sure sampler or block uniforms are properly set.<para/>
        /// This should always be called before a draw operation and assumes this
        /// <see cref="ShaderProgram"/> is the one currently in use.
        /// </summary>
        internal void EnsurePreDrawStates()
        {
            Uniforms?.EnsureSamplerUniformsSet();
            BlockUniforms.EnsureBufferBindingsSet();
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
