using System;

namespace TrippyGL
{
    /// <summary>
    /// Stores states used depth testing. The depth testing function can be configured to obtain
    /// different results, such as discarding faraway fragments or discarding the nearest ones.
    /// </summary>
    public sealed class DepthState : IEquatable<DepthState>
    {
        /// <summary>
        /// Whether depth testing is enabled for this depth state.
        /// If false, all other <see cref="DepthState"/> parameters are irrelevant.
        /// </summary>
        public bool DepthTestingEnabled;

        /// <summary>The depth value to use when clearing a depth buffer.</summary>
        public float ClearDepth;

        /// <summary>The function for comparing depth values to determine whether the new value passes the depth test.</summary>
        public DepthFunction DepthComparison;

        private double depthNear, depthFar;

        /// <summary>The near depth value for the depth's range. Must be in the [0, 1] range.</summary>
        public double DepthRangeNear
        {
            get => depthNear;
            set => depthNear = Math.Clamp(value, 0, 1);
        }

        /// <summary>The far depth value for the depth's range. Must be in the [0, 1] range.</summary>
        public double DepthRangeFar
        {
            get => depthFar;
            set => depthFar = Math.Clamp(value, 0, 1);
        }

        /// <summary>Whether the depth buffer will be written to when a depth check succeeds.</summary>
        public bool DepthBufferWrittingEnabled;

        /// <summary>
        /// Create a <see cref="DepthState"/> with the specified depth testing parameters.
        /// </summary>
        /// <param name="testingEnabled">Whether depth testing is enabled.</param>
        /// <param name="comparison">The comparison mode that determines when a depth test succeedes.</param>
        /// <param name="clearDepth">The depth value to set on a clear operation.</param>
        /// <param name="nearRange">The near depth value for the depth's range.</param>
        /// <param name="farRange">The far depth value for the depth's range.</param>
        /// <param name="depthBufferWrittingEnabled">Whether writting into the depth buffer is enabled.</param>
        public DepthState(bool testingEnabled, DepthFunction comparison = DepthFunction.Less, float clearDepth = 1,
            double nearRange = 0, double farRange = 1, bool depthBufferWrittingEnabled = true)
        {
            DepthTestingEnabled = testingEnabled;
            DepthComparison = comparison;
            ClearDepth = clearDepth;
            DepthRangeNear = nearRange;
            DepthRangeFar = farRange;
            DepthBufferWrittingEnabled = depthBufferWrittingEnabled;
        }

        /// <summary>
        /// Creates a new <see cref="DepthState"/> instance with the same values as this one.
        /// </summary>
        public DepthState Clone()
        {
            return new DepthState(DepthTestingEnabled, DepthComparison, ClearDepth, depthNear, depthFar, DepthBufferWrittingEnabled);
        }

        public override string ToString()
        {
            if (!DepthTestingEnabled)
                return "DepthState Disabled";

            return string.Concat("DepthState Enabled, ",
                nameof(DepthComparison) + "=\"", DepthComparison.ToString(), "\"",
                ", " + nameof(ClearDepth) + "=\"", ClearDepth.ToString(), "\"",
                ", DepthRange=[", depthNear.ToString(), ", ", depthFar.ToString(), "]",
                ", " + nameof(DepthBufferWrittingEnabled) + "=", DepthBufferWrittingEnabled.ToString()
            );
        }

        public bool Equals(DepthState? other)
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
        /// The default, most common <see cref="DepthState"/>.
        /// Checks that the depth of a fragment is closer to the camera or else, discards the fragment.
        /// </summary>
        public static DepthState Default => new DepthState(true);

        /// <summary>
        /// The default and most common <see cref="DepthState"/> but with an inverted
        /// comparison, so fragments will be discarded if they're closer to the camera.
        /// </summary>
        public static DepthState Inverted => new DepthState(true, DepthFunction.Greater, 0);

        /// <summary>
        /// Gets a <see cref="DepthState"/> with the same settings as <see cref="Default"/>,
        /// but writting to the depth buffer is disabled.
        /// </summary>
        public static DepthState ReadOnly => new DepthState(true, DepthFunction.Less, 1, 0, 1, false);

        /// <summary>
        /// Gets a <see cref="DepthState"/> with the same settings as <see cref="Inverted"/>,
        /// but writting to the depth buffer is disabled.
        /// </summary>
        public static DepthState ReadOnlyInverted => new DepthState(true, DepthFunction.Greater, 0, 0, 1, false);

        /// <summary>
        /// Gets a <see cref="DepthState"/> where no depth testing is done and all fragments are written.
        /// </summary>
        public static DepthState None => new DepthState(false);

        #endregion
    }
}
