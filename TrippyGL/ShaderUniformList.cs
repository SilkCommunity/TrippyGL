using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;

namespace TrippyGL
{
    /// <summary>
    /// A list of <see cref="ShaderUniform"/> belonging to a <see cref="ShaderProgram"/>.
    /// This class also does some controlling over these uniforms to make everything run nicely.
    /// </summary>
    public class ShaderUniformList
    {
        /// <summary>The <see cref="ShaderProgram"/> the uniforms belong to.</summary>
        public readonly ShaderProgram Program;

        /// <summary>All of the (non-block) uniforms from the <see cref="ShaderProgram"/>.</summary>
        private readonly ShaderUniform[] uniforms;

        /// <summary>This array contains only the sampler <see cref="ShaderUniform"/>-s.</summary>
        private readonly ShaderSamplerUniform[] samplerUniforms;

        /// <summary>This array contains only the sampler array <see cref="ShaderUniform"/>-s.</summary>
        private readonly ShaderSamplerArrayUniform[] samplerArrayUniforms;

        /// <summary>
        /// Gets a <see cref="ShaderUniform"/> by name. If there's no such name, returns null.
        /// </summary>
        /// <param name="name">The name (as declared in the shaders) of the <see cref="ShaderUniform"/> to get.</param>
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

        /// <summary>The amount of <see cref="ShaderUniform"/>-s in the <see cref="ShaderProgram"/>.</summary>
        public int Count => uniforms.Length;

        /// <summary>A not-always-correct list with all the textures currently applied to the sampler uniforms.</summary>
        private readonly List<Texture> textureList;

        /// <summary>
        /// Whether the <see cref="textureList"/> is correct or not.<para/>
        /// <see cref="ShaderSamplerUniform"/>-s set this to true when their value has changed.
        /// </summary>
        internal bool isTextureListDirty = true;

        /// <summary>Whether there is at least one sampler-type (or sampler-array-type) <see cref="ShaderUniform"/>.</summary>
        private readonly bool hasSamplerUniforms;

        private ShaderUniformList(ShaderProgram program, int totalUniformCount, int totalUniformBlockCount)
        {
            Program = program;
            uniforms = new ShaderUniform[totalUniformCount - totalUniformBlockCount];

            // All ShaderSamplerUniform-s and ShaderSamplerArrayUniform-s found will be added to these lists,
            // which will then be turned into the samplerUniforms and samplerArrayUniforms arrays
            List<ShaderSamplerUniform> su = new List<ShaderSamplerUniform>(uniforms.Length);
            List<ShaderSamplerArrayUniform> suarr = new List<ShaderSamplerArrayUniform>(uniforms.Length);

            int arrIndex = 0;
            for (int i = 0; i < totalUniformCount; i++)
            {
                string name = GL.GetActiveUniform(program.Handle, i, out int size, out ActiveUniformType type);
                int location = GL.GetUniformLocation(program.Handle, name);
                if (location != -1) //If the location is -1, then it's probably a uniform block so let's not add it to the uniform list
                {
                    if (size == 1) // if size is 1, it's not an array uniform
                    {
                        if (TrippyUtils.IsUniformSamplerType(type))
                        { // sampler uniforms are treated differently, they are called on use to ensure the proper texture value is applied
                            ShaderSamplerUniform s = new ShaderSamplerUniform(program, location, name, size, type);
                            uniforms[arrIndex++] = s;
                            su.Add(s);
                        }
                        else
                            uniforms[arrIndex++] = new ShaderUniform(program, location, name, size, type);
                    }
                    else // size is > 1, this is an array
                    {
                        if (TrippyUtils.IsUniformSamplerType(type))
                        { // sampler uniform arrays, like sampler uniforms, also treated differently for the same reason
                            ShaderSamplerArrayUniform s = new ShaderSamplerArrayUniform(program, location, name.Substring(0, name.Length - name.LastIndexOf('[') + 1), size, type);
                            uniforms[arrIndex++] = s;
                            suarr.Add(s);
                        }
                        else
                            uniforms[arrIndex++] = new ShaderUniform(program, location, name.Substring(0, name.Length - name.LastIndexOf('[') + 1), size, type);
                    }
                }
            }

            if (su.Count + suarr.Count == 0) // If there are no sampler uniforms, then we mark this as false and don't
                hasSamplerUniforms = false; // create any of the sampler uniform variables nor do any of their processes
            else
            {
                hasSamplerUniforms = true;
                samplerUniforms = su.ToArray();
                samplerArrayUniforms = suarr.ToArray();

                int maxTextures = samplerUniforms.Length;
                for (int i = 0; i < samplerArrayUniforms.Length; i++)
                    maxTextures += samplerArrayUniforms[i].ArrayLength;
                textureList = new List<Texture>(maxTextures);
            }
        }

        /// <summary>
        /// When using sampler uniforms, this will make sure they all work together properly.
        /// This is called by <see cref="ShaderProgram.EnsurePreDrawStates"/> after the program is ensured to be in use.
        /// </summary>
        internal void EnsureSamplerUniformsSet()
        {
            // Quick explanation of this method:
            // This method binds all the textures needed for the ShaderProgram's sampler-type uniforms to different texture units.
            // Then, it tells each ShaderSamplerUniform to ensure the texture unit it's sampler is using is the correct one for their texture
            // This is necessary because else, when using multiple samplers, you can't ensure they will all be using the correct texture

            if (hasSamplerUniforms)
            {
                if (isTextureListDirty)
                    RemakeTextureList();

                Program.GraphicsDevice.BindAllTextures(textureList);

                for (int i = 0; i < samplerUniforms.Length; i++)
                    samplerUniforms[i].ApplyUniformValue();
                for (int i = 0; i < samplerArrayUniforms.Length; i++)
                    samplerArrayUniforms[i].ApplyUniformValues();
            }
        }

        /// <summary>
        /// Recreates the <see cref="textureList"/> list. This is, clears it and then adds all the
        /// sampler uniform's texture values, then marks the list as not dirty.
        /// </summary>
        private void RemakeTextureList()
        {
            textureList.Clear();

            for (int i = 0; i < samplerUniforms.Length; i++)
            {
                Texture t = samplerUniforms[i].TextureValue;
                if (t != null && !textureList.Contains(t))
                    textureList.Add(t);
            }

            for (int i = 0; i < samplerArrayUniforms.Length; i++)
            {
                Texture[] tarr = samplerArrayUniforms[i].texValues;
                for (int c = 0; c < tarr.Length; c++)
                {
                    Texture t = tarr[c];
                    if (t != null && !textureList.Contains(t))
                        textureList.Add(t);
                }
            }

            isTextureListDirty = false;
        }

        public override string ToString()
        {
            return string.Concat("ShaderUniformList with ", uniforms.Length.ToString(), " uniforms");
        }

        /// <summary>
        /// Creates a <see cref="ShaderUniformList"/> and queries the uniforms for a given <see cref="ShaderProgram"/>.<para/>
        /// The <see cref="ShaderProgram"/> must already have had it's block uniforms queried prior to this.
        /// </summary>
        internal static ShaderUniformList CreateForProgram(ShaderProgram program)
        {
            GL.GetProgram(program.Handle, GetProgramParameterName.ActiveUniforms, out int totalUniformCount);
            int totalUniformBlockCount = program.BlockUniforms == null ? 0 : program.BlockUniforms.TotalUniformCount;
           
            if (totalUniformCount - totalUniformBlockCount == 0)
                return null;
            return new ShaderUniformList(program, totalUniformCount, totalUniformBlockCount);
        }
    }
}
