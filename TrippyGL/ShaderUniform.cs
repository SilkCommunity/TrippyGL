using System;
using System.Numerics;
using Silk.NET.OpenGL;

namespace TrippyGL
{
    /// <summary>
    /// Represents a shader uniform from a <see cref="ShaderProgram"/> and allows control over that uniform.
    /// </summary>
    public readonly struct ShaderUniform : IEquatable<ShaderUniform>
    {
        /// <summary>The name with which this uniform is declared on the <see cref="ShaderProgram"/>.</summary>
        public readonly string Name;

        /// <summary>The type of this uniform.</summary>
        public readonly UniformType UniformType;

        /// <summary>The <see cref="ShaderProgram"/> that contains this uniform.</summary>
        public readonly ShaderProgram OwnerProgram;

        /// <summary>The location of the uniform on the <see cref="ShaderProgram"/>. Used for setting the value.</summary>
        public readonly int UniformLocation;

        /// <summary>For array uniforms, this is the length of the array. 1 for non-array uniforms.</summary>
        public readonly int Size;

        /// <summary>Whether this <see cref="ShaderUniform"/> is of a sampler or sampler-array type.</summary>
        public readonly bool IsSamplerType;

        /// <summary>For sampler uniforms, the <see cref="Texture"/>/s the user set to this <see cref="ShaderUniform"/>.</summary>
        private readonly Texture[] textureValues;
        /// <summary>For sampler uniforms, the texture units last set as the uniform's value/s.</summary>
        private readonly int[] textureLastAppliedUnits;

        /// <summary>Whether this <see cref="ShaderUniform"/> instance has null values.</summary>
        public bool IsEmpty => OwnerProgram == null;

        /// <summary>
        /// Provides direct read-only access to this <see cref="ShaderUniform"/>'s set textures.
        /// These are not bound to the <see cref="GraphicsDevice"/> until needed.<para/>
        /// If this <see cref="ShaderUniform"/> is not of sampler type, accesing this will fail.
        /// </summary>
        public ReadOnlySpan<Texture> Textures => textureValues;

        internal ShaderUniform(ShaderProgram owner, int uniformLoc, string name, int size, UniformType type)
        {
            OwnerProgram = owner;
            UniformLocation = uniformLoc;
            Size = size;
            UniformType = type;

            // The name might come as array name for array uniforms.
            // We need to turn the name "arrayUniform[0]" into just "arrayUniform"
            int nameIndexOfThing = name.LastIndexOf('[');
            Name = nameIndexOfThing > 0 ? name.Substring(0, name.Length - nameIndexOfThing + 1) : name;

            IsSamplerType = TrippyUtils.IsUniformSamplerType(type);

            if (IsSamplerType)
            {
                textureValues = new Texture[size];
                textureLastAppliedUnits = new int[size];
                textureLastAppliedUnits.AsSpan().Fill(-1);
            }
            else
            {
                textureValues = null;
                textureLastAppliedUnits = null;
            }
        }

        public static bool operator ==(ShaderUniform left, ShaderUniform right) => left.Equals(right);

        public static bool operator !=(ShaderUniform left, ShaderUniform right) => !left.Equals(right);

        #region SetValue1
        public void SetValueFloat(float value)
        {
            ValidateUniformType(UniformType.Float);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform1(UniformLocation, value);
        }
        public void SetValueDouble(double value)
        {
            ValidateUniformType(UniformType.Double);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform1(UniformLocation, value);
        }
        public void SetValueInt(int value)
        {
            ValidateUniformType(UniformType.Int);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform1(UniformLocation, value);
        }
        public void SetValueUint(uint value)
        {
            ValidateUniformType(UniformType.UnsignedInt);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform1(UniformLocation, value);
        }
        public void SetValueBool(bool value)
        {
            ValidateUniformType(UniformType.Bool);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform1(UniformLocation, value ? (int)GLEnum.True : (int)GLEnum.False);
        }
        #endregion

        #region SetValue2
        public void SetValueVec2(float x, float y)
        {
            ValidateUniformType(UniformType.FloatVec2);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform2(UniformLocation, x, y);
        }
        public void SetValueDVec2(double x, double y)
        {
            ValidateUniformType(UniformType.DoubleVec2);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform2(UniformLocation, x, y);
        }
        public void SetValueIVec2(int x, int y)
        {
            ValidateUniformType(UniformType.IntVec2);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform2(UniformLocation, x, y);
        }
        public void SetValueUVec2(uint x, uint y)
        {
            ValidateUniformType(UniformType.UnsignedIntVec2);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform2(UniformLocation, x, y);
        }
        public void SetValueBVec2(bool x, bool y)
        {
            ValidateUniformType(UniformType.BoolVec2);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform2(UniformLocation, x ? (int)GLEnum.True : (int)GLEnum.False, y ? (int)GLEnum.True : (int)GLEnum.False);
        }

        public void SetValueVec2(in Vector2 value)
        {
            SetValueVec2(value.X, value.Y);
        }
        #endregion

        #region SetValue3
        public void SetValueVec3(float x, float y, float z)
        {
            ValidateUniformType(UniformType.FloatVec3);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform3(UniformLocation, x, y, z);
        }
        public void SetValueDVec3(double x, double y, double z)
        {
            ValidateUniformType(UniformType.DoubleVec3);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform3(UniformLocation, x, y, z);
        }
        public void SetValueIVec3(int x, int y, int z)
        {
            ValidateUniformType(UniformType.IntVec3);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform3(UniformLocation, x, y, z);
        }
        public void SetValueUVec3(uint x, uint y, uint z)
        {
            ValidateUniformType(UniformType.UnsignedIntVec3);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform3(UniformLocation, x, y, z);
        }
        public void SetValueBVec3(bool x, bool y, bool z)
        {
            ValidateUniformType(UniformType.BoolVec3);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform3(UniformLocation,
                x ? (int)GLEnum.True : (int)GLEnum.False,
                y ? (int)GLEnum.True : (int)GLEnum.False,
                z ? (int)GLEnum.True : (int)GLEnum.False
            );
        }

        public void SetValueVec3(in Vector3 value)
        {
            SetValueVec3(value.X, value.Y, value.Z);
        }
        #endregion

        #region SetValue4
        public void SetValueVec4(float x, float y, float z, float w)
        {
            ValidateUniformType(UniformType.FloatVec4);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform4(UniformLocation, x, y, z, w);
        }
        public void SetValueDVec4(double x, double y, double z, double w)
        {
            ValidateUniformType(UniformType.DoubleVec4);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform4(UniformLocation, x, y, z, w);
        }
        public void SetValueIVec4(int x, int y, int z, int w)
        {
            ValidateUniformType(UniformType.IntVec4);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform4(UniformLocation, x, y, z, w);
        }
        public void SetValueUVec4(uint x, uint y, uint z, uint w)
        {
            ValidateUniformType(UniformType.UnsignedIntVec4);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform4(UniformLocation, x, y, z, w);
        }
        public void SetValueBVec4(bool x, bool y, bool z, bool w)
        {
            ValidateUniformType(UniformType.BoolVec4);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform4(UniformLocation,
                x ? (int)GLEnum.True : (int)GLEnum.False,
                y ? (int)GLEnum.True : (int)GLEnum.False,
                z ? (int)GLEnum.True : (int)GLEnum.False,
                w ? (int)GLEnum.True : (int)GLEnum.False
            );
        }

        public void SetValueVec4(in Vector4 value)
        {
            SetValueVec4(value.X, value.Y, value.Z, value.W);
        }
        public void SetValueVec4(Color4b value)
        {
            SetValueVec4(value.R / 255f, value.G / 255f, value.B / 255f, value.A / 255f);
        }
        public void SetValueVec4(in Quaternion value)
        {
            SetValueVec4(value.X, value.Y, value.Z, value.W);
        }
        #endregion

        #region SetValueSamplers
        public void SetValueTexture(Texture texture, int uniformIndex = 0)
        {
            ValidateIsSampler();

            if (uniformIndex < 0 || uniformIndex >= Size)
                throw new ArgumentOutOfRangeException(nameof(uniformIndex), nameof(uniformIndex) + " must be in the range [0, " + nameof(Size) + ")");

            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

            if (textureValues[uniformIndex] != texture)
            {
                textureValues[uniformIndex] = texture;
                OwnerProgram.areSamplerUniformsDirty = true;
            }
        }

        public void SetValueTextureArray(ReadOnlySpan<Texture> textures, int startUniformIndex = 0)
        {
            ValidateIsSampler();

            if (startUniformIndex < 0 || startUniformIndex >= Size)
                throw new ArgumentOutOfRangeException(nameof(startUniformIndex), nameof(startUniformIndex) + " must be in the range [0, " + nameof(Size) + ")");

            if (startUniformIndex + textures.Length > Size)
                throw new ArgumentOutOfRangeException("Tried to set too many textures");

            bool isDirty = false;
            for (int i = 0; i < textures.Length; i++)
            {
                int uniformIndex = startUniformIndex + i;
                if (textureValues[uniformIndex] != textures[i])
                {
                    textureValues[uniformIndex] = textures[i];
                    isDirty = true;
                }
            }

            OwnerProgram.areSamplerUniformsDirty |= isDirty;
        }
        #endregion

        #region SetValueMat2
        public unsafe void SetValueMat2Ptr(float* ptr, int count = 1, bool transpose = false)
        {
            ValidateArrayAndType(UniformType.FloatMat2, count);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.UniformMatrix2(UniformLocation, (uint)count, transpose, ptr);
        }
        public unsafe void SetValueDMat2Ptr(double* ptr, int count = 1, bool transpose = false)
        {
            ValidateArrayAndType(UniformType.DoubleMat2, count);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.UniformMatrix2(UniformLocation, (uint)count, transpose, ptr);
        }
        public unsafe void SetValueMat2x4Ptr(float* ptr, int count = 1, bool transpose = false)
        {
            ValidateArrayAndType(UniformType.FloatMat2x4, count);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.UniformMatrix2x4(UniformLocation, (uint)count, transpose, ptr);
        }
        public unsafe void SetValueDMat2x4Ptr(double* ptr, int count = 1, bool transpose = false)
        {
            ValidateArrayAndType(UniformType.DoubleMat2x4, count);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.UniformMatrix2x4(UniformLocation, (uint)count, transpose, ptr);
        }
        public unsafe void SetValueMat2x3Ptr(float* ptr, int count = 1, bool transpose = false)
        {
            ValidateArrayAndType(UniformType.FloatMat2x3, count);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.UniformMatrix2x3(UniformLocation, (uint)count, transpose, ptr);
        }
        public unsafe void SetValueDMat2x3Ptr(double* ptr, int count = 1, bool transpose = false)
        {
            ValidateArrayAndType(UniformType.DoubleMat2x3, count);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.UniformMatrix2x3(UniformLocation, (uint)count, transpose, ptr);
        }

        public unsafe void SetValueMat2(ReadOnlySpan<float> value, bool transpose = false)
        {
            fixed (float* ptr = value)
                SetValueMat2Ptr(ptr, value.Length / 4, transpose);
        }
        public unsafe void SetValueDMat2(ReadOnlySpan<double> value, bool transpose = false)
        {
            fixed (double* ptr = value)
                SetValueDMat2Ptr(ptr, value.Length / 4, transpose);
        }
        public unsafe void SetValueMat2x4(ReadOnlySpan<float> value, bool transpose = false)
        {
            fixed (float* ptr = value)
                SetValueMat2x4Ptr(ptr, value.Length / 8, transpose);
        }
        public unsafe void SetValueDMat2x4(ReadOnlySpan<double> value, bool transpose = false)
        {
            fixed (double* ptr = value)
                SetValueDMat2x4Ptr(ptr, value.Length / 8, transpose);
        }
        public unsafe void SetValueMat2x3(ReadOnlySpan<float> value, bool transpose = false)
        {
            fixed (float* ptr = value)
                SetValueMat2x3Ptr(ptr, value.Length / 6, transpose);
        }
        public unsafe void SetValueDMat2x3(ReadOnlySpan<double> value, bool transpose = false)
        {
            fixed (double* ptr = value)
                SetValueDMat2x3Ptr(ptr, value.Length / 6, transpose);
        }
        #endregion

        #region SetValueMat3xX
        public unsafe void SetValueMat3Ptr(float* ptr, int count = 1, bool transpose = false)
        {
            ValidateArrayAndType(UniformType.FloatMat3, count);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.UniformMatrix3(UniformLocation, (uint)count, transpose, ptr);
        }
        public unsafe void SetValueDMat3Ptr(double* ptr, int count = 1, bool transpose = false)
        {
            ValidateArrayAndType(UniformType.DoubleMat3, count);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.UniformMatrix3(UniformLocation, (uint)count, transpose, ptr);
        }
        public unsafe void SetValueMat3x4Ptr(float* ptr, int count = 1, bool transpose = false)
        {
            ValidateArrayAndType(UniformType.FloatMat3x4, count);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.UniformMatrix3x4(UniformLocation, (uint)count, transpose, ptr);
        }
        public unsafe void SetValueDMat3x4Ptr(double* ptr, int count = 1, bool transpose = false)
        {
            ValidateArrayAndType(UniformType.DoubleMat3x4, count);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.UniformMatrix3x4(UniformLocation, (uint)count, transpose, ptr);
        }
        public unsafe void SetValueMat3x2Ptr(float* ptr, int count = 1, bool transpose = false)
        {
            ValidateArrayAndType(UniformType.FloatMat3x2, count);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.UniformMatrix3x2(UniformLocation, (uint)count, transpose, ptr);
        }
        public unsafe void SetValueDMat3x2Ptr(double* ptr, int count = 1, bool transpose = false)
        {
            ValidateArrayAndType(UniformType.DoubleMat3x2, count);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.UniformMatrix3x2(UniformLocation, (uint)count, transpose, ptr);
        }

        public unsafe void SetValueMat3(ReadOnlySpan<float> value, bool transpose = false)
        {
            fixed (float* ptr = value)
                SetValueMat3Ptr(ptr, value.Length / 9, transpose);
        }
        public unsafe void SetValueDMat3(ReadOnlySpan<double> value, bool transpose = false)
        {
            fixed (double* ptr = value)
                SetValueDMat3Ptr(ptr, value.Length / 9, transpose);
        }
        public unsafe void SetValueMat3x4(ReadOnlySpan<float> value, bool transpose = false)
        {
            fixed (float* ptr = value)
                SetValueMat3x4Ptr(ptr, value.Length / 12, transpose);
        }
        public unsafe void SetValueDMat3x4(ReadOnlySpan<double> value, bool transpose = false)
        {
            fixed (double* ptr = value)
                SetValueDMat3x4Ptr(ptr, value.Length / 12, transpose);
        }
        public unsafe void SetValueMat3x2(ReadOnlySpan<float> value, bool transpose = false)
        {
            fixed (float* ptr = value)
                SetValueMat3x2Ptr(ptr, value.Length / 6, transpose);
        }
        public unsafe void SetValueDMat3x2(ReadOnlySpan<double> value, bool transpose = false)
        {
            fixed (double* ptr = value)
                SetValueDMat3x2Ptr(ptr, value.Length / 6, transpose);
        }

        public unsafe void SetValueMat3x2(in Matrix3x2 value, bool transpose = false)
        {
            fixed (float* ptr = &value.M11)
                SetValueMat3x2Ptr(ptr, 1, transpose);
        }
        public unsafe void SetValueMat3x2Array(ReadOnlySpan<Matrix3x2> value, bool transpose)
        {
            fixed (float* ptr = &value[0].M11)
                SetValueMat3x2Ptr(ptr, value.Length, transpose);
        }
        #endregion

        #region SetValueMat4xX
        public unsafe void SetValueMat4Ptr(float* ptr, int count = 1, bool transpose = false)
        {
            ValidateArrayAndType(UniformType.FloatMat4, count);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.UniformMatrix4(UniformLocation, (uint)count, transpose, ptr);
        }
        public unsafe void SetValueDMat4Ptr(double* ptr, int count = 1, bool transpose = false)
        {
            ValidateArrayAndType(UniformType.DoubleMat4, count);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.UniformMatrix4(UniformLocation, (uint)count, transpose, ptr);
        }
        public unsafe void SetValueMat4x3Ptr(float* ptr, int count = 1, bool transpose = false)
        {
            ValidateArrayAndType(UniformType.FloatMat4x3, count);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.UniformMatrix4x3(UniformLocation, (uint)count, transpose, ptr);
        }
        public unsafe void SetValueDMat4x3Ptr(double* ptr, int count = 1, bool transpose = false)
        {
            ValidateArrayAndType(UniformType.DoubleMat4x3, count);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.UniformMatrix4x3(UniformLocation, (uint)count, transpose, ptr);
        }
        public unsafe void SetValueMat4x2Ptr(float* ptr, int count = 1, bool transpose = false)
        {
            ValidateArrayAndType(UniformType.FloatMat4x2, count);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.UniformMatrix4x2(UniformLocation, (uint)count, transpose, ptr);
        }
        public unsafe void SetValueDMat4x2Ptr(double* ptr, int count = 1, bool transpose = false)
        {
            ValidateArrayAndType(UniformType.DoubleMat4x2, count);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.UniformMatrix4x2(UniformLocation, (uint)count, transpose, ptr);
        }

        public unsafe void SetValueMat4(ReadOnlySpan<float> value, bool transpose = false)
        {
            fixed (float* ptr = value)
                SetValueMat4Ptr(ptr, value.Length / 16, transpose);
        }
        public unsafe void SetValueDMat4(ReadOnlySpan<double> value, bool transpose = false)
        {
            fixed (double* ptr = value)
                SetValueDMat4Ptr(ptr, value.Length / 16, transpose);
        }
        public unsafe void SetValueMat4x3(ReadOnlySpan<float> value, bool transpose = false)
        {
            fixed (float* ptr = value)
                SetValueMat4x3Ptr(ptr, value.Length / 12, transpose);
        }
        public unsafe void SetValueDMat4x3(ReadOnlySpan<double> value, bool transpose = false)
        {
            fixed (double* ptr = value)
                SetValueDMat4x3Ptr(ptr, value.Length / 12, transpose);
        }
        public unsafe void SetValueMat4x2(ReadOnlySpan<float> value, bool transpose = false)
        {
            fixed (float* ptr = value)
                SetValueMat4x2Ptr(ptr, value.Length / 8, transpose);
        }
        public unsafe void SetValueDMat4x2(ReadOnlySpan<double> value, bool transpose = false)
        {
            fixed (double* ptr = value)
                SetValueDMat4x2Ptr(ptr, value.Length / 8, transpose);
        }

        public unsafe void SetValueMat4(in Matrix4x4 value, bool transpose = false)
        {
            fixed (float* ptr = &value.M11)
                SetValueMat4Ptr(ptr, 1, transpose);
        }
        public unsafe void SetValueMat4Array(ReadOnlySpan<Matrix4x4> value, bool transpose = false)
        {
            fixed (float* ptr = &value[0].M11)
                SetValueMat4Ptr(ptr, value.Length, transpose);

        }
        #endregion

        #region SetValue1Array
        public unsafe void SetValue1ArrayPtr(float* ptr, int count)
        {
            ValidateArrayAndType(UniformType.Float, count);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform1(UniformLocation, (uint)count, ptr);
        }
        public unsafe void SetValue1ArrayPtr(double* ptr, int count)
        {
            ValidateArrayAndType(UniformType.Double, count);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform1(UniformLocation, (uint)count, ptr);
        }
        public unsafe void SetValue1ArrayPtr(int* ptr, int count)
        {
            ValidateArrayAndType(UniformType.Int, count);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform1(UniformLocation, (uint)count, ptr);
        }
        public unsafe void SetValue1ArrayPtr(uint* ptr, int count)
        {
            ValidateArrayAndType(UniformType.UnsignedInt, count);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform1(UniformLocation, (uint)count, ptr);
        }

        public unsafe void SetValue1Array(ReadOnlySpan<float> value)
        {
            fixed (float* ptr = value)
                SetValue1ArrayPtr(ptr, value.Length);
        }
        public unsafe void SetValue1Array(ReadOnlySpan<double> value)
        {
            fixed (double* ptr = value)
                SetValue1ArrayPtr(ptr, value.Length);
        }
        public unsafe void SetValue1Array(ReadOnlySpan<int> value)
        {
            fixed (int* ptr = value)
                SetValue1ArrayPtr(ptr, value.Length);
        }
        public unsafe void SetValue1Array(ReadOnlySpan<uint> value)
        {
            fixed (uint* ptr = value)
                SetValue1ArrayPtr(ptr, value.Length);
        }
        #endregion

        #region SetValue2Array
        public unsafe void SetValue2ArrayPtr(float* ptr, int count)
        {
            ValidateArrayAndType(UniformType.FloatVec2, count);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform2(UniformLocation, (uint)count, ptr);
        }
        public unsafe void SetValue2ArrayPtr(double* ptr, int count)
        {
            ValidateArrayAndType(UniformType.DoubleVec2, count);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform2(UniformLocation, (uint)count, ptr);
        }
        public unsafe void SetValue2ArrayPtr(int* ptr, int count)
        {
            ValidateArrayAndType(UniformType.IntVec2, count);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform2(UniformLocation, (uint)count, ptr);
        }
        public unsafe void SetValue2ArrayPtr(uint* ptr, int count)
        {
            ValidateArrayAndType(UniformType.UnsignedIntVec2, count);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform2(UniformLocation, (uint)count, ptr);
        }

        public unsafe void SetValue2Array(ReadOnlySpan<Vector2> value)
        {
            fixed (float* ptr = &value[0].X)
                SetValue2ArrayPtr(ptr, value.Length);
        }

        public unsafe void SetValue2Array(ReadOnlySpan<float> value)
        {
            fixed (float* ptr = value)
                SetValue2ArrayPtr(ptr, value.Length / 2);
        }
        public unsafe void SetValue2Array(ReadOnlySpan<double> value)
        {
            fixed (double* ptr = value)
                SetValue2ArrayPtr(ptr, value.Length / 2);
        }
        public unsafe void SetValue2Array(ReadOnlySpan<int> value)
        {
            fixed (int* ptr = value)
                SetValue2ArrayPtr(ptr, value.Length / 2);
        }
        public unsafe void SetValue2Array(ReadOnlySpan<uint> value)
        {
            fixed (uint* ptr = value)
                SetValue2ArrayPtr(ptr, value.Length / 2);
        }
        #endregion

        #region SetValue3Array
        public unsafe void SetValue3ArrayPtr(float* ptr, int count)
        {
            ValidateArrayAndType(UniformType.FloatVec3, count);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform3(UniformLocation, (uint)count, ptr);
        }
        public unsafe void SetValue3ArrayPtr(double* ptr, int count)
        {
            ValidateArrayAndType(UniformType.DoubleVec3, count);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform3(UniformLocation, (uint)count, ptr);
        }
        public unsafe void SetValue3ArrayPtr(int* ptr, int count)
        {
            ValidateArrayAndType(UniformType.IntVec3, count);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform3(UniformLocation, (uint)count, ptr);
        }
        public unsafe void SetValue3ArrayPtr(uint* ptr, int count)
        {
            ValidateArrayAndType(UniformType.UnsignedIntVec3, count);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform3(UniformLocation, (uint)count, ptr);
        }

        public unsafe void SetValue3Array(ReadOnlySpan<Vector3> value)
        {
            fixed (float* ptr = &value[0].X)
                SetValue3ArrayPtr(ptr, value.Length);
        }

        public unsafe void SetValue3Array(ReadOnlySpan<float> value)
        {
            fixed (float* ptr = value)
                SetValue3ArrayPtr(ptr, value.Length / 3);
        }
        public unsafe void SetValue3Array(ReadOnlySpan<double> value)
        {
            fixed (double* ptr = value)
                SetValue3ArrayPtr(ptr, value.Length / 3);
        }
        public unsafe void SetValue3Array(ReadOnlySpan<int> value)
        {
            fixed (int* ptr = value)
                SetValue3ArrayPtr(ptr, value.Length / 3);
        }
        public unsafe void SetValue3Array(ReadOnlySpan<uint> value)
        {
            fixed (uint* ptr = value)
                SetValue3ArrayPtr(ptr, value.Length / 3);
        }
        #endregion

        #region SetValue4Array
        public unsafe void SetValue4ArrayPtr(float* ptr, int count)
        {
            ValidateArrayAndType(UniformType.FloatVec4, count);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform4(UniformLocation, (uint)count, ptr);
        }
        public unsafe void SetValue4ArrayPtr(double* ptr, int count)
        {
            ValidateArrayAndType(UniformType.DoubleVec4, count);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform4(UniformLocation, (uint)count, ptr);
        }
        public unsafe void SetValue4ArrayPtr(int* ptr, int count)
        {
            ValidateArrayAndType(UniformType.IntVec4, count);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform4(UniformLocation, (uint)count, ptr);
        }
        public unsafe void SetValue4ArrayPtr(uint* ptr, int count)
        {
            ValidateArrayAndType(UniformType.UnsignedIntVec4, count);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform4(UniformLocation, (uint)count, ptr);
        }

        public unsafe void SetValue4Array(ReadOnlySpan<Vector4> value)
        {
            fixed (float* ptr = &value[0].X)
                SetValue4ArrayPtr(ptr, value.Length);
        }

        public unsafe void SetValue4Array(ReadOnlySpan<float> value)
        {
            fixed (float* ptr = value)
                SetValue4ArrayPtr(ptr, value.Length / 4);
        }
        public unsafe void SetValue4Array(ReadOnlySpan<double> value)
        {
            fixed (double* ptr = value)
                SetValue4ArrayPtr(ptr, value.Length / 4);
        }
        public unsafe void SetValue4Array(ReadOnlySpan<int> value)
        {
            fixed (int* ptr = value)
                SetValue4ArrayPtr(ptr, value.Length / 4);
        }
        public unsafe void SetValue4Array(ReadOnlySpan<uint> value)
        {
            fixed (uint* ptr = value)
                SetValue4ArrayPtr(ptr, value.Length / 4);
        }
        #endregion

        /// <summary>
        /// This is called by <see cref="ShaderUniformList.EnsureSamplerUniformsSet"/> after all
        /// the required sampler uniform textures have been bound to different texture units.<para/>
        /// This method assumes that the <see cref="textureValues"/> textures are all bound to
        /// texture units and ready to be used. This method also assumes that the
        /// <see cref="ShaderProgram"/> that owns this uniform is the one currently in use.
        /// </summary>
        internal void ApplyUniformTextureValues()
        {
            if (Size == 1)
            {
                int unit = textureValues[0] == null ? 0 : textureValues[0].lastBindUnit;
                textureLastAppliedUnits[0] = unit;
                OwnerProgram.GL.Uniform1(UniformLocation, unit);
            }
            else
            {
                bool isSetRequired = false;
                for (int i = 0; i < Size; i++)
                {
                    if (textureLastAppliedUnits[i] != textureValues[i].lastBindUnit)
                    {
                        textureLastAppliedUnits[i] = textureValues[i].lastBindUnit;
                        isSetRequired = true;
                    }
                }

                if (isSetRequired)
                    OwnerProgram.GL.Uniform1(UniformLocation, (uint)Size, textureLastAppliedUnits);
            }
        }

        /// <summary>
        /// Checks that <see cref="UniformType"/> is the correct type and throws an exception otherwise.
        /// </summary>
        private void ValidateUniformType(UniformType type)
        {
            if (IsEmpty)
                throw new NullReferenceException("This " + nameof(ShaderUniform) + " is null/empty");

            if (UniformType != type)
                throw new InvalidOperationException(string.Concat("Tried to set a uniform with an incorrect type. You tried to set a ", type.ToString(), " while the uniform's type was ", UniformType.ToString()));
        }

        /// <summary>
        /// Checks that <see cref="UniformType"/> is the correct type and valueLength is less
        /// than <see cref="Size"/>, and throws an exception otherwise.
        /// </summary>
        private void ValidateArrayAndType(UniformType type, int valueLength)
        {
            ValidateUniformType(type);

            if (valueLength > Size)
                throw new ArgumentOutOfRangeException("value.Length", valueLength, "You tried to set too many elements for this uniform");
        }

        private void ValidateIsSampler()
        {
            if (IsEmpty)
                throw new NullReferenceException("This " + nameof(ShaderUniform) + " is null/empty");

            if (!IsSamplerType)
                throw new InvalidOperationException("Tried to set a texture on a non-sampler uniform");
        }

        public override string ToString()
        {
            if (Size == 1)
                return string.Concat(UniformType.ToString(), " ", Name);
            return string.Concat(UniformType.ToString(), "[", Size.ToString(), "] ", Name);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = UniformLocation.GetHashCode();
                hashCode = (hashCode * 397) ^ Size.GetHashCode();
                hashCode = (hashCode * 397) ^ UniformType.GetHashCode();
                hashCode = (hashCode * 397) ^ OwnerProgram.GetHashCode();
                hashCode = (hashCode * 397) ^ Name.GetHashCode(StringComparison.InvariantCulture);
                return hashCode;
            }
        }

        public bool Equals(ShaderUniform other)
        {
            return OwnerProgram == other.OwnerProgram
                && UniformLocation == other.UniformLocation
                && Size == other.Size
                && UniformType == other.UniformType
                && Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            if (obj is ShaderUniform shaderUniform)
                return Equals(shaderUniform);
            return false;
        }
    }
}
