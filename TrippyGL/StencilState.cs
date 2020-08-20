using System;
using Silk.NET.OpenGL;

namespace TrippyGL
{
    /// <summary>
    /// Stores states used for stencil testing. The stencil testing function can be configured
    /// to obtain different results.
    /// </summary>
    public sealed class StencilState : IEquatable<StencilState>
    {
        /// <summary>A stencil mask in which all bits are turned on.</summary>
        public const uint FullMask = uint.MaxValue;

        /// <summary>A stencil mask in which all bits are turned off.</summary>
        public const uint EmptyMask = 0;

        /// <summary>
        /// Whether stencil testing is enabled for this stencil state.
        /// If false, all other <see cref="StencilState"/> parameters are irrelevant.
        /// </summary>
        public bool StencilTestingEnabled;

        /// <summary>The stencil value to use when clearing the stencil buffer.</summary>
        public int ClearStencil;

        /// <summary>The mask to apply when writting the stencil buffer from the front of a polygon.</summary>
        public uint FrontWriteMask;
        /// <summary>The mask to apply when writting the stencil buffer from the back of a polygon.</summary>
        public uint BackWriteMask;

        /// <summary>The function to use on the stencil test from the front of a polygon.</summary>
        public StencilFunction FrontFunction;
        /// <summary>The function to use on the stencil test from the back of a polygon.</summary>
        public StencilFunction BackFunction;

        /// <summary>A value to use for comparison during the stencil test from the front of a polygon.</summary>
        public int FrontRefValue;
        /// <summary>A value to use for comparison during the stencil test from the back of a polygon.</summary>
        public int BackRefValue;

        /// <summary>The mask to apply before comparison on the stencil test from the front of a polygon.</summary>
        public uint FrontTestMask;
        /// <summary>The mask to apply before comparison on the stencil test from the back of a polygon.</summary>
        public uint BackTestMask;

        /// <summary>The operation to do when a stencil test fails from the front of a polygon.</summary>
        public StencilOp FrontStencilFailOperation;
        /// <summary>The operation to do when a depth test fails from the front of a polygon.</summary>
        public StencilOp FrontDepthFailOperation;
        /// <summary>The operation to do when both the depth and stencil tests pass from the front of a polygon.</summary>
        public StencilOp FrontPassOperation;

        /// <summary>The operation to do when a stencil test fails from the back of a polygon.</summary>
        public StencilOp BackStencilFailOperation;
        /// <summary>The operation to do when a depth test fails from the back of a polygon.</summary>
        public StencilOp BackDepthFailOperation;
        /// <summary>The operation to do when both the depth and stencil tests pass from the back of a polygon.</summary>
        public StencilOp BackPassOperation;

        /// <summary>Sets the write mask for both front and back.</summary>
        public uint WriteMask
        {
            set
            {
                FrontWriteMask = value;
                BackWriteMask = value;
            }
        }

        /// <summary>Sets the stencil function for both front and back.</summary>
        public StencilFunction Function
        {
            set
            {
                FrontFunction = value;
                BackFunction = value;
            }
        }

        /// <summary>Sets the ref value for both front and back.</summary>
        public int RefValue
        {
            set
            {
                FrontRefValue = value;
                BackRefValue = value;
            }
        }

        /// <summary>Sets the test mask value for both front and back.</summary>
        public uint TestMask
        {
            set
            {
                FrontTestMask = value;
                BackTestMask = value;
            }
        }

        /// <summary>Sets the stencil fail operation for both front and back.</summary>
        public StencilOp StencilFailOperation
        {
            set
            {
                FrontStencilFailOperation = value;
                BackStencilFailOperation = value;
            }
        }

        /// <summary>Sets the depth fail operation for both front and back.</summary>
        public StencilOp DepthFailOperation
        {
            set
            {
                FrontDepthFailOperation = value;
                BackDepthFailOperation = value;
            }
        }

        /// <summary>Sets the depth and stencil pass operation for both front and back.</summary>
        public StencilOp PassOperation
        {
            set
            {
                FrontPassOperation = value;
                BackPassOperation = value;
            }
        }

        /// <summary>
        /// Creates an empty, zeroed-out <see cref="StencilState"/>.
        /// </summary>
        public StencilState()
        {

        }

        /// <summary>
        /// Creates a <see cref="StencilState"/> with the specified stencil testing parameters.
        /// </summary>
        /// <param name="testingEnabled">Whether stencil testing is enabled.</param>
        /// <param name="clearStencil">The stencil value to set on a clear operation.</param>
        /// <param name="writeMask">The mask to apply when writting to the stencil buffer.</param>
        /// <param name="function">The function to use for comparing stencil values.</param>
        /// <param name="refValue">A value used for comparison during the stencil test.</param>
        /// <param name="testMask">A mask applied before comparison during the stencil test.</param>
        /// <param name="sfail">The operation to do when a stencil test fails.</param>
        /// <param name="dpfail">The operation to do when a depth test fails.</param>
        /// <param name="dppass">The operation to do when both the depth and stencil tests pass.</param>
        public StencilState(bool testingEnabled, int clearStencil = 0, uint writeMask = FullMask,
            StencilFunction function = StencilFunction.Greater, int refValue = 0, uint testMask = FullMask,
            StencilOp sfail = StencilOp.Keep, StencilOp dpfail = StencilOp.Keep, StencilOp dppass = StencilOp.Keep)
        {
            StencilTestingEnabled = testingEnabled;
            ClearStencil = clearStencil;
            FrontWriteMask = writeMask;
            BackWriteMask = writeMask;
            FrontFunction = function;
            BackFunction = function;
            FrontRefValue = refValue;
            BackRefValue = refValue;
            FrontTestMask = testMask;
            BackTestMask = testMask;
            FrontStencilFailOperation = sfail;
            BackStencilFailOperation = sfail;
            FrontDepthFailOperation = dpfail;
            BackDepthFailOperation = dpfail;
            FrontPassOperation = dppass;
            BackPassOperation = dppass;
        }

        /// <summary>
        /// Creates a <see cref="StencilState"/> instance with the same values as this one.
        /// </summary>
        public StencilState Clone()
        {
            return new StencilState()
            {
                StencilTestingEnabled = StencilTestingEnabled,
                ClearStencil = ClearStencil,
                FrontWriteMask = FrontWriteMask,
                BackWriteMask = BackWriteMask,
                FrontFunction = FrontFunction,
                BackFunction = BackFunction,
                FrontRefValue = FrontRefValue,
                BackRefValue = BackRefValue,
                FrontTestMask = FrontTestMask,
                BackTestMask = BackTestMask,
                FrontStencilFailOperation = FrontStencilFailOperation,
                BackStencilFailOperation = BackStencilFailOperation,
                FrontDepthFailOperation = FrontDepthFailOperation,
                BackDepthFailOperation = BackDepthFailOperation,
                FrontPassOperation = FrontPassOperation,
                BackPassOperation = BackPassOperation
            };
        }

        public override string ToString()
        {
            return StencilTestingEnabled ? "Enabled StencilState" : "Disabled StencilState";
        }

        public bool Equals(StencilState other)
        {
            return other != null
                && StencilTestingEnabled == other.StencilTestingEnabled
                && ClearStencil == other.ClearStencil
                && FrontWriteMask == other.FrontWriteMask
                && BackWriteMask == other.BackWriteMask
                && FrontFunction == other.FrontFunction
                && BackFunction == other.BackFunction
                && FrontRefValue == other.FrontRefValue
                && BackRefValue == other.BackRefValue
                && FrontTestMask == other.FrontTestMask
                && BackTestMask == other.BackTestMask
                && FrontStencilFailOperation == other.FrontStencilFailOperation
                && FrontDepthFailOperation == other.FrontDepthFailOperation
                && FrontPassOperation == other.FrontPassOperation
                && BackStencilFailOperation == other.BackStencilFailOperation
                && BackDepthFailOperation == other.BackDepthFailOperation
                && BackPassOperation == other.BackPassOperation;
        }
    }
}
