using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    /// <summary>
    /// Represents a way to use depth testing. The depth testing function can be configured to obtain
    /// different results, such as discarding faraway fragments or discarding the nearest ones.
    /// </summary>
    public class DepthTestingState
    {
        /// <summary>Whether depth testing is enabled for this depth state. If false, all other DepthTestingState parameters are irrelevant.</summary>
        public bool DepthTestingEnabled;

        /// <summary>The function for comparing depth values to determine whether the new value passes the depth test.</summary>
        public DepthFunction DepthComparison;

        /// <summary>The depth value to use when clearing a depth buffer.</summary>
        public float ClearDepth;

        private double depthNear, depthFar;

        /// <summary>The near depth value for the depth's range. Must be in the [0, 1] range.</summary>
        public double DepthRangeNear
        {
            get { return depthNear; }
            set { depthNear = MathHelper.Clamp(value, 0, 1); }
        }

        /// <summary>The far depth value for the depth's range. Must be in the [0, 1] range.</summary>
        public double DepthRangeFar
        {
            get { return depthFar; }
            set { depthFar = MathHelper.Clamp(value, 0, 1); }
        }

        /// <summary>Whether the depth buffer will be written to when a depth check succeeds.</summary>
        public bool DepthBufferWrittingEnabled;

        /// <summary>
        /// Create a DepthTestingState with the specified depth testing parameters.
        /// </summary>
        /// <param name="testingEnabled">Whether depth testing is enabled.</param>
        /// <param name="comparison">The comparison mode that determines when a depth test succeedes.</param>
        /// <param name="clearDepth">The depth value to set on a clear operation.</param>
        /// <param name="nearRange">The near depth value for the depth's range.</param>
        /// <param name="farRange">The far depth value for the depth's range.</param>
        /// <param name="depthBufferWrittingEnabled">Whether writting into the depth buffer is enabled.</param>
        public DepthTestingState(bool testingEnabled, DepthFunction comparison = DepthFunction.Less, float clearDepth = 1, double nearRange = 0, double farRange = 1, bool depthBufferWrittingEnabled = true)
        {
            DepthTestingEnabled = testingEnabled;
            DepthComparison = comparison;
            ClearDepth = clearDepth;
            DepthRangeNear = nearRange;
            DepthRangeFar = farRange;
            DepthBufferWrittingEnabled = depthBufferWrittingEnabled;
        }

        /// <summary>
        /// Creates a DepthTestingState with the same values as another specified DepthTestingState.
        /// </summary>
        /// <param name="copy">The DepthTestingState whose values to copy.</param>
        public DepthTestingState(DepthTestingState copy)
        {
            DepthTestingEnabled = copy.DepthTestingEnabled;
            DepthComparison = copy.DepthComparison;
            ClearDepth = copy.ClearDepth;
            depthNear = copy.depthNear;
            depthFar = copy.depthFar;
            DepthBufferWrittingEnabled = copy.DepthBufferWrittingEnabled;
        }

        public override string ToString()
        {
            if (!DepthTestingEnabled)
                return "Disabled";

            return string.Concat("DepthComparison=\"", DepthComparison.ToString(), "\", ClearDepth=\"", ClearDepth.ToString(), "\", DepthRange=[", depthNear.ToString(), ", ", depthFar.ToString(), "],  DepthBufferWrittingEnabled=", DepthBufferWrittingEnabled.ToString());
        }

        #region Static Members

        /// <summary>The default, most common DepthTestingState. Checks that the depth of a fragment is closer to the camera or else, discards the fragment.</summary>
        public static DepthTestingState Default { get { return new DepthTestingState(true); } }

        /// <summary>The default and most common DepthTestingState but with an inverted comparison, so fragments will be discarded if they're closer to the camera.</summary>
        public static DepthTestingState Inverted { get { return new DepthTestingState(true, DepthFunction.Greater, 0); } }

        /// <summary>Gets a DepthTestingState where no depth testing is done and all fragments are written.</summary>
        public static DepthTestingState None { get { return new DepthTestingState(false); } }

        #endregion
    }
}
