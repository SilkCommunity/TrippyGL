using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    /// <summary>
    /// Represents a shader uniform from a ShaderProgram and allows control over that uniform
    /// </summary>
    public class ShaderUniform
    {
        /// <summary>The name with which this uniform is declared on the shader program</summary>
        public readonly string Name;

        /// <summary>The type of uniform</summary>
        public readonly ActiveUniformType UniformType;

        /// <summary>The program containing this uniform</summary>
        public readonly ShaderProgram OwnerProgram;

        /// <summary>The location of the uniform on the program. Used for setting the value</summary>
        public readonly int UniformLocation;

        internal ShaderUniform(ShaderProgram owner, int uniformLoc, string name, ActiveUniformType type)
        {
            this.OwnerProgram = owner;
            this.UniformLocation = uniformLoc;
            this.Name = name;
            this.UniformType = type;
        }

        #region SetValue1
        public void SetValue1(float value)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform1(UniformLocation, value);
        }

        public void SetValue1(double value)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform1(UniformLocation, value);
        }

        public void SetValue1(int value)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform1(UniformLocation, value);
        }
        public void SetValue1(uint value)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform1(UniformLocation, value);
        }
        #endregion

        #region SetValue2
        public void SetValue2(ref Vector2 value)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform2(UniformLocation, ref value);
        }
        public void SetValue2(Vector2 value)
        {
            SetValue2(ref value);
        }
        public void SetValue2(float x, float y)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform2(UniformLocation, x, y);
        }

        public void SetValue2(ref Vector2d value)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform2(UniformLocation, value.X, value.Y);
        }
        public void SetValue2(Vector2d value)
        {
            SetValue2(ref value);
        }
        public void SetValue2(double x, double y)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform2(UniformLocation, x, y);
        }
        
        public void SetValue2(int x, int y)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform2(UniformLocation, x, y);
        }
        public void SetValue2(uint x, uint y)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform2(UniformLocation, x, y);
        }
        #endregion

        #region SetValue3
        public void SetValue3(ref Vector3 value)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform3(UniformLocation, ref value);
        }
        public void SetValue3(Vector3 value)
        {
            SetValue3(ref value);
        }
        public void SetValue3(float x, float y, float z)
        {
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
            OwnerProgram.EnsureInUse();
            GL.Uniform3(UniformLocation, x, y, z);
        }
        
        public void SetValue3(int x, int y, int z)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform3(UniformLocation, x, y, z);
        }
        public void SetValue3(uint x, uint y, uint z)
        {
            GL.Uniform3(UniformLocation, x, y, z);
        }
        #endregion

        #region SetValue4
        public void SetValue4(ref Vector4 value)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform4(UniformLocation, ref value);
        }
        public void SetValue4(Vector4 value)
        {
            SetValue4(ref value);
        }
        public void SetValue4(ref Color4 value)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform4(UniformLocation, value);
        }
        public void SetValue4(Color4 value)
        {
            SetValue4(ref value);
        }
        public void SetValue4(Color4b value)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform4(UniformLocation, value.R / 255f, value.G / 255f, value.B / 255f, value.A / 255f);
        }
        public void SetValue4(Quaternion value)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform4(UniformLocation, value);
        }
        public void SetValue4(float x, float y, float z, float w)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform4(UniformLocation, x, y, z, w);
        }

        public void SetValue4(ref Vector4d value)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform4(UniformLocation, value.X, value.Y, value.Z, value.W);
        }
        public void SetValue4(Vector4d value)
        {
            SetValue4(ref value);
        }
        public void SetValue4(double x, double y, double z, double w)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform4(UniformLocation, x, y, z, w);
        }

        public void SetValue4(int x, int y, int z, int w)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform4(UniformLocation, x, y, z, w);
        }
        public void SetValue4(uint x, uint y, uint z, uint w)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform4(UniformLocation, x, y, z, w);
        }
        #endregion

        #region SetValueSamplers
        public virtual void SetValueTexture(Texture texture) { }
        public virtual void SetValueTextureArray(Texture[] textures, int startValueIndex, int startUniformIndex, int count) { }
        #endregion

        #region SetValueMat2
        public void SetValueMat2(ref Matrix2 value, bool transpose = false)
        {
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix2(UniformLocation, transpose, ref value);
        }
        public void SetValueMat2(ref Matrix2x3 value, bool transpose = false)
        {
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix2x3(UniformLocation, 1, transpose, ref value.Row0.X);
        }
        public void SetValueMat2(ref Matrix2x4 value, bool transpose = false)
        {
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix2x4(UniformLocation, 1, transpose, ref value.Row0.X);
        }

        public void SetValueMat2(ref Matrix2d value, bool transpose = false)
        {
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix2(UniformLocation, 1, transpose, ref value.Row0.X);
        }
        public void SetValueMat2(ref Matrix2x3d value, bool transpose = false)
        {
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix2x3(UniformLocation, 1, transpose, ref value.Row0.X);
        }
        public void SetValueMat2(ref Matrix2x4d value, bool transpose = false)
        {
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix2x4(UniformLocation, 1, transpose, ref value.Row0.X);
        }
        #endregion

        #region SetValueMat3
        public void SetValueMat3(ref Matrix3 value, bool transpose = false)
        {
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix3(UniformLocation, transpose, ref value);
        }
        public void SetValueMat3(ref Matrix3x2 value, bool transpose = false)
        {
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix3x2(UniformLocation, 1, transpose, ref value.Row0.X);
        }
        public void SetValueMat3(ref Matrix3x4 value, bool transpose = false)
        {
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix3x4(UniformLocation, 1, transpose, ref value.Row0.X);
        }

        public void SetValueMat3(ref Matrix3d value, bool transpose = false)
        {
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix3(UniformLocation, 1, transpose, ref value.Row0.X);
        }
        public void SetValueMat3(ref Matrix3x2d value, bool transpose = false)
        {
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix3x2(UniformLocation, 1, transpose, ref value.Row0.X);
        }
        public void SetValueMat3(ref Matrix3x4d value, bool transpose = false)
        {
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix3x4(UniformLocation, 1, transpose, ref value.Row0.X);
        }
        #endregion

        #region SetValueMat4
        public void SetValueMat4(ref Matrix4 value, bool transpose = false)
        {
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix4(UniformLocation, transpose, ref value);
        }
        public void SetValueMat4(ref Matrix4x2 value, bool transpose = false)
        {
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix4x2(UniformLocation, 1, transpose, ref value.Row0.X);
        }
        public void SetValueMat4(ref Matrix4x3 value, bool transpose = false)
        {
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix4x3(UniformLocation, 1, transpose, ref value.Row0.X);
        }

        public void SetValueMat4(ref Matrix4d value, bool transpose = false)
        {
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix4(UniformLocation, 1, transpose, ref value.Row0.X);
        }
        public void SetValueMat4(ref Matrix4x2d value, bool transpose = false)
        {
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix4x2(UniformLocation, 1, transpose, ref value.Row0.X);
        }
        public void SetValueMat4(ref Matrix4x3d value, bool transpose = false)
        {
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix4x3(UniformLocation, 1, transpose, ref value.Row0.X);
        }
        #endregion

        #region SetValue1Array
        public void SetValue1Array(float[] value, int startValueIndex, int startUniformIndex, int count)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform1(UniformLocation + startUniformIndex, count, ref value[startValueIndex]);
        }

        public void SetValue1Array(double[] value, int startValueIndex, int startUniformIndex, int count)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform1(UniformLocation + startUniformIndex, count, ref value[startValueIndex]);
        }

        public void SetValue1Array(int[] value, int startValueIndex, int startUniformIndex, int count)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform1(UniformLocation + startUniformIndex, count, ref value[startValueIndex]);
        }

        public void SetValue1Array(uint[] value, int startValueIndex, int startUniformIndex, int count)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform1(UniformLocation + startUniformIndex, count, ref value[startValueIndex]);
        }
        #endregion

        #region SetValue2Array
        public void SetValue2Array(Vector2[] value, int startValueIndex, int startUniformIndex, int count)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform2(UniformLocation + startUniformIndex, count, ref value[startValueIndex].X);
        }

        public void SetValue2Array(Vector2d[] value, int startValueIndex, int startUniformIndex, int count)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform2(UniformLocation + startUniformIndex, count, ref value[startValueIndex].X);
        }
        #endregion

        #region SetValue3Array
        public void SetValue3Array(Vector3[] value, int startValueIndex, int startUniformIndex, int count)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform3(UniformLocation + startUniformIndex, count, ref value[startValueIndex].X);
        }

        public void SetValue3Array(Vector3d[] value, int startValueIndex, int startUniformIndex, int count)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform3(UniformLocation + startUniformIndex, count, ref value[startValueIndex].X);
        }
        #endregion

        #region SetValue4Array
        public void SetValue4Array(Vector4[] value, int startValueIndex, int startUniformIndex, int count)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform4(UniformLocation + startUniformIndex, count, ref value[startValueIndex].X);
        }

        public void SetValue4Array(Vector4d[] value, int startValueIndex, int startUniformIndex, int count)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform4(UniformLocation + startUniformIndex, count, ref value[startValueIndex].X);
        }
        #endregion

        //region SetValueMatrix2Array or god knows what the fuck WHY ARE THERE SO MANY SET VALUES

        //TODO: SetValues for ivecX-s and uvecX-s somehow? We don't have such structs from OpenTK...
        //Note from the future: methods can have multiple parameters you malnourished rat
        //now implement these god fucking SetValue methods haha yes I will, one day.
        //i'm reading this again and still not about to implement it because alta paja gil

        public override string ToString()
        {
            return String.Concat("Name=\"", Name, "\" Type=", UniformType.ToString());
        }
    }
}
