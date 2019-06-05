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
        /// <summary>All of the (non-block) uniforms from the ShaderProgram</summary>
        private readonly ShaderUniform[] uniforms;

        /// <summary>This array contains only the sampler uniforms</summary>
        private readonly ShaderSamplerUniform[] samplerUniforms;

        /// <summary>This array contains only the sampler array uniforms</summary>
        private readonly ShaderSamplerArrayUniform[] samplerArrayUniforms;

        public readonly ShaderProgram Program;

        /// <summary>
        /// Gets a ShaderUniform by name. If there's no such name, returns null
        /// </summary>
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
        
        /// <summary>The amount of uniforms in the shader program</summary>
        public int Count { get { return uniforms.Length; } }

        internal ShaderUniformList(ShaderProgram program)
        {
            this.Program = program;
            GL.GetProgram(program.Handle, GetProgramParameterName.ActiveUniforms, out int totalCount);

            uniforms = new ShaderUniform[totalCount - program.BlockUniforms.TotalUniformCount];

            // All ShaderSamplerUniform-s and ShaderSamplerArrayUniform-s found will be added to these lists,
            // which will then be turned into the samplerUniforms and samplerArrayUniforms arrays
            List<ShaderSamplerUniform> su = new List<ShaderSamplerUniform>(uniforms.Length);
            List<ShaderSamplerArrayUniform> suarr = new List<ShaderSamplerArrayUniform>(uniforms.Length);

            int arrIndex = 0;
            for (int i = 0; i < totalCount; i++)
            {
                string name = GL.GetActiveUniform(program.Handle, i, out int size, out ActiveUniformType type);
                int location = GL.GetUniformLocation(program.Handle, name);
                if (location != -1) //If the location is -1, then it's probably a uniform block so let's not add it to the uniform list
                {
                    if (size == 1) // if size is 1, it's not an array uniform
                    {
                        if (IsSamplerUniformType(type))
                        { // sampler uniforms are treated differently, they are called on use to ensure the proper texture value is applied
                            ShaderSamplerUniform s = new ShaderSamplerUniform(program, location, name, type);
                            uniforms[arrIndex++] = s;
                            su.Add(s);
                        }
                        else
                            uniforms[arrIndex++] = new ShaderUniform(program, location, name, type);
                    }
                    else // size is > 1, this is an array
                    {
                        if (IsSamplerUniformType(type))
                        { // sampler uniform arrays, like sampler uniforms, also treated differently for the same reason
                            ShaderSamplerArrayUniform s = new ShaderSamplerArrayUniform(program, location, name.Substring(0, name.Length - name.LastIndexOf('[') + 1), size, type);
                            uniforms[arrIndex++] = s;
                            suarr.Add(s);
                        }
                        else
                            uniforms[arrIndex++] = new ShaderUniform(program, location, name.Substring(0, name.Length - name.LastIndexOf('[') + 1), type);
                    }
                }

            }

            samplerUniforms = su.ToArray();
            samplerArrayUniforms = suarr.ToArray();
        }

        /// <summary>
        /// When using sampler uniforms, this will make sure they all work together properly.
        /// This is called by ShaderProgram.EnsurePreDrawStates()
        /// </summary>
        internal void EnsureSamplerUniformsSet()
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

            Program.GraphicsDevice.EnsureAllBound(textures);

            for (int i = 0; i < samplerUniforms.Length; i++)
                samplerUniforms[i].ApplyUniformValue();
            for (int i = 0; i < samplerArrayUniforms.Length; i++)
                samplerArrayUniforms[i].ApplyUniformValues();

            //TODO: Sampler uniform arrays
        }

        public override string ToString()
        {
            return String.Concat("ShaderUniformList with ", uniforms.Length, " uniforms");
        }

        /// <summary>
        /// Returns whether the given ActiveUniformType is a sampler type.
        /// Sampler array's don't count (return false)
        /// </summary>
        /// <param name="type"></param>
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
