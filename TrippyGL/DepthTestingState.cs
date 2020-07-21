using System;
using Silk.NET.OpenGL;

namespace TrippyGL
{
    /// <summary>
    /// Represents a way to use depth testing. The depth testing function can be configured to obtain
    /// different results, such as discarding faraway fragments or discarding the nearest ones.
    /// </summary>
    public sealed class DepthTestingState : IEquatable<DepthTestingState>
    {
        /// <summary>
        /// Whether depth testing is enabled for this depth state.
        /// If false, all other <see cref="DepthTestingState"/> parameters are irrelevant.
        /// </summary>
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
            set { depthNear = Math.Clamp(value, 0, 1); }
        }

        /// <summary>The far depth value for the depth's range. Must be in the [0, 1] range.</summary>
        public double DepthRangeFar
        {
            get { return depthFar; }
            set { depthFar = Math.Clamp(value, 0, 1); }
        }

        /// <summary>Whether the depth buffer will be written to when a depth check succeeds.</summary>
        public bool DepthBufferWrittingEnabled;

        /// <summary>
        /// Create a <see cref="DepthTestingState"/> with the specified depth testing parameters.
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
        /// Creates a new <see cref="BlendState"/> instance with the same values as this one.
        /// </summary>
        public DepthTestingState Clone()
        {
            return new DepthTestingState(DepthTestingEnabled, DepthComparison, ClearDepth, depthNear, depthFar, DepthBufferWrittingEnabled);
        }

        public override string ToString()
        {
            if (!DepthTestingEnabled)
                return "Disabled";

            return string.Concat("Enabled, ",
                nameof(DepthComparison) + "=\"", DepthComparison.ToString(), "\"",
                ", " + nameof(ClearDepth) + "=\"", ClearDepth.ToString(), "\"",
                ", DepthRange=[", depthNear.ToString(), ", ", depthFar.ToString(), "]",
                ", " + nameof(DepthBufferWrittingEnabled) + "=", DepthBufferWrittingEnabled.ToString()
            );
        }

        public bool Equals(DepthTestingState other)
        {
            return other != null
                && DepthTestingEnabled == other.DepthTestingEnabled
                && DepthComparison == other.DepthComparison
                && ClearDepth == other.ClearDepth
                && depthNear == other.depthNear
                && depthFar == other.depthFar
                && DepthBufferWrittingEnabled == other.DepthBufferWrittingEnabled;
        }

        #region Static Members

        /// <summary>
        /// The default, most common <see cref="DepthTestingState"/>.
        /// Checks that the depth of a fragment is closer to the camera or else, discards the fragment.
        /// </summary>
        public static DepthTestingState Default => new DepthTestingState(true);

        /// <summary>
        /// The default and most common <see cref="DepthTestingState"/> but with an inverted
        /// comparison, so fragments will be discarded if they're closer to the camera.
        /// </summary>
        public static DepthTestingState Inverted => new DepthTestingState(true, DepthFunction.Greater, 0);

        /// <summary>
        /// Gets a <see cref="DepthTestingState"/> with the same settings as <see cref="Default"/>,
        /// but writting to the depth buffer is disabled.
        /// </summary>
        public static DepthTestingState ReadOnly => new DepthTestingState(true, DepthFunction.Less, 1, 0, 1, false);

        /// <summary>
        /// Gets a <see cref="DepthTestingState"/> with the same settings as <see cref="Inverted"/>,
        /// but writting to the depth buffer is disabled.
        /// </summary>
        public static DepthTestingState ReadOnlyInverted => new DepthTestingState(true, DepthFunction.Greater, 0, 0, 1, false);

        /// <summary>Gets a <see cref="DepthTestingState"/> where no depth testing is done and all fragments are written.</summary>
        public static DepthTestingState None => new DepthTestingState(false);

        #endregion
    }
}
