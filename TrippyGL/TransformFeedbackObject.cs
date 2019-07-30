using System;
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

        public readonly int Handle;

        public bool IsActive { get; private set; }

        public bool IsPaused { get; private set; }

        public TransformFeedbackPrimitiveType PrimitiveType { get; set; }

        public TransformFeedbackMode TransformFeedbackMode { get; private set; }

        public readonly TransformFeedbackVariableDescriptionList Variables;

        private GraphicsDevice.BufferRangeBinding[] bufferBindings;

        public TransformFeedbackObject(GraphicsDevice graphicsDevice, TransformFeedbackVariableDescription[] variableDescriptions, TransformFeedbackPrimitiveType primitiveType) : base(graphicsDevice)
        {
            Handle = graphicsDevice.IsTransformFeedbackObjectsAvailable ? GL.GenTransformFeedback() : -1;
            IsActive = false;
            IsPaused = false;
            PrimitiveType = primitiveType;
            Variables = new TransformFeedbackVariableDescriptionList(variableDescriptions);

            if (Variables.BufferSubsetCount > graphicsDevice.MaxTransformFeedbackBuffers)
                throw new PlatformNotSupportedException("The specified variable descriptions need more buffers than are supported on this system");


            if (!Variables.ContainsPadding && Variables.BufferSubsetCount == Variables.Count)
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
            {
                #region UseInterleavedAttribs
                TransformFeedbackMode = TransformFeedbackMode.InterleavedAttribs;

                if (Variables.AttribCount > graphicsDevice.MaxTransformFeedbackInterleavedComponents)
                    throw new PlatformNotSupportedException("The specified variable descriptions needs more interleaved components than are supported on this system");

                for (int i = 0; i < Variables.Count; i++)
                    if (Variables[i].ComponentCount > graphicsDevice.MaxTransformFeedbackInterleavedComponents)
                        throw new PlatformNotSupportedException("A specified variable description uses more components than the maximum supported on this system for interleaved attribs");

                bufferBindings = new GraphicsDevice.BufferRangeBinding[Variables.BufferSubsetCount];

                for(int i=0; i<Variables.Count; i++)
                {
                    
                }

                #endregion
            }

            if (Handle != -1)
            {
                graphicsDevice.ForceBindTransformFeedback(this);
                for (int i = 0; i < bufferBindings.Length; i++)
                    GL.BindBufferRange(BufferRangeTarget.TransformFeedbackBuffer, i, bufferBindings[i].Buffer.Handle, (IntPtr)bufferBindings[i].Offset, bufferBindings[i].Size);
            }
        }

        public void Begin()
        {
            if (GraphicsDevice.TransformFeedback == null)
                GraphicsDevice.ForceBindTransformFeedback(this);
            else
            {
                if (GraphicsDevice.TransformFeedback.IsActive)
                    throw new InvalidOperationException("Another transform feedback operation is already active. It must end before another one can begin");

                GraphicsDevice.TransformFeedback = this;
                GL.BeginTransformFeedback(PrimitiveType);

                IsActive = true;
                IsPaused = false;
            }
        }

        public void Pause()
        {
            if (GraphicsDevice.IsTransformFeedbackObjectsAvailable)
                throw new PlatformNotSupportedException("Transform feedback pausing required OpenGL 4.0");

            if (!IsActive || GraphicsDevice.TransformFeedback != this) // The second condition shouldn't be necessary, since you can't change a TFO while it's active.
                throw new InvalidOperationException("A transform feedback operation must be active to be paused");

            if (IsPaused)
                throw new InvalidOperationException("This transform feedback operation is already paused");

            GL.PauseTransformFeedback();
            IsPaused = true;
        }

        public void Resume()
        {
            if (GraphicsDevice.IsTransformFeedbackObjectsAvailable)
                throw new PlatformNotSupportedException("Transform feedback pausing required OpenGL 4.0");

            if (!IsActive || GraphicsDevice.TransformFeedback != this) // The second condition shouldn't be necessary, since you can't change a TFO while it's active.
                throw new InvalidOperationException("A transform feedback operation must be active to be resumed");

            if (!IsPaused)
                throw new InvalidOperationException("This transform feedback operation is not paused");

            GL.ResumeTransformFeedback();
            IsPaused = false;
        }

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

        internal static void PerformUnbindOperation(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice.TransformFeedback != null && graphicsDevice.TransformFeedback.IsActive)
                throw new InvalidOperationException("A TransformFeedbackObject cannot be unbound if the current one is in an active feedback operation");

            if (graphicsDevice.IsTransformFeedbackObjectsAvailable)
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
