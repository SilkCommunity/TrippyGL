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

        /// <summary>For array uniforms, this is the length of the array. 1 for non-arrays.</summary>
        public readonly int Size;

        /// <summary>Gets whether this <see cref="ShaderUniform"/> is of a sampler or sampler-array type.</summary>
        public readonly bool IsSamplerType;

        /// <summary>For sampler uniforms, the <see cref="Texture"/>/s the user set to this <see cref="ShaderUniform"/>.</summary>
        private readonly Texture[] textureValues;
        /// <summary>For sampler uniforms, the texture units last set as the uniform's value/s.</summary>
        private readonly int[] textureLastAppliedUnits;

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
        public void SetValue1(float value)
        {
            ValidateUniformType(UniformType.Float);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform1(UniformLocation, value);
        }

        public void SetValue1(double value)
        {
            ValidateUniformType(UniformType.Double);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform1(UniformLocation, value);
        }

        public void SetValue1(int value)
        {
            ValidateUniformType(UniformType.Int);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform1(UniformLocation, value);
        }
        public void SetValue1(uint value)
        {
            ValidateUniformType(UniformType.UnsignedInt);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform1(UniformLocation, value);
        }
        #endregion

        #region SetValue2
        public void SetValue2(ref Vector2 value)
        {
            ValidateUniformType(UniformType.FloatVec2);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform2(UniformLocation, ref value);
        }
        public void SetValue2(Vector2 value)
        {
            SetValue2(ref value);
        }
        public void SetValue2(float x, float y)
        {
            ValidateUniformType(UniformType.FloatVec2);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform2(UniformLocation, x, y);
        }

        public void SetValue2(double x, double y)
        {
            ValidateUniformType(UniformType.DoubleVec2);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform2(UniformLocation, x, y);
        }

        public void SetValue2(int x, int y)
        {
            ValidateUniformType(UniformType.IntVec2);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform2(UniformLocation, x, y);
        }
        public void SetValue2(uint x, uint y)
        {
            ValidateUniformType(UniformType.UnsignedIntVec2);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform2(UniformLocation, x, y);
        }
        #endregion

        #region SetValue3
        public void SetValue3(ref Vector3 value)
        {
            ValidateUniformType(UniformType.FloatVec3);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform3(UniformLocation, ref value);
        }
        public void SetValue3(Vector3 value)
        {
            SetValue3(ref value);
        }
        public void SetValue3(float x, float y, float z)
        {
            ValidateUniformType(UniformType.FloatVec3);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform3(UniformLocation, x, y, z);
        }

        public void SetValue3(double x, double y, double z)
        {
            ValidateUniformType(UniformType.DoubleVec3);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform3(UniformLocation, x, y, z);
        }

        public void SetValue3(int x, int y, int z)
        {
            ValidateUniformType(UniformType.IntVec3);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform3(UniformLocation, x, y, z);
        }
        public void SetValue3(uint x, uint y, uint z)
        {
            ValidateUniformType(UniformType.UnsignedIntVec3);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform3(UniformLocation, x, y, z);
        }
        #endregion

        #region SetValue4
        public void SetValue4(ref Vector4 value)
        {
            ValidateUniformType(UniformType.FloatVec4);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform4(UniformLocation, ref value);
        }
        public void SetValue4(Vector4 value)
        {
            SetValue4(ref value);
        }
        public void SetValue4(Color4b value)
        {
            ValidateUniformType(UniformType.FloatVec4);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform4(UniformLocation, value.R / 255f, value.G / 255f, value.B / 255f, value.A / 255f);
        }
        public void SetValue4(Quaternion value)
        {
            ValidateUniformType(UniformType.FloatVec4);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform4(UniformLocation, value);
        }
        public void SetValue4(float x, float y, float z, float w)
        {
            ValidateUniformType(UniformType.FloatVec4);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform4(UniformLocation, x, y, z, w);
        }
        public void SetValue4(double x, double y, double z, double w)
        {
            ValidateUniformType(UniformType.DoubleVec4);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform4(UniformLocation, x, y, z, w);
        }

        public void SetValue4(int x, int y, int z, int w)
        {
            ValidateUniformType(UniformType.IntVec4);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform4(UniformLocation, x, y, z, w);
        }
        public void SetValue4(uint x, uint y, uint z, uint w)
        {
            ValidateUniformType(UniformType.UnsignedIntVec4);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform4(UniformLocation, x, y, z, w);
        }
        #endregion

        #region SetValueSamplers
        public void SetValueTexture(Texture texture)
        {
            ValidateIsSampler();

            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

            if (textureValues[0] != texture)
            {
                textureValues[0] = texture;
                OwnerProgram.Uniforms.isTextureListDirty = true;
            }
        }

        public void SetValueTextureArray(Span<Texture> textures, int startUniformIndex = 0)
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

            OwnerProgram.Uniforms.isTextureListDirty |= isDirty;
        }
        #endregion

        #region SetValueMat2
        /*
        public void SetValueMat2(ref Matrix2 value, bool transpose = false)
        {
            ValidateUniformType(UniformType.FloatMat2);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.UniformMatrix2(UniformLocation, transpose, ref value);
        }
        public void SetValueMat2(ref Matrix2x3 value, bool transpose = false)
        {
            ValidateUniformType(UniformType.FloatMat2x3);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.UniformMatrix2x3(UniformLocation, 1, transpose, ref value.Row0.X);
        }
        public void SetValueMat2(ref Matrix2x4 value, bool transpose = false)
        {
            ValidateUniformType(UniformType.FloatMat2x4);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.UniformMatrix2x4(UniformLocation, 1, transpose, ref value.Row0.X);
        }

        public void SetValueMat2(ref Matrix2d value, bool transpose = false)
        {
            ValidateUniformType((UniformType)All.DoubleMat2);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.UniformMatrix2(UniformLocation, 1, transpose, ref value.Row0.X);
        }
        public void SetValueMat2(ref Matrix2x3d value, bool transpose = false)
        {
            ValidateUniformType((UniformType)All.DoubleMat2x3);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.UniformMatrix2x3(UniformLocation, 1, transpose, ref value.Row0.X);
        }
        public void SetValueMat2(ref Matrix2x4d value, bool transpose = false)
        {
            ValidateUniformType((UniformType)All.DoubleMat2x4);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.UniformMatrix2x4(UniformLocation, 1, transpose, ref value.Row0.X);
        }*/
        #endregion

        #region SetValueMat3
        /*public void SetValueMat3(ref Matrix3 value, bool transpose = false)
        {
            ValidateUniformType(UniformType.FloatMat3);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.UniformMatrix3(UniformLocation, transpose, ref value);
        }*/
        public unsafe void SetValueMat3(ref Matrix3x2 value, bool transpose = false)
        {
            ValidateUniformType(UniformType.FloatMat3x2);
            OwnerProgram.EnsureInUse();
            fixed (float* ptr = &value.M11)
                OwnerProgram.GL.UniformMatrix3x2(UniformLocation, 1, transpose, ptr);
        }
        /*public void SetValueMat3(ref Matrix3x4 value, bool transpose = false)
        {
            ValidateUniformType(UniformType.FloatMat3x4);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.UniformMatrix3x4(UniformLocation, 1, transpose, ref value.Row0.X);
        }

        public void SetValueMat3(ref Matrix3d value, bool transpose = false)
        {
            ValidateUniformType((UniformType)All.DoubleMat3);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.UniformMatrix3(UniformLocation, 1, transpose, ref value.Row0.X);
        }
        public void SetValueMat3(ref Matrix3x2d value, bool transpose = false)
        {
            ValidateUniformType((UniformType)All.DoubleMat3x2);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.UniformMatrix3x2(UniformLocation, 1, transpose, ref value.Row0.X);
        }
        public void SetValueMat3(ref Matrix3x4d value, bool transpose = false)
        {
            ValidateUniformType((UniformType)All.DoubleMat3x4);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.UniformMatrix3x4(UniformLocation, 1, transpose, ref value.Row0.X);
        }*/
        #endregion

        #region SetValueMat4
        public unsafe void SetValueMat4(ref Matrix4x4 value, bool transpose = false)
        {
            ValidateUniformType(UniformType.FloatMat4);

            OwnerProgram.EnsureInUse();
            fixed (float* ptr = &value.M11)
                OwnerProgram.GL.UniformMatrix4(UniformLocation, 1, transpose, ptr);
        }
        /*public void SetValueMat4(ref Matrix4x2 value, bool transpose = false)
        {
            ValidateUniformType(UniformType.FloatMat4x2);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.UniformMatrix4x2(UniformLocation, 1, transpose, ref value.Row0.X);
        }
        public void SetValueMat4(ref Matrix4x3 value, bool transpose = false)
        {
            ValidateUniformType(UniformType.FloatMat4x3);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.UniformMatrix4x3(UniformLocation, 1, transpose, ref value.Row0.X);
        }

        public void SetValueMat4(ref Matrix4d value, bool transpose = false)
        {
            ValidateUniformType((UniformType)All.DoubleMat4);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.UniformMatrix4(UniformLocation, 1, transpose, ref value.Row0.X);
        }
        public void SetValueMat4(ref Matrix4x2d value, bool transpose = false)
        {
            ValidateUniformType((UniformType)All.DoubleMat4x2);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.UniformMatrix4x2(UniformLocation, 1, transpose, ref value.Row0.X);
        }
        public void SetValueMat4(ref Matrix4x3d value, bool transpose = false)
        {
            ValidateUniformType((UniformType)All.DoubleMat4x3);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.UniformMatrix4x3(UniformLocation, 1, transpose, ref value.Row0.X);
        }*/
        #endregion

        #region SetValue1Array
        public unsafe void SetValue1Array(ReadOnlySpan<float> value)
        {
            ValidateArrayAndType(UniformType.Float, value.Length);
            OwnerProgram.EnsureInUse();
            fixed (float* ptr = &value[0])
                OwnerProgram.GL.Uniform1(UniformLocation, (uint)value.Length, ptr);
        }

        public unsafe void SetValue1Array(ReadOnlySpan<double> value)
        {
            ValidateArrayAndType(UniformType.Double, value.Length);
            OwnerProgram.EnsureInUse();
            fixed (double* ptr = &value[0])
                OwnerProgram.GL.Uniform1(UniformLocation, (uint)value.Length, ptr);
        }

        public unsafe void SetValue1Array(ReadOnlySpan<int> value)
        {
            ValidateArrayAndType(UniformType.Int, value.Length);
            OwnerProgram.EnsureInUse();
            fixed (int* ptr = &value[0])
                OwnerProgram.GL.Uniform1(UniformLocation, (uint)value.Length, ptr);
        }

        public unsafe void SetValue1Array(ReadOnlySpan<uint> value)
        {
            ValidateArrayAndType(UniformType.UnsignedInt, value.Length);
            OwnerProgram.EnsureInUse();
            fixed (uint* ptr = &value[0])
                OwnerProgram.GL.Uniform1(UniformLocation, (uint)value.Length, ptr);
        }
        #endregion

        #region SetValue2Array
        public unsafe void SetValue2Array(ReadOnlySpan<Vector2> value)
        {
            ValidateArrayAndType(UniformType.FloatVec2, value.Length);
            OwnerProgram.EnsureInUse();
            fixed (float* ptr = &value[0].X)
                OwnerProgram.GL.Uniform2(UniformLocation, (uint)value.Length, ptr);
        }

        /*public void SetValue2Array(Span<Vector2d> value)
        {
            ValidateArrayAndType(UniformType.DoubleVec2, value.Length);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform2(UniformLocation, value.Length, ref value[0].X);
        }*/
        #endregion

        #region SetValue3Array
        public unsafe void SetValue3Array(ReadOnlySpan<Vector3> value)
        {
            ValidateArrayAndType(UniformType.FloatVec3, value.Length);
            OwnerProgram.EnsureInUse();
            fixed (float* ptr = &value[0].X)
                OwnerProgram.GL.Uniform3(UniformLocation, (uint)value.Length, ptr);
        }

        /*public unsafe void SetValue3Array(ReadOnlySpan<Vector3d> value)
        {
            ValidateArrayAndType(UniformType.DoubleVec3, value.Length);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform3(UniformLocation, value.Length, ref value[0].X);
        }*/
        #endregion

        #region SetValue4Array
        public unsafe void SetValue4Array(ReadOnlySpan<Vector4> value)
        {
            ValidateArrayAndType(UniformType.FloatVec4, value.Length);
            OwnerProgram.EnsureInUse();
            fixed (float* ptr = &value[0].X)
                OwnerProgram.GL.Uniform4(UniformLocation, (uint)value.Length, ptr);
        }

        /*public unsafe void SetValue4Array(Span<Vector4d> value)
        {
            ValidateArrayAndType(UniformType.DoubleVec4, value.Length);
            OwnerProgram.EnsureInUse();
            OwnerProgram.GL.Uniform4(UniformLocation, value.Length, ref value[0].X);
        }*/
        #endregion

        /// <summary>
        /// This is called by <see cref="ShaderUniformList.EnsureSamplerUniformsSet"/> after all the required sampler uniform
        /// textures have been bound to different texture units.<para/>
        /// This method assumes that the <see cref="textureValues"/> textures are all bound to texture units and ready to be used.
        /// This method also assumes that the <see cref="ShaderProgram"/> that owns this uniform is the one currently in use.
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

        // TODO: Add missing SetValue-s

        // TODO: region SetValueMatrix2Array or god knows what the fuck WHY ARE THERE SO MANY SET VALUES
        // TODO: Should we validate array sets to make sure the indices are OK and such?
        // Also, Should we even check that the type is OK?

        /// <summary>
        /// Checks that <see cref="UniformType"/> is the correct type and throws an exception otherwise.
        /// </summary>
        private void ValidateUniformType(UniformType type)
        {
            if (UniformType != type)
                throw new InvalidOperationException(string.Concat("Tried to set a uniform with an incorrect type. You tried to set a ", type.ToString(), " while the uniform's type was ", UniformType.ToString()));
        }

        private void ValidateArrayAndType(UniformType type, int valueLength)
        {
            ValidateUniformType(type);

            if (valueLength > Size)
                throw new ArgumentOutOfRangeException("value.Length", valueLength, "You tried to set too many elements for this uniform");
        }

        private void ValidateIsSampler()
        {
            if (!IsSamplerType)
                throw new InvalidOperationException("Tried to set a texture on a non-sampler uniform");
        }

        public override string ToString()
        {
            return string.Concat(
                nameof(Name) + "=\"", Name, "\"",
                nameof(UniformType) + "=", UniformType.ToString()
            );
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = UniformLocation.GetHashCode();
                hashCode = (hashCode * 397) ^ Size.GetHashCode();
                hashCode = (hashCode * 397) ^ UniformType.GetHashCode();
                hashCode = (hashCode * 397) ^ OwnerProgram.GetHashCode();
                hashCode = (hashCode * 397) ^ Name.GetHashCode();
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
