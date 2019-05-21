using System;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    public static class TrippyLib
    {
        /// <summary>
        /// Initiates the TrippyGL library. This should be called after the context has been created and on the same thread
        /// </summary>
        public static void Init()
        {
            if (isLibActive)
                throw new InvalidOperationException("The library is already active!");

            isLibActive = true;
            GL.Enable(EnableCap.Multisample);

            Texture.Init();
            BufferObject.Init();

            ResetGLBindStates();
        }

        /// <summary>
        /// Call this when quitting the TrippyGL library. This should be called before the context is destroyed
        /// </summary>
        public static void Quit()
        {
            if (!isLibActive)
                throw new InvalidOperationException("The library isn't active!");

            isLibActive = false;
        }
        
        internal static bool isLibActive;

        /// <summary>
        /// Resets any internal library variable storing OpenGL states. These variables are used to, for example, dont bind the same texture twice if it's already bound.
        /// Calling this method calls any other reset states method
        /// You might want to reset this when interoperating with other libraries
        /// </summary>
        public static void ResetGLBindStates()
        {
            BufferObject.ResetBindStates();
            Texture.ResetBindStates();
            VertexArray.ResetBindState();
        }

        public static int MaxTextureImageUnits { get { return GL.GetInteger(GetPName.MaxTextureImageUnits); } }

        public static int MaxSamples { get { return GL.GetInteger(GetPName.MaxSamples); } }

        public static int MaxTextureSize { get { return GL.GetInteger(GetPName.MaxTextureSize); } }

        public static int GLMajorVersion { get { return GL.GetInteger(GetPName.MajorVersion); } }

        public static int GLMinorVersion { get { return GL.GetInteger(GetPName.MinorVersion); } }
    }
}
