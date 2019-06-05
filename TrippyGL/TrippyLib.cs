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
    }
}
