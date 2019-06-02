using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    /// <summary>
    /// Manages all OpenGL operations such as binding buffers, textures, arrays, etc.
    /// This class saves which things are bound where and ensures things like unnecessarily binding twice don't happen
    /// </summary>
    public static class States
    {
        /// <summary>
        /// Initializes all states variables
        /// </summary>
        internal static void Init()
        {
            bufferBindings = new List<int>(14);
            bufferBindingTargets = new List<BufferTarget>(14);
            bufferRangeBindings = new BufferRangeBinding[4][];

            bufferBindingTargets.Add(BufferTarget.TransformFeedbackBuffer);
            bufferBindingTargets.Add(BufferTarget.UniformBuffer);       // While not all of these might be needed, I'll put them all
            bufferBindingTargets.Add(BufferTarget.ShaderStorageBuffer); // in here to ensure compatibility if such features are ever 
            bufferBindingTargets.Add(BufferTarget.AtomicCounterBuffer); // implemented into the library or whatever.

            bufferBindings.Add(0);
            bufferBindings.Add(0);
            bufferBindings.Add(0);
            bufferBindings.Add(0);

            bufferRangeBindings[0] = new BufferRangeBinding[GL.GetInteger(GetPName.MaxTransformFeedbackBuffers)];
            bufferRangeBindings[1] = new BufferRangeBinding[GL.GetInteger(GetPName.MaxUniformBufferBindings)];
            bufferRangeBindings[2] = new BufferRangeBinding[GL.GetInteger((GetPName)All.MaxShaderStorageBufferBindings)]; //opentk wtf
            bufferRangeBindings[3] = new BufferRangeBinding[GL.GetInteger((GetPName)All.MaxAtomicCounterBufferBindings)];


            vertexArrayBinding = 0;
            shaderProgramBinding = 0;
        }

        /// <summary>
        /// Resets all saved states. These variables are used to prevent unnecessarily setting the same states twice.
        /// You should only need to call this when interoperating with other libraries or using your own GL functions
        /// </summary>
        public static void ResetStates()
        {
            ResetBufferStates();
            ResetVertexArrayStates();
            ResetShaderProgramStates();
        }


        #region BufferObjectBindingStates

        /// <summary>Stores the handle of the last buffer bound to the BufferTarget found on the same index on the bufferBindingsTarget array</summary>
        private static List<int> bufferBindings;
        
        /// <summary>The BufferTargets for the handles found on the bufferBindings array</summary>
        private static List<BufferTarget> bufferBindingTargets;

        /// <summary>TODO: add summary and explanation</summary>
        private static BufferRangeBinding[][] bufferRangeBindings;

        /// <summary>
        /// Ensures a buffer is bound to it's BufferTarget by binding it if it's not
        /// </summary>
        /// <param name="buffer">The buffer to ensure is bound. This value is assumed not to be null</param>
        public static void EnsureBufferBound(BufferObject buffer)
        {
            if (bufferBindings[buffer.bufferBindingTargetIndex] != buffer.Handle)
                BindBuffer(buffer);
        }

        /// <summary>
        /// Binds a buffer to it's BufferTarget. Prefer using EnsureBufferBound() instead to prevent unnecessary binds
        /// </summary>
        /// <param name="buffer">The buffer to bind. This value is assumed not to be null</param>
        public static void BindBuffer(BufferObject buffer)
        {
            bufferBindings[buffer.bufferBindingTargetIndex] = buffer.Handle;
            GL.BindBuffer(buffer.BufferTarget, buffer.Handle);
        }

        /// <summary>
        /// Ensures a buffer is bound to a specified binding index in it's BufferTarget by binding it if it's not.
        /// The buffer object's BufferTarget must be one with multiple binding indexes
        /// </summary>
        /// <param name="buffer">The buffer to ensure is bound. This value is assumed not to be null</param>
        /// <param name="bindingIndex">The binding index in the buffer target where the buffer should be bound</param>
        public static void EnsureBufferBoundBase(BufferObject buffer, int bindingIndex)
        {
            BufferRangeBinding b = bufferRangeBindings[buffer.bufferBindingTargetIndex][bindingIndex];
            if (b.BufferHandle != buffer.Handle || b.Size != buffer.StorageLengthInBytes || b.Offset != 0)
                BindBufferBase(buffer, bindingIndex);
        }

        /// <summary>
        /// Bind a buffer to a binding index on it's BufferTarget. Prefer using EnsureBufferBoundBase() to prevent unnecessary binds.
        /// The buffer object's BufferTarget must be one with multiple binding indexes
        /// </summary>
        /// <param name="buffer">The buffer to bind. This value is assumed not to be null</param>
        /// <param name="bindingIndex">The binding index in the buffer target where the buffer will be bound</param>
        public static void BindBufferBase(BufferObject buffer, int bindingIndex)
        {
            bufferBindings[buffer.bufferBindingTargetIndex] = buffer.Handle;
            bufferRangeBindings[buffer.bufferBindingTargetIndex][bindingIndex].SetBase(buffer);
            GL.BindBufferBase((BufferRangeTarget)buffer.BufferTarget, bindingIndex, buffer.Handle);
        }

        /// <summary>
        /// Ensures a buffer's range is bound to a specified binding index in it's BufferTarget by binding it if it's not.
        /// The buffer object's BufferTarget must be one with multiple binding indexes
        /// </summary>
        /// <param name="buffer">The buffer to bind. This value is assumed not to be null</param>
        /// <param name="bindingIndex">The binding index in the buffer target where the buffer will be bound</param>
        /// <param name="offset">The offset in bytes into the buffer's storage where the bind begins</param>
        /// <param name="size">The amount of bytes that can be read from the storage, starting from offset</param>
        public static void EnsureBufferBoundRange(BufferObject buffer, int bindingIndex, int offset, int size)
        {
            BufferRangeBinding b = bufferRangeBindings[buffer.bufferBindingTargetIndex][bindingIndex];
            if (b.BufferHandle != buffer.Handle || b.Size != size || b.Offset != offset)
                BindBufferRange(buffer, bindingIndex, offset, size);
        }

        /// <summary>
        /// Bind a range of a buffer to a binding index on it's BufferTarget. Prefer using EnsureBufferBoundBase() to prevent unnecessary binds.
        /// The buffer object's BufferTarget must be one with multiple binding indexes
        /// </summary>
        /// <param name="buffer">The buffer to bind. This value is assumed not to be null</param>
        /// <param name="bindingIndex">The binding index in the buffer target where the buffer will be bound</param>
        /// <param name="offset">The offset in bytes into the buffer's storage where the bind begins</param>
        /// <param name="size">The amount of bytes that can be read from the storage, starting from offset</param>
        public static void BindBufferRange(BufferObject buffer, int bindingIndex, int offset, int size)
        {
            bufferBindings[buffer.bufferBindingTargetIndex] = buffer.Handle;
            bufferRangeBindings[buffer.bufferBindingTargetIndex][bindingIndex].SetRange(buffer, offset, size);
            GL.BindBufferRange((BufferRangeTarget)buffer.BufferTarget, bindingIndex, buffer.Handle, (IntPtr)offset, size);
        }

        /// <summary>
        /// Returns whether the given buffer is the currently bound one for it's BufferTarget
        /// </summary>
        /// <param name="buffer">The buffer to check. This value is assumed not to be null</param>
        public static bool IsBufferCurrentlyBound(BufferObject buffer)
        {
            return bufferBindings[buffer.bufferBindingTargetIndex] == buffer.Handle;
        }

        /// <summary>
        /// Gets the index on the 'bufferBindings' list for the specified BufferTarget.
        /// If there's no index for that BufferTarget, it's created
        /// </summary>
        /// <param name="bufferTarget">The BufferTarget to get the binds list index for</param>
        internal static int GetBindingTargetIndex(BufferTarget bufferTarget)
        {
            for (int i = 0; i < bufferBindingTargets.Count; i++)
                if (bufferBindingTargets[i] == bufferTarget)
                    return i;
            bufferBindings.Add(-1);
            bufferBindingTargets.Add(bufferTarget);
            return bufferBindings.Count - 1;
        }

        /// <summary>
        /// Resets all saved states for buffer objects. This is, the variables used to check whether to bind a buffer or not.
        /// You should only need to call this when interoperating with other libraries or using your own GL functions
        /// </summary>
        public static void ResetBufferStates()
        {
            for (int i = 0; i < bufferBindings.Count; i++)
                bufferBindings[i] = 0;
        }

        #endregion BufferObjectBindingStates

        #region VertexArrayBindingStates

        /// <summary>The currently bound VertexArray's handle</summary>
        private static int vertexArrayBinding;

        /// <summary>
        /// Ensures a vertex array is bound by binding it if it's not
        /// </summary>
        /// <param name="array">The array to ensure is bound. This value is assumed not to be null</param>
        public static void EnsureVertexArrayBound(VertexArray array)
        {
            if (vertexArrayBinding != array.Handle)
                BindVertexArray(array);
        }

        /// <summary>
        /// Binds a vertex array. Prefer using EnsureVertexArrayBound() instead to prevent unnecessary binds
        /// </summary>
        /// <param name="array">The array to ensure is bound. This value is assumed not to be null</param>
        public static void BindVertexArray(VertexArray array)
        {
            GL.BindVertexArray(array.Handle);
            vertexArrayBinding = array.Handle;
        }

        /// <summary>
        /// Resets all saved states for vertex arrays This is, the variables used to check whether to bind a vertex array or not.
        /// You should only need to call this when interoperating with other libraries or using your own GL functions
        /// </summary>
        public static void ResetVertexArrayStates()
        {
            vertexArrayBinding = GL.GetInteger(GetPName.VertexArrayBinding);
        }
        
        /// <summary>
        /// Unbinds the currently active vertex array by binding to array 0
        /// </summary>
        public static void UnbindVertexArray()
        {
            GL.BindVertexArray(0);
            vertexArrayBinding = 0;
        }

        #endregion VertexArrayBindingStates

        #region ShaderProgramBindingStates

        /// <summary>The currently bound ShaderProgram's handle</summary>
        private static int shaderProgramBinding;

        /// <summary>
        /// Returns whether the given shader program is the one currently in use
        /// </summary>
        /// <param name="program">The program to check, This value is assumed to not be null</param>
        public static bool IsShaderProgramInUse(ShaderProgram program)
        {
            return shaderProgramBinding == program.Handle;
        }

        /// <summary>
        /// Ensures the given ShaderProgram is the one currently in use
        /// </summary>
        /// <param name="program">The shader program to use. This value is assumed not to be null</param>
        public static void EnsureShaderProgramInUse(ShaderProgram program)
        {
            if (shaderProgramBinding != program.Handle)
                UseShaderProgram(program);
        }

        /// <summary>
        /// Installs the given program into the rendering pipeline. Prefer using EnsureShaderProgramInUse() to avoid unnecessary uses
        /// </summary>
        /// <param name="program">The shader program to use. This value is assumed not to be null</param>
        public static void UseShaderProgram(ShaderProgram program)
        {
            shaderProgramBinding = program.Handle;
            GL.UseProgram(program.Handle);
        }

        /// <summary>
        /// Uninstalls the current shader program from the pipeline by using program 0
        /// </summary>
        public static void UninstallShaderProgram()
        {
            shaderProgramBinding = 0;
            GL.UseProgram(0);
        }

        /// <summary>
        /// Resets all saved states for shader programs. This is, the variables used to check whether to use a shader program or not.
        /// You should only need to call this when itneroperating with other libraries or using your own GL functions
        /// </summary>
        public static void ResetShaderProgramStates()
        {
            shaderProgramBinding = 0;
        }

        #endregion ShaderProgramBindingStates

        #region TextureBindingStates

        private static int[] textureBindings;
        private static int activeTextureUnit;

        #endregion TextureBindingStates

        //TODO: Pass absolutely ALL BINDING THINGS to here. Textures are missing!

        /// <summary>
        /// This struct is used to manage buffer object binding in cases where a buffer can be bound to multiple indices in the same target.
        /// Each BufferRangeBinding represents one of these binding points in a BufferTarget. Of course, this must be in a BufferTarget
        /// to which multiple buffers can be bound.
        /// </summary>
        private struct BufferRangeBinding
        {
            public int BufferHandle;
            public int Offset;
            public int Size;

            /// <summary>
            /// Set the values of this BufferRangeBinding as for when glBindBufferBase was called
            /// </summary>
            public void SetBase(BufferObject buffer)
            {
                this.BufferHandle = buffer.Handle;
                this.Offset = 0;
                this.Size = buffer.StorageLengthInBytes;
            }

            /// <summary>
            /// Set the values of the BufferRangeBinding
            /// </summary>
            public void SetRange(BufferObject buffer, int offset, int size)
            {
                this.BufferHandle = buffer.Handle;
                this.Offset = offset;
                this.Size = size;
            }
        }
    }
}
