using System;
using System.Numerics;

namespace TrippyGL
{
    public sealed class SimpleShaderProgram : ShaderProgram
    {
        public readonly bool VertexColorsEnabled;

        public readonly bool TextureEnabled;

        public readonly bool LightningEnabled;

        private readonly DirectionalLight[] directionalLights;
        public ReadOnlySpan<DirectionalLight> DirectionalLights => new ReadOnlySpan<DirectionalLight>(directionalLights);

        private readonly ShaderUniform reflectivityUniform;
        private float reflectivity;
        public float Reflectivity
        {
            get => reflectivity;
            set
            {
                reflectivityUniform.SetValueFloat(value);
                reflectivity = value;
            }
        }

        private readonly ShaderUniform ambientLightColorUniform;
        private Vector4 ambientLightColor;
        public Vector4 AmbientLightColor
        {
            get => ambientLightColor;
            set
            {
                ambientLightColorUniform.SetValueVec4(value);
                ambientLightColor = value;
            }
        }

        private readonly ShaderUniform worldUniform;
        private Matrix4x4 world;
        public Matrix4x4 World
        {
            get => world;
            set
            {
                worldUniform.SetValueMat4(value);
                world = value;
            }
        }

        private readonly ShaderUniform viewUniform;
        private ShaderUniform cameraPosUniform;
        private Matrix4x4 view;
        public Matrix4x4 View
        {
            get => view;
            set
            {
                Vector3 cameraPos;
                if (!cameraPosUniform.IsEmpty && Matrix4x4.Invert(value, out Matrix4x4 inverse))
                {
                    Vector4 tmp = Vector4.Transform(Vector4.UnitW, inverse);
                    cameraPos = new Vector3(tmp.X, tmp.Y, tmp.Z);
                }
                else
                    cameraPos = default;
                SetView(value, cameraPos);
            }
        }

        private readonly ShaderUniform projectionUniform;
        private Matrix4x4 projection;
        public Matrix4x4 Projection
        {
            get => projection;
            set
            {
                projectionUniform.SetValueMat4(value);
                projection = value;
            }
        }

        private readonly ShaderUniform colorUniform;
        private Vector4 color;
        public Vector4 Color
        {
            get => color;
            set
            {
                colorUniform.SetValueVec4(value);
                color = value;
            }
        }

        private readonly ShaderUniform sampUniform;
        private Texture2D texture;
        public Texture2D Texture
        {
            get => texture;
            set
            {
                sampUniform.SetValueTexture(value);
                texture = value;
            }
        }

        public SimpleShaderProgram(GraphicsDevice graphicsDevice, uint programHandle, ActiveVertexAttrib[] activeAttribs,
            bool vertColorsEnabled, int directionalLightCount)
            : base(graphicsDevice, programHandle, activeAttribs)
        {
            sampUniform = Uniforms["samp"];
            ambientLightColorUniform = Uniforms["ambientLightColor"];
            reflectivityUniform = Uniforms["reflectivity"];
            worldUniform = Uniforms["World"];
            viewUniform = Uniforms["View"];
            projectionUniform = Uniforms["Projection"];
            cameraPosUniform = Uniforms["cameraPos"];
            colorUniform = Uniforms["color"];

            VertexColorsEnabled = vertColorsEnabled;
            TextureEnabled = !sampUniform.IsEmpty;
            LightningEnabled = !cameraPosUniform.IsEmpty;

            Color = new Vector4(1, 1, 1, 1);

            World = Matrix4x4.Identity;
            SetView(Matrix4x4.Identity, Vector3.Zero);
            Projection = Matrix4x4.Identity;

            if (directionalLightCount != 0)
            {
                directionalLights = new DirectionalLight[directionalLightCount];
                for (int i = 0; i < directionalLightCount; i++)
                {
                    string itostring = i.ToString();
                    directionalLights[i] = new DirectionalLight(Uniforms["lightDir" + itostring], Uniforms["lightColor" + itostring]);
                }
            }

            if (LightningEnabled)
            {
                AmbientLightColor = new Vector4(0, 0, 0, 1);
                Reflectivity = 1;
            }
        }

        public void SetView(in Matrix4x4 view, in Vector3 cameraPos)
        {
            viewUniform.SetValueMat4(view);
            if (!cameraPosUniform.IsEmpty)
                cameraPosUniform.SetValueVec3(cameraPos);
            this.view = view;
        }
    }
}
