using System;
using OpenTK;
using OpenTK.Graphics;
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
            GL.Enable(EnableCap.Multisample);

            Texture.Init();

            Console.WriteLine(MaxTextureSize);
        }

        /// <summary>
        /// Resets any internal library variable storing OpenGL states. These variables are used to, for example, dont bind the same texture twice if it's already bound.
        /// Calling this method calls any other reset states method
        /// You might want to reset this when interoperating with other libraries
        /// </summary>
        public static void ResetGLBindStates()
        {
            Texture.ResetBindStates();
            VertexDataBufferObject<int>.ResetBindState();
            VertexArray.ResetBindState();
        }

        public static int MaxTextureImageUnits { get { return GL.GetInteger(GetPName.MaxTextureImageUnits); } }

        public static int MaxSamples { get { return GL.GetInteger(GetPName.MaxSamples); } }

        public static int MaxTextureSize { get { return GL.GetInteger(GetPName.MaxTextureSize); } }
    }
}
