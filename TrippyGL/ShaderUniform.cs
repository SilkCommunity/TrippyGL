using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;

namespace TrippyGL
{
    /// <summary>
    /// Represents a shader uniform from a ShaderProgram and allows control over that uniform.
    /// </summary>
    public class ShaderUniform
    {
        /// <summary>The name with which this uniform is declared on the shader program.</summary>
        public readonly string Name;

        /// <summary>The type of uniform.</summary>
        public readonly ActiveUniformType UniformType;

        /// <summary>The program containing this uniform.</summary>
        public readonly ShaderProgram OwnerProgram;

        /// <summary>The location of the uniform on the program. Used for setting the value.</summary>
        public readonly int UniformLocation;

        /// <summary>For array uniforms, this is the length of the array. 1 for non-arrays.</summary>
        public readonly int Size;

        internal ShaderUniform(ShaderProgram owner, int uniformLoc, string name, int size, ActiveUniformType type)
        {
            OwnerProgram = owner;
            UniformLocation = uniformLoc;
            Name = name;
            Size = size;
            UniformType = type;
        }

        #region SetValue1
        public void SetValue1(float value)
        {
            ValidateType(ActiveUniformType.Float);
            OwnerProgram.EnsureInUse();
            GL.Uniform1(UniformLocation, value);
        }

        public void SetValue1(double value)
        {
            ValidateType(ActiveUniformType.Double);
            OwnerProgram.EnsureInUse();
            GL.Uniform1(UniformLocation, value);
        }

        public void SetValue1(int value)
        {
            ValidateType(ActiveUniformType.Int);
            OwnerProgram.EnsureInUse();
            GL.Uniform1(UniformLocation, value);
        }
        public void SetValue1(uint value)
        {
            ValidateType(ActiveUniformType.UnsignedInt);
            OwnerProgram.EnsureInUse();
            GL.Uniform1(UniformLocation, value);
        }
        #endregion

        #region SetValue2
        public void SetValue2(ref Vector2 value)
        {
            ValidateType(ActiveUniformType.FloatVec2);
            OwnerProgram.EnsureInUse();
            GL.Uniform2(UniformLocation, ref value);
        }
        public void SetValue2(Vector2 value)
        {
            SetValue2(ref value);
        }
        public void SetValue2(float x, float y)
        {
            ValidateType(ActiveUniformType.FloatVec2);
            OwnerProgram.EnsureInUse();
            GL.Uniform2(UniformLocation, x, y);
        }

        public void SetValue2(ref Vector2d value)
        {
            ValidateType(ActiveUniformType.DoubleVec2);
            OwnerProgram.EnsureInUse();
            GL.Uniform2(UniformLocation, value.X, value.Y);
        }
        public void SetValue2(Vector2d value)
        {
            SetValue2(ref value);
        }
        public void SetValue2(double x, double y)
        {
            ValidateType(ActiveUniformType.DoubleVec2);
            OwnerProgram.EnsureInUse();
            GL.Uniform2(UniformLocation, x, y);
        }

        public void SetValue2(int x, int y)
        {
            ValidateType(ActiveUniformType.IntVec2);
            OwnerProgram.EnsureInUse();
            GL.Uniform2(UniformLocation, x, y);
        }
        public void SetValue2(uint x, uint y)
        {
            ValidateType(ActiveUniformType.UnsignedIntVec2);
            OwnerProgram.EnsureInUse();
            GL.Uniform2(UniformLocation, x, y);
        }
        #endregion

        #region SetValue3
        public void SetValue3(ref Vector3 value)
        {
            ValidateType(ActiveUniformType.FloatVec3);
            OwnerProgram.EnsureInUse();
            GL.Uniform3(UniformLocation, ref value);
        }
        public void SetValue3(Vector3 value)
        {
            SetValue3(ref value);
        }
        public void SetValue3(float x, float y, float z)
        {
            ValidateType(ActiveUniformType.FloatVec3);
            OwnerProgram.EnsureInUse();
            GL.Uniform3(UniformLocation, x, y, z);
        }

        public void SetValue3(ref Vector3d value)
        {
            SetValue3(value.X, value.Y, value.Z);
        }
        public void SetValue3(Vector3d value)
        {
            SetValue3(value.X, value.Y, value.Z);
        }
        public void SetValue3(double x, double y, double z)
        {
            ValidateType(ActiveUniformType.DoubleVec3);
            OwnerProgram.EnsureInUse();
            GL.Uniform3(UniformLocation, x, y, z);
        }

        public void SetValue3(int x, int y, int z)
        {
            ValidateType(ActiveUniformType.IntVec3);
            OwnerProgram.EnsureInUse();
            GL.Uniform3(UniformLocation, x, y, z);
        }
        public void SetValue3(uint x, uint y, uint z)
        {
            ValidateType(ActiveUniformType.UnsignedIntVec3);
            OwnerProgram.EnsureInUse();
            GL.Uniform3(UniformLocation, x, y, z);
        }
        #endregion

        #region SetValue4
        public void SetValue4(ref Vector4 value)
        {
            ValidateType(ActiveUniformType.FloatVec4);
            OwnerProgram.EnsureInUse();
            GL.Uniform4(UniformLocation, ref value);
        }
        public void SetValue4(Vector4 value)
        {
            SetValue4(ref value);
        }
        public void SetValue4(ref Color4 value)
        {
            ValidateType(ActiveUniformType.FloatVec4);
            OwnerProgram.EnsureInUse();
            GL.Uniform4(UniformLocation, value);
        }
        public void SetValue4(Color4 value)
        {
            SetValue4(ref value);
        }
        public void SetValue4(Color4b value)
        {
            ValidateType(ActiveUniformType.FloatVec4);
            OwnerProgram.EnsureInUse();
            GL.Uniform4(UniformLocation, value.R / 255f, value.G / 255f, value.B / 255f, value.A / 255f);
        }
        public void SetValue4(Quaternion value)
        {
            ValidateType(ActiveUniformType.FloatVec4);
            OwnerProgram.EnsureInUse();
            GL.Uniform4(UniformLocation, value);
        }
        public void SetValue4(float x, float y, float z, float w)
        {
            ValidateType(ActiveUniformType.FloatVec4);
            OwnerProgram.EnsureInUse();
            GL.Uniform4(UniformLocation, x, y, z, w);
        }

        public void SetValue4(ref Vector4d value)
        {
            ValidateType(ActiveUniformType.DoubleVec4);
            OwnerProgram.EnsureInUse();
            GL.Uniform4(UniformLocation, value.X, value.Y, value.Z, value.W);
        }
        public void SetValue4(Vector4d value)
        {
            SetValue4(ref value);
        }
        public void SetValue4(double x, double y, double z, double w)
        {
            ValidateType(ActiveUniformType.DoubleVec4);
            OwnerProgram.EnsureInUse();
            GL.Uniform4(UniformLocation, x, y, z, w);
        }

        public void SetValue4(int x, int y, int z, int w)
        {
            ValidateType(ActiveUniformType.IntVec4);
            OwnerProgram.EnsureInUse();
            GL.Uniform4(UniformLocation, x, y, z, w);
        }
        public void SetValue4(uint x, uint y, uint z, uint w)
        {
            ValidateType(ActiveUniformType.UnsignedIntVec4);
            OwnerProgram.EnsureInUse();
            GL.Uniform4(UniformLocation, x, y, z, w);
        }
        #endregion

        #region SetValueSamplers
        public virtual void SetValueTexture(Texture texture)
        {
            throw new InvalidOperationException("You tried to set a uniform with an incorrect type");
        }
        public virtual void SetValueTextureArray(Texture[] textures, int startValueIndex, int startUniformIndex, int count)
        {
            throw new InvalidOperationException("You tried to set a uniform with an incorrect type");
        }
        #endregion

        #region SetValueMat2
        public void SetValueMat2(ref Matrix2 value, bool transpose = false)
        {
            ValidateType(ActiveUniformType.FloatMat2);
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix2(UniformLocation, transpose, ref value);
        }
        public void SetValueMat2(ref Matrix2x3 value, bool transpose = false)
        {
            ValidateType(ActiveUniformType.FloatMat2x3);
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix2x3(UniformLocation, 1, transpose, ref value.Row0.X);
        }
        public void SetValueMat2(ref Matrix2x4 value, bool transpose = false)
        {
            ValidateType(ActiveUniformType.FloatMat2x4);
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix2x4(UniformLocation, 1, transpose, ref value.Row0.X);
        }

        public void SetValueMat2(ref Matrix2d value, bool transpose = false)
        {
            ValidateType((ActiveUniformType)All.DoubleMat2);
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix2(UniformLocation, 1, transpose, ref value.Row0.X);
        }
        public void SetValueMat2(ref Matrix2x3d value, bool transpose = false)
        {
            ValidateType((ActiveUniformType)All.DoubleMat2x3);
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix2x3(UniformLocation, 1, transpose, ref value.Row0.X);
        }
        public void SetValueMat2(ref Matrix2x4d value, bool transpose = false)
        {
            ValidateType((ActiveUniformType)All.DoubleMat2x4);
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix2x4(UniformLocation, 1, transpose, ref value.Row0.X);
        }
        #endregion

        #region SetValueMat3
        public void SetValueMat3(ref Matrix3 value, bool transpose = false)
        {
            ValidateType(ActiveUniformType.FloatMat3);
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix3(UniformLocation, transpose, ref value);
        }
        public void SetValueMat3(ref Matrix3x2 value, bool transpose = false)
        {
            ValidateType(ActiveUniformType.FloatMat3x2);
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix3x2(UniformLocation, 1, transpose, ref value.Row0.X);
        }
        public void SetValueMat3(ref Matrix3x4 value, bool transpose = false)
        {
            ValidateType(ActiveUniformType.FloatMat3x4);
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix3x4(UniformLocation, 1, transpose, ref value.Row0.X);
        }

        public void SetValueMat3(ref Matrix3d value, bool transpose = false)
        {
            ValidateType((ActiveUniformType)All.DoubleMat3);
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix3(UniformLocation, 1, transpose, ref value.Row0.X);
        }
        public void SetValueMat3(ref Matrix3x2d value, bool transpose = false)
        {
            ValidateType((ActiveUniformType)All.DoubleMat3x2);
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix3x2(UniformLocation, 1, transpose, ref value.Row0.X);
        }
        public void SetValueMat3(ref Matrix3x4d value, bool transpose = false)
        {
            ValidateType((ActiveUniformType)All.DoubleMat3x4);
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix3x4(UniformLocation, 1, transpose, ref value.Row0.X);
        }
        #endregion

        #region SetValueMat4
        public void SetValueMat4(ref Matrix4 value, bool transpose = false)
        {
            ValidateType(ActiveUniformType.FloatMat4);
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix4(UniformLocation, transpose, ref value);
        }
        public void SetValueMat4(ref Matrix4x2 value, bool transpose = false)
        {
            ValidateType(ActiveUniformType.FloatMat4x2);
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix4x2(UniformLocation, 1, transpose, ref value.Row0.X);
        }
        public void SetValueMat4(ref Matrix4x3 value, bool transpose = false)
        {
            ValidateType(ActiveUniformType.FloatMat4x3);
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix4x3(UniformLocation, 1, transpose, ref value.Row0.X);
        }

        public void SetValueMat4(ref Matrix4d value, bool transpose = false)
        {
            ValidateType((ActiveUniformType)All.DoubleMat4);
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix4(UniformLocation, 1, transpose, ref value.Row0.X);
        }
        public void SetValueMat4(ref Matrix4x2d value, bool transpose = false)
        {
            ValidateType((ActiveUniformType)All.DoubleMat4x2);
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix4x2(UniformLocation, 1, transpose, ref value.Row0.X);
        }
        public void SetValueMat4(ref Matrix4x3d value, bool transpose = false)
        {
            ValidateType((ActiveUniformType)All.DoubleMat4x3);
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix4x3(UniformLocation, 1, transpose, ref value.Row0.X);
        }
        #endregion

        #region SetValue1Array
        public void SetValue1Array(float[] value, int startValueIndex, int count)
        {
            ValidateArrayAndType(ActiveUniformType.Float, value.Length, startValueIndex, count);
            OwnerProgram.EnsureInUse();
            GL.Uniform1(UniformLocation, count, ref value[startValueIndex]);
        }

        public void SetValue1Array(double[] value, int startValueIndex, int count)
        {
            ValidateArrayAndType(ActiveUniformType.Double, value.Length, startValueIndex, count);
            OwnerProgram.EnsureInUse();
            GL.Uniform1(UniformLocation, count, ref value[startValueIndex]);
        }

        public void SetValue1Array(int[] value, int startValueIndex, int count)
        {
            ValidateArrayAndType(ActiveUniformType.Int, value.Length, startValueIndex, count);
            OwnerProgram.EnsureInUse();
            GL.Uniform1(UniformLocation, count, ref value[startValueIndex]);
        }

        public void SetValue1Array(uint[] value, int startValueIndex, int count)
        {
            ValidateArrayAndType(ActiveUniformType.UnsignedInt, value.Length, startValueIndex, count);
            OwnerProgram.EnsureInUse();
            GL.Uniform1(UniformLocation, count, ref value[startValueIndex]);
        }
        #endregion

        #region SetValue2Array
        public void SetValue2Array(Vector2[] value, int startValueIndex, int count)
        {
            ValidateArrayAndType(ActiveUniformType.FloatVec2, value.Length, startValueIndex, count);
            OwnerProgram.EnsureInUse();
            GL.Uniform2(UniformLocation, count, ref value[startValueIndex].X);
        }

        public void SetValue2Array(Vector2d[] value, int startValueIndex, int count)
        {
            ValidateArrayAndType(ActiveUniformType.DoubleVec2, value.Length, startValueIndex, count);
            OwnerProgram.EnsureInUse();
            GL.Uniform2(UniformLocation, count, ref value[startValueIndex].X);
        }
        #endregion

        #region SetValue3Array
        public void SetValue3Array(Vector3[] value, int startValueIndex, int count)
        {
            ValidateArrayAndType(ActiveUniformType.FloatVec3, value.Length, startValueIndex, count);
            OwnerProgram.EnsureInUse();
            GL.Uniform3(UniformLocation, count, ref value[startValueIndex].X);
        }

        public void SetValue3Array(Vector3d[] value, int startValueIndex, int count)
        {
            ValidateArrayAndType(ActiveUniformType.DoubleVec3, value.Length, startValueIndex, count);
            OwnerProgram.EnsureInUse();
            GL.Uniform3(UniformLocation, count, ref value[startValueIndex].X);
        }
        #endregion

        #region SetValue4Array
        public void SetValue4Array(Vector4[] value, int startValueIndex, int count)
        {
            ValidateArrayAndType(ActiveUniformType.FloatVec4, value.Length, startValueIndex, count);
            OwnerProgram.EnsureInUse();
            GL.Uniform4(UniformLocation, count, ref value[startValueIndex].X);
        }

        public void SetValue4Array(Vector4d[] value, int startValueIndex, int count)
        {
            ValidateType(ActiveUniformType.DoubleVec4);
            OwnerProgram.EnsureInUse();
            GL.Uniform4(UniformLocation, count, ref value[startValueIndex].X);
        }
        #endregion

        //region SetValueMatrix2Array or god knows what the fuck WHY ARE THERE SO MANY SET VALUES

        //TODO: SetValues for ivecX-s and uvecX-s somehow? We don't have such structs from OpenTK...
        //Note from the future: methods can have multiple parameters you malnourished rat
        //now implement these god fucking SetValue methods haha yes I will, one day.
        //i'm reading this again and still not about to implement it because alta paja gil
        //alright you still haven't done this I'm not disappointed but... ya know
        //dude...

        //TODO: Should we validate array sets to make sure the indices are OK and such?
        //Also: Should we even check that the type is OK?

        /// <summary>
        /// Checks that the ShaderUniform's UniformType is the correct type and throws an exception otherwise.
        /// </summary>
        /// <param name="type".></param>
        private protected void ValidateType(ActiveUniformType type)
        {
            if (UniformType != type)
                throw new InvalidOperationException(string.Concat("You tried to set a uniform with an incorrect type. You tried to set a ", type.ToString(), " while the uniform's type was ", UniformType.ToString()));
        }

        private protected void ValidateArrayAndType(ActiveUniformType type, int valueLength, int startValueIndex, int count)
        {
            ValidateType(type);
            if (count > Size)
                throw new ArgumentOutOfRangeException("count", count, "You tried to set too many elements for this uniform");

            if (startValueIndex < 0 || startValueIndex > valueLength)
                throw new ArgumentOutOfRangeException("startValueIndex", startValueIndex, "startValueIndex must be in the range [0, value.Length)");

            if (valueLength - startValueIndex < count)
                throw new IndexOutOfRangeException("The array isn't big enough to read count values");
        }

        public override string ToString()
        {
            return string.Concat("Name=\"", Name, "\" Type=", UniformType.ToString());
        }
    }
}
