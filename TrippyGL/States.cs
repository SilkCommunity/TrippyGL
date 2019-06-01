using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    public static class States
    {

        #region Buffer Object bindings

        private static List<int> bufferBindings;
        private static List<BufferTarget> bufferBindingTargets;

        public static void EnsureBufferBound()
        {

        }

        #endregion Buffer Object bindings

        private static int vertexArrayBinding;
    }
}
