using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    /// <summary>
    /// Controls transform feedback operations and stores the states needed for the transform feedback to work
    /// </summary>
    public class TransformFeedbackObject : GraphicsResource
    {
        // All the states stored by an OpenGL Transform Feedback Object:
        // The generic buffer binding GL_TRANSFORM_FEEDBACK_BUFFER
        // The indexed (ranged) buffer binding GL_TRANSFORM_FEEDBACK_BUFFER
        // Whether the feedback operation is paused and/or active
        // The current amount of primitives recorded in the current feedback operation, if active

        /// <summary>The GL handle of this Transform Feedback Object</summary>
        public readonly int Handle;

        /// <summary>Whether this transform feedback is in an active feedback operation</summary>
        public bool IsActive { get; private set; }

        /// <summary>Whether the transform feedback operation is paused</summary>
        public bool IsPaused { get; private set; }

        /// <summary>The mode in which the transform feedback attributes are written into the buffers</summary>
        public TransformFeedbackMode TransformFeedbackMode { get; private set; }

        /// <summary>The output variables with which this transform feedback is configures</summary>
        public readonly TransformFeedbackVariableDescriptionList Variables;

        /// <summary>The binding for each buffer index in this transform feedback</summary>
        private GraphicsDevice.BufferRangeBinding[] bufferBindings;

        private TransformFeedbackPrimitiveType beginPrimitiveType;

        public TransformFeedbackObject(GraphicsDevice graphicsDevice, TransformFeedbackVariableDescription[] variableDescriptions) : base(graphicsDevice)
        {
            if (variableDescriptions.Length == 0)
                throw new ArgumentException("At least one variable description must be specified for transform feedback");

            IsActive = false;
            IsPaused = false;
            Variables = new TransformFeedbackVariableDescriptionList(variableDescriptions);

            if (Variables.BufferBindingsNeeded > graphicsDevice.MaxTransformFeedbackBuffers)
                throw new PlatformNotSupportedException("The specified variable descriptions need more buffers than are supported on this system");

            if (Variables.ContainsPadding && graphicsDevice.IsAdvancedTransformFeedbackAvailable)
                throw new PlatformNotSupportedException("Padding transform feedback output variables requires advanced transform feedback capabilities");

            if (!Variables.ContainsPadding && Variables.BufferBindingsNeeded == Variables.Count)
            { // No padding and one variable goes to each buffer? Then let's just use separate attribs
                #region UseSeparateAttribs

                TransformFeedbackMode = TransformFeedbackMode.SeparateAttribs;

                for (int i = 0; i < Variables.Count; i++)
                    if (Variables[i].ComponentCount > graphicsDevice.MaxTransformFeedbackSeparateComponents)
                        throw new PlatformNotSupportedException("A specified variable description uses more components than the maximum supported on this system for separate attribs");

                if (Variables.AttribCount > graphicsDevice.MaxTransformFeedbackSeparateAttribs)
                    throw new PlatformNotSupportedException("The specified variable descriptions need more separate attribs than are supported on this system");

                bufferBindings = new GraphicsDevice.BufferRangeBinding[Variables.Count];
                for (int i = 0; i < bufferBindings.Length; i++)
                    bufferBindings[i].SetRange(Variables[i].BufferSubset);
                #endregion
            }
            else
            { // We're gonna need to use interleaving or advanced interleaving
                #region UseInterleavedAttribs
                TransformFeedbackMode = TransformFeedbackMode.InterleavedAttribs;

                if (Variables.AttribCount > graphicsDevice.MaxTransformFeedbackInterleavedComponents)
                    throw new PlatformNotSupportedException("The specified variable descriptions needs more interleaved components than are supported on this system");

                for (int i = 0; i < Variables.Count; i++)
                    if (Variables[i].ComponentCount > graphicsDevice.MaxTransformFeedbackInterleavedComponents)
                        throw new PlatformNotSupportedException("A specified variable description uses more components than the maximum supported on this system for interleaved attribs");

                bufferBindings = new GraphicsDevice.BufferRangeBinding[Variables.BufferBindingsNeeded];
                int bindingIndex = 1;
                int variableIndex = 0;

                bufferBindings[0].SetRange(Variables[0].BufferSubset);
                while(bindingIndex < bufferBindings.Length)
                {
                    if (Variables[variableIndex].BufferSubset != Variables[variableIndex - 1].BufferSubset)
                    {
                        // When the buffer subset changes, we need to go to the next index
                        bufferBindings[bindingIndex++].SetRange(Variables[variableIndex].BufferSubset);
                    }
                    variableIndex++;
                }

                #endregion
            }

            Handle = graphicsDevice.IsAdvancedTransformFeedbackAvailable ? GL.GenTransformFeedback() : -1;
            if (Handle != -1)
            {
                graphicsDevice.ForceBindTransformFeedback(this);
                for (int i = 0; i < bufferBindings.Length; i++)
                    GL.BindBufferRange(BufferRangeTarget.TransformFeedbackBuffer, i, bufferBindings[i].Buffer.Handle, (IntPtr)bufferBindings[i].Offset, bufferBindings[i].Size);
            }
        }

        /// <summary>
        /// Configures the transform feedback varyings for a ShaderProgram. The ShaderProgram must be unlinked.
        /// This method is called by ShaderProgram.ConfigureTransformFeedback()
        /// </summary>
        /// <param name="program">The unlinked program</param>
        /// <param name="feedbackOutputNames">The provided names for the transform feedback output variables</param>
        internal void PerformConfigureShaderProgram(ShaderProgram program, string[] feedbackOutputNames)
        {
            if (Variables.AttribCount != feedbackOutputNames.Length)
                throw new InvalidOperationException("The amount of specified output variables names don't match the amount of variables on this transform feedback");

            // We'll store all the varyings to give OpenGL in this list
            List<string> varyings = new List<string>(feedbackOutputNames.Length * 2);
            int nameIndex = 0;

            for (int i = 0; i < Variables.Count; i++)
            {
                if (i != 0 && Variables[i].BufferSubset != Variables[i - 1].BufferSubset)
                {
                    #region NextBuffer
                    // If the subset changed, we're now going into the next buffer index
                    varyings.Add("gl_NextBuffer");

                    // We might need to add padding, if the same buffer subset is specified twice but not consecutively, it's
                    // going to require two buffer indices and the second variable is gonna need padding so it doesn't overwrite
                    // the first variable
                    int skips = Variables.CalculateVariableOffsetIntoSubset(i);
                    while (skips >= 4)
                    {
                        varyings.Add("gl_SkipComponents4");
                        skips -= 4;
                    }
                    if (skips != 0)
                        varyings.Add("gl_SkipComponents" + skips.ToString());
                    #endregion
                }

                // Padding variables are specified to OpenGL as "gl_SkipComponentsX", with 'X' being an integer in the range [1, 4] 
                if (Variables[i].IsPadding)
                    varyings.Add(String.Concat("gl_SkipComponents", Variables[i].PaddingComponentCount.ToString()));
                else // If it's not padding, then let's get the next name and assign that name to the variable
                    varyings.Add(feedbackOutputNames[nameIndex++]);
            }
            // nameIndex at this point should equal feedbackOutputNames.Length, because we checked
            // that Variables.AttribCount equals it at the beginning.

            GL.TransformFeedbackVaryings(program.Handle, varyings.Count, varyings.ToArray(), TransformFeedbackMode);
        }

        /// <summary>
        /// Starts a transform feedback operation
        /// </summary>
        /// <param name="primitiveType">The primitive type to record</param>
        public void Begin(TransformFeedbackPrimitiveType primitiveType)
        {
            if (GraphicsDevice.TransformFeedback == null)
                GraphicsDevice.ForceBindTransformFeedback(this);
            else
            {
                if (GraphicsDevice.TransformFeedback.IsActive)
                    throw new InvalidOperationException("Another transform feedback operation is already active. It must end before another one can begin");

                GraphicsDevice.TransformFeedback = this;
            }

            if (GraphicsDevice.ShaderProgram == null)
                throw new InvalidOperationException("A transform feedback operation can't start if no ShaderProgram is in use");

            beginPrimitiveType = primitiveType;
            GL.BeginTransformFeedback(primitiveType);

            IsActive = true;
            IsPaused = false;
        }

        /// <summary>
        /// Pauses the current active transform feedback operation
        /// </summary>
        public void Pause()
        {
            if (GraphicsDevice.IsAdvancedTransformFeedbackAvailable)
                throw new PlatformNotSupportedException("Transform feedback pausing required advanced transform feedback");

            if (!IsActive)
                throw new InvalidOperationException("A transform feedback operation must be active to be paused");

            if (IsPaused)
                throw new InvalidOperationException("This transform feedback operation is already paused");

            GL.PauseTransformFeedback();
            IsPaused = true;
        }

        /// <summary>
        /// Resumes the current transform feedback oepration from being paused
        /// </summary>
        public void Resume()
        {
            if (GraphicsDevice.IsAdvancedTransformFeedbackAvailable)
                throw new PlatformNotSupportedException("Transform feedback pausing required OpenGL 4.0");

            if (!IsActive || GraphicsDevice.TransformFeedback != this) // The second condition shouldn't be necessary, since you can't change a TFO while it's active.
                throw new InvalidOperationException("A transform feedback operation must be active to be resumed");

            if (!IsPaused)
                throw new InvalidOperationException("This transform feedback operation is not paused");

            GL.ResumeTransformFeedback();
            IsPaused = false;
        }

        /// <summary>
        /// Ends the current transform feedback operation
        /// </summary>
        public void End()
        {
            if (!IsActive || GraphicsDevice.TransformFeedback != this) // The second condition shouldn't be necessary, since you can't change a TFO while it's active.
                throw new InvalidOperationException("This transform feedback object doesn't have an active feedback operation to end");

            GL.EndTransformFeedback();
            IsActive = false;
            IsPaused = false;
        }

        protected override void Dispose(bool isManualDispose)
        {
            if (Handle != -1)
                GL.DeleteTransformFeedback(Handle);
            base.Dispose(isManualDispose);
        }

        /// <summary>
        /// Calls the OpenGL functions needed to bind this transform feedback for use
        /// </summary>
        internal void PerformBindOperation()
        {
            if (GraphicsDevice.TransformFeedback != null && GraphicsDevice.TransformFeedback.IsActive)
                throw new InvalidOperationException("A TransformFeedbackObject cannot be bound if the current one is in an active feedback operation");

            if (Handle == -1)
            {
                // Bind for non-feedback-objects
                for (int i = 0; i < bufferBindings.Length; i++)
                    GL.BindBufferRange(BufferRangeTarget.TransformFeedbackBuffer, i, bufferBindings[i].Buffer.Handle, (IntPtr)bufferBindings[i].Offset, bufferBindings[i].Size);
            }
            else
                GL.BindTransformFeedback(TransformFeedbackTarget.TransformFeedback, Handle);
        }

        /// <summary>
        /// Calls the OpenGL functions needed to unbind any bound transform feedback
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice to unbind the transform feedback from</param>
        internal static void PerformUnbindOperation(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice.TransformFeedback != null && graphicsDevice.TransformFeedback.IsActive)
                throw new InvalidOperationException("A TransformFeedbackObject cannot be unbound if the current one is in an active feedback operation");

            if (graphicsDevice.IsAdvancedTransformFeedbackAvailable)
                GL.BindTransformFeedback(TransformFeedbackTarget.TransformFeedback, 0);
            else
            {
                // Unbind for non-feedback-objects
                for (int i = 0; i < graphicsDevice.MaxTransformFeedbackBuffers; i++)
                    GL.BindBufferBase(BufferRangeTarget.TransformFeedbackBuffer, i, 0);
            }
        }
    }
}
