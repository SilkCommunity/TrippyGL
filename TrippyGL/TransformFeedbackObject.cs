using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
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

        public TransformFeedbackObject(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {
            Handle = graphicsDevice.IsTransformFeedbackObjectsAvailable ? GL.GenTransformFeedback() : -1;
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

        internal void PerformBindOperation()
        {
            if (GraphicsDevice.TransformFeedback != null && GraphicsDevice.TransformFeedback.IsActive)
                throw new InvalidOperationException("A TransformFeedbackObject cannot be bound if the current one is in an active feedback operation");

            if (Handle == -1)
            {
                // Bind for non-feedback-objects
            }
            else
                GL.BindTransformFeedback(TransformFeedbackTarget.TransformFeedback, Handle);
        }

        internal static void PerformUnbindOperation(GraphicsDevice device)
        {
            if (device.TransformFeedback != null && device.TransformFeedback.IsActive)
                throw new InvalidOperationException("A TransformFeedbackObject cannot be unbound if the current one is in an active feedback operation");

            if (device.IsTransformFeedbackObjectsAvailable)
                GL.BindTransformFeedback(TransformFeedbackTarget.TransformFeedback, 0);
            else
            {
                // Unbind for non-feedback-objects
            }
        }
    }
}
