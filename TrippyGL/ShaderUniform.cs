using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    public class ShaderUniform
    {
        /// <summary>The name with which this uniform is declared on the shader program</summary>
        public readonly string Name;

        /// <summary>The type of uniform</summary>
        public readonly ActiveUniformType UniformType;

        /// <summary>The program containing this uniform</summary>
        public readonly ShaderProgram OwnerProgram;

        /// <summary>The location of the uniform on the program. Used for setting the value</summary>
        private protected readonly int location;

        internal ShaderUniform(ShaderProgram owner, int uniformLoc, string name, ActiveUniformType type)
        {
            this.OwnerProgram = owner;
            this.location = uniformLoc;
            this.Name = name;
            this.UniformType = type;
        }

        #region SetValue1
        public void SetValue(float value)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform1(location, value);
        }

        public void SetValue(double value)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform1(location, value);
        }

        public void SetValue(int value)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform1(location, value);
        }
        public void SetValue(uint value)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform1(location, value);
        }
        #endregion

        #region SetValue2
        public void SetValue(ref Vector2 value)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform2(location, ref value);
        }
        public void SetValue(Vector2 value)
        {
            SetValue(ref value);
        }
        public void SetValue(float x, float y)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform2(location, x, y);
        }

        public void SetValue(ref Vector2d value)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform2(location, value.X, value.Y);
        }
        public void SetValue(Vector2d value)
        {
            SetValue(ref value);
        }
        public void SetValue(double x, double y)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform2(location, x, y);
        }
        
        public void SetValue(int x, int y)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform2(location, x, y);
        }
        public void SetValue(uint x, uint y)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform2(location, x, y);
        }
        #endregion

        #region SetValue3
        public void SetValue(ref Vector3 value)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform3(location, ref value);
        }
        public void SetValue(Vector3 value)
        {
            SetValue(ref value);
        }
        public void SetValue(float x, float y, float z)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform3(location, x, y, z);
        }

        public void SetValue(ref Vector3d value)
        {
            SetValue(value.X, value.Y, value.Z);
        }
        public void SetValue(Vector3d value)
        {
            SetValue(value.X, value.Y, value.Z);
        }
        public void SetValue(double x, double y, double z)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform3(location, x, y, z);
        }
        
        public void SetValue(int x, int y, int z)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform3(location, x, y, z);
        }
        public void SetValue(uint x, uint y, uint z)
        {
            GL.Uniform3(location, x, y, z);
        }
        #endregion

        #region SetValue4
        public void SetValue(ref Vector4 value)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform4(location, ref value);
        }
        public void SetValue(Vector4 value)
        {
            SetValue(ref value);
        }
        public void SetValue(ref Color4 value)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform4(location, value);
        }
        public void SetValue(Color4 value)
        {
            SetValue(ref value);
        }
        public void SetValue(Color4b value)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform4(location, value.R / 255f, value.G / 255f, value.B / 255f, value.A / 255f);
        }
        public void SetValue(Quaternion value)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform4(location, value);
        }
        public void SetValue(float x, float y, float z, float w)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform4(location, x, y, z, w);
        }

        public void SetValue(ref Vector4d value)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform4(location, value.X, value.Y, value.Z, value.W);
        }
        public void SetValue(Vector4d value)
        {
            SetValue(ref value);
        }
        public void SetValue(double x, double y, double z, double w)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform4(location, x, y, z, w);
        }

        public void SetValue(int x, int y, int z, int w)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform4(location, x, y, z, w);
        }
        public void SetValue(uint x, uint y, uint z, uint w)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform4(location, x, y, z, w);
        }
        #endregion

        #region SetValueSamplers
        public virtual void SetValue(Texture texture) { }
        public virtual void SetValue(Texture[] textures, int startValueIndex, int startUniformIndex, int count) { }
        #endregion

        #region SetValueMat2
        public void SetValue(ref Matrix2 value, bool transpose = false)
        {
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix2(location, transpose, ref value);
        }
        public void SetValue(ref Matrix2x3 value, bool transpose = false)
        {
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix2x3(location, 1, transpose, ref value.Row0.X);
        }
        public void SetValue(ref Matrix2x4 value, bool transpose = false)
        {
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix2x4(location, 1, transpose, ref value.Row0.X);
        }

        public void SetValue(ref Matrix2d value, bool transpose = false)
        {
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix2(location, 1, transpose, ref value.Row0.X);
        }
        public void SetValue(ref Matrix2x3d value, bool transpose = false)
        {
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix2x3(location, 1, transpose, ref value.Row0.X);
        }
        public void SetValue(ref Matrix2x4d value, bool transpose = false)
        {
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix2x4(location, 1, transpose, ref value.Row0.X);
        }
        #endregion

        #region SetValueMat3
        public void SetValue(ref Matrix3 value, bool transpose = false)
        {
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix3(location, transpose, ref value);
        }
        public void SetValue(ref Matrix3x2 value, bool transpose = false)
        {
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix3x2(location, 1, transpose, ref value.Row0.X);
        }
        public void SetValue(ref Matrix3x4 value, bool transpose = false)
        {
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix3x4(location, 1, transpose, ref value.Row0.X);
        }

        public void SetValue(ref Matrix3d value, bool transpose = false)
        {
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix3(location, 1, transpose, ref value.Row0.X);
        }
        public void SetValue(ref Matrix3x2d value, bool transpose = false)
        {
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix3x2(location, 1, transpose, ref value.Row0.X);
        }
        public void SetValue(ref Matrix3x4d value, bool transpose = false)
        {
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix3x4(location, 1, transpose, ref value.Row0.X);
        }
        #endregion

        #region SetValueMat4
        public void SetValue(ref Matrix4 value, bool transpose = false)
        {
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix4(location, transpose, ref value);
        }
        public void SetValue(ref Matrix4x2 value, bool transpose = false)
        {
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix4x2(location, 1, transpose, ref value.Row0.X);
        }
        public void SetValue(ref Matrix4x3 value, bool transpose = false)
        {
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix4x3(location, 1, transpose, ref value.Row0.X);
        }

        public void SetValue(ref Matrix4d value, bool transpose = false)
        {
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix4(location, 1, transpose, ref value.Row0.X);
        }
        public void SetValue(ref Matrix4x2d value, bool transpose = false)
        {
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix4x2(location, 1, transpose, ref value.Row0.X);
        }
        public void SetValue(ref Matrix4x3d value, bool transpose = false)
        {
            OwnerProgram.EnsureInUse();
            GL.UniformMatrix4x3(location, 1, transpose, ref value.Row0.X);
        }
        #endregion


        #region SetValue1Array
        public void SetValue(float[] value, int startValueIndex, int startUniformIndex, int count)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform1(location + startUniformIndex, count, ref value[startValueIndex]);
        }

        public void SetValue(double[] value, int startValueIndex, int startUniformIndex, int count)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform1(location + startUniformIndex, count, ref value[startValueIndex]);
        }

        public void SetValue(int[] value, int startValueIndex, int startUniformIndex, int count)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform1(location + startUniformIndex, count, ref value[startValueIndex]);
        }

        public void SetValue(uint[] value, int startValueIndex, int startUniformIndex, int count)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform1(location + startUniformIndex, count, ref value[startValueIndex]);
        }
        #endregion

        #region SetValue2Array
        public void SetValue(Vector2[] value, int startValueIndex, int startUniformIndex, int count)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform2(location + startUniformIndex, count, ref value[startValueIndex].X);
        }

        public void SetValue(Vector2d[] value, int startValueIndex, int startUniformIndex, int count)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform2(location + startUniformIndex, count, ref value[startValueIndex].X);
        }
        #endregion

        #region SetValue3Array
        public void SetValue(Vector3[] value, int startValueIndex, int startUniformIndex, int count)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform3(location + startUniformIndex, count, ref value[startValueIndex].X);
        }

        public void SetValue(Vector3d[] value, int startValueIndex, int startUniformIndex, int count)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform3(location + startUniformIndex, count, ref value[startValueIndex].X);
        }
        #endregion

        #region SetValue4Array
        public void SetValue(Vector4[] value, int startValueIndex, int startUniformIndex, int count)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform4(location + startUniformIndex, count, ref value[startValueIndex].X);
        }

        public void SetValue(Vector4d[] value, int startValueIndex, int startUniformIndex, int count)
        {
            OwnerProgram.EnsureInUse();
            GL.Uniform4(location + startUniformIndex, count, ref value[startValueIndex].X);
        }
        #endregion

        //region SetValueMatrix2Array or god knows what the fuck WHY ARE THERE SO MANY SET VALUES

        //TODO: SetValues for ivecX-s and uvecX-s somehow? We don't have such structs from OpenTK...

        public override string ToString()
        {
            return String.Concat("Name=\"", Name, "\" Type=", UniformType);
        }
    }
}
