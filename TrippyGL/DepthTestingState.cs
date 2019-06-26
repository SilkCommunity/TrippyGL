using System;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    /// <summary>
    /// Represents a way to use depth testing. The depth testing function can be configured to obtain
    /// different results, such as discarding faraway fragments or discarding the nearest ones
    /// </summary>
    public class DepthTestingState
    {
        /// <summary>Whether depth testing is enabled for this depth state. If false, all other DepthTestingState parameters are ignored</summary>
        public bool DepthTestingEnabled;

        /// <summary>The function for comparing depth values to determine whether the new value passes the depth test</summary>
        public DepthFunction DepthComparison;

        /// <summary>The depth value to use when clearing a depth buffer</summary>
        public float ClearDepth;

        internal double depthNear, depthFar;

        /// <summary>The near depth value for the depth's range. Must be in the [0, 1] range</summary>
        public double DepthRangeNear
        {
            get { return depthNear; }
            set { depthNear = MathHelper.Clamp(value, 0, 1); }
        }

        /// <summary>The far depth value for the depth's range. Must be in the [0, 1] range</summary>
        public double DepthRangeFar
        {
            get { return depthFar; }
            set { depthFar = MathHelper.Clamp(value, 0, 1); }
        }
        
        /// <summary>Whether the depth buffer will be written to when a depth check succeeds</summary>
        public bool IsDepthBufferWrittingEnabled;

        public DepthTestingState(bool testingEnabled, DepthFunction comparison = DepthFunction.Less, float clearDepth = 1, float nearRange = 0, float farRange = 1, bool depthBufferWrittingEnabled = true)
        {
            DepthTestingEnabled = testingEnabled;
            DepthComparison = comparison;
            ClearDepth = clearDepth;
            DepthRangeNear = nearRange;
            DepthRangeFar = farRange;
            IsDepthBufferWrittingEnabled = depthBufferWrittingEnabled;
        }

        #region Static Members

        /// <summary>The default, most common DepthTestingState. Checks that the depth of a fragment is closer to the camera or else, discards the fragment</summary>
        public static DepthTestingState Default { get { return new DepthTestingState(true); } }

        /// <summary>The default, mos common DepthTestingState but with an inverted comparison, so fragments will be discarded if they're closer to the camera</summary>
        public static DepthTestingState Inverted { get { return new DepthTestingState(true, DepthFunction.Greater, 0); } }

        /// <summary>Gets a DepthTestingState where no depth testing is done and all fragments are written</summary>
        public static DepthTestingState None { get { return new DepthTestingState(false); } }

        #endregion
    }
}
