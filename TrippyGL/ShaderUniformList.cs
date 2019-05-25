using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    /// <summary>
    /// A list of ShaderUniform variables.
    /// This class also does some controlling over these uniforms to make everything run nicely
    /// </summary>
    public class ShaderUniformList
    {
        /// <summary>All of the uniforms from the ShaderProgram</summary>
        private ShaderUniform[] uniforms;

        /// <summary>This array contains only the sampler uniforms</summary>
        private ShaderSamplerUniform[] samplerUniforms;

        /// <summary>This array contains only the sampler array uniforms</summary>
        private ShaderSamplerArrayUniform[] samplerArrayUniforms;

        /// <summary>Gets a uniform by name</summary>
        /// <param name="name">The name (declared in the shaders) of the uniform to get</param>
        public ShaderUniform this[string name]
        {
            get
            {
                for (int i = 0; i < uniforms.Length; i++)
                    if (uniforms[i].Name == name)
                        return uniforms[i];
                return null;
            }
        }

        public int UniformCount { get { return uniforms.Length; } }

        internal ShaderUniformList(ShaderProgram program)
        {
            GL.GetProgram(program.Handle, GetProgramParameterName.ActiveUniforms, out int count);
            uniforms = new ShaderUniform[count];

            // All ShaderSamplerUniform-s and ShaderSamplerArrayUniform-s found will be added to these lists,
            // which will then be turned into the samplerUniforms and samplerArrayUniforms arrays
            List<ShaderSamplerUniform> su = new List<ShaderSamplerUniform>(uniforms.Length);
            List<ShaderSamplerArrayUniform> suarr = new List<ShaderSamplerArrayUniform>(uniforms.Length);

            int location = 0; // The location of each uniform, this value is added the size of the uniform each loop to account for array uniforms
            for (int i = 0; i < count; i++)
            {
                string name = GL.GetActiveUniform(program.Handle, i, out int size, out ActiveUniformType type);
                if (size == 1) // if size is 1, it's not an array uniform
                {
                    if (IsSamplerUniformType(type))
                    { // sampler uniforms are treated differently, they are called on use to ensure the proper texture value is applied
                        ShaderSamplerUniform s = new ShaderSamplerUniform(program, location, name, type);
                        uniforms[i] = s;
                        su.Add(s);
                    }
                    else
                        uniforms[i] = new ShaderUniform(program, location, name, type);
                }
                else // size is > 1, this is an array
                {
                    if (IsSamplerUniformType(type))
                    { // sampler uniform arrays, like sampler uniforms, also treated differently for the same reason
                        ShaderSamplerArrayUniform s = new ShaderSamplerArrayUniform(program, location, name.Substring(0, name.Length - name.LastIndexOf('[') + 1), size, type);
                        uniforms[i] = s;
                        suarr.Add(s);
                    }
                    else
                        uniforms[i] = new ShaderUniform(program, location, name.Substring(0, name.Length - name.LastIndexOf('[') + 1), type);
                }
                location += size;
            }

            samplerUniforms = su.ToArray();
            samplerArrayUniforms = suarr.ToArray();
        }

        /// <summary>
        /// When using sampler uniforms, this will make sure they all work together properly.
        /// This is called automatically when the program is used.
        /// You should call this if you have multiple sampler uniforms (or an array of samplers) after modifying the uniforms but before drawing
        /// </summary>
        public void EnsureSamplerUniformsSet()
        {
            // Quick explanation of this method:
            // This method binds all the textures needed for the ShaderProgram's sampler-type uniforms to different texture units.
            // Then, it tells each ShaderSamplerUniform to ensure the texture unit it's sampler is using is the correct one for their texture
            // This is necessary because else, when using multiple samplers, you can't ensure they will all be using the correct texture

            List<Texture> textures = new List<Texture>(samplerUniforms.Length);
            for (int i = 0; i < samplerUniforms.Length; i++)
            {
                Texture t = samplerUniforms[i].TextureValue;
                if (t != null && !textures.Contains(t))
                    textures.Add(t);
            }
            for(int i=0; i<samplerArrayUniforms.Length; i++)
            {
                Texture[] tarr = samplerArrayUniforms[i].texValues;
                for(int c=0; c<tarr.Length; c++)
                {
                    Texture t = tarr[c];
                    if (t != null && !textures.Contains(t))
                        textures.Add(t);
                }
            }

            Texture.EnsureAllBound(textures);

            for (int i = 0; i < samplerUniforms.Length; i++)
                samplerUniforms[i].ApplyUniformValue();
            for (int i = 0; i < samplerArrayUniforms.Length; i++)
                samplerArrayUniforms[i].ApplyUniformValues();

            //TODO: Sampler uniform arrays
        }

        public static bool IsSamplerUniformType(ActiveUniformType type)
        {
            return (type >= ActiveUniformType.Sampler1D && type <= ActiveUniformType.Sampler2DRectShadow)
                || (type >= ActiveUniformType.Sampler2DMultisample && type <= ActiveUniformType.UnsignedIntSampler2DMultisample)
                || (type >= ActiveUniformType.IntSampler1D && type <= ActiveUniformType.IntSampler2DRect)
                || (type >= ActiveUniformType.UnsignedIntSampler1D && type <= ActiveUniformType.UnsignedIntSampler2DRect)
                || type == ActiveUniformType.SamplerBuffer || type == ActiveUniformType.SamplerCubeShadow;
        }
    }
}
