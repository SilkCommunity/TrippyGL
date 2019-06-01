using System;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    public static class TrippyLib
    {
        private static int glMinorVersion, glMajorVersion;

        /// <summary>
        /// Initiates the TrippyGL library. This should be called after the context has been created and on the same thread
        /// </summary>
        public static void Init()
        {
            if (isLibActive)
                throw new InvalidOperationException("The library is already active!");
            RecheckGLVersion();

            isLibActive = true;
            GL.Enable(EnableCap.Multisample);

            Texture.Init();
            BufferObject.Init();
            UniformBufferObject<int>.Init1();

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

        /// <summary>
        /// Checks the OpenGL versions for the library. You shouldn't need to call this though
        /// </summary>
        public static void RecheckGLVersion()
        {
            glMajorVersion = GL.GetInteger(GetPName.MajorVersion);
            glMinorVersion = GL.GetInteger(GetPName.MinorVersion);
            
            if (glMajorVersion < 3)
                throw new PlatformNotSupportedException("The OpenGL version must be at least 3.0");
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

        public static int UniformBufferOffsetAlignment { get { return GL.GetInteger(GetPName.UniformBufferOffsetAlignment); } }

        public static int MaxUniformBufferBindings { get { return GL.GetInteger(GetPName.MaxUniformBufferBindings); } }

        public static int MaxUniformBlockSize { get { return GL.GetInteger(GetPName.MaxUniformBlockSize); } }

        public static int MaxSamples { get { return GL.GetInteger(GetPName.MaxSamples); } }

        public static int MaxTextureSize { get { return GL.GetInteger(GetPName.MaxTextureSize); } }

        public static int GLMajorVersion { get { return glMajorVersion; } }

        public static int GLMinorVersion { get { return glMinorVersion; } }

        public static string GLVersion { get { return GL.GetString(StringName.Version); } }

        public static string GLVendor { get { return GL.GetString(StringName.Vendor); } }

        public static string GLRenderer { get { return GL.GetString(StringName.Renderer); } }

        public static string GLShadingLanguageVersion { get { return GL.GetString(StringName.ShadingLanguageVersion); } }
    }
}
