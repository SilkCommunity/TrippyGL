using System;
using System.Numerics;

namespace TrippyGL
{
    public sealed class DirectionalLight
    {
        private readonly ShaderUniform colorUniform;
        private readonly ShaderUniform directionUniform;

        private Vector3 direction;
        public Vector3 Direction
        {
            get => direction;
            set
            {
                direction = Vector3.Normalize(value);
                directionUniform.SetValueVec4(direction.X, direction.Y, direction.Z, specularPower);
            }
        }

        private Vector4 color;
        public Vector4 Color
        {
            get => color;
            set
            {
                color = value;
                colorUniform.SetValueVec4(value);
            }
        }

        private float specularPower;
        public float SpecularPower
        {
            get => specularPower;
            set
            {
                specularPower = value;
                directionUniform.SetValueVec4(direction.X, direction.Y, direction.Z, specularPower);
            }
        }

        internal DirectionalLight(ShaderUniform directionUniform, ShaderUniform colorUniform)
        {
            if (directionUniform.IsEmpty || colorUniform.IsEmpty)
                throw new ArgumentException("The uniforms can't be empty.");

            this.colorUniform = colorUniform;
            this.directionUniform = directionUniform;

            direction = new Vector3(0, -1, 0);
            SpecularPower = 1;
            Color = new Vector4(1, 1, 1, 1);
        }
    }
}
