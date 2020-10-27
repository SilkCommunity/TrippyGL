using System;
using System.Collections.Generic;
using Silk.NET.OpenGL;

namespace TrippyGL
{
    /// <summary>
    /// A list of <see cref="ShaderUniform"/> belonging to a <see cref="ShaderProgram"/>.
    /// </summary>
    public readonly struct ShaderUniformList : IEquatable<ShaderUniformList>
    {
        /// <summary>Whether this <see cref="ShaderUniformList"/> has null values.</summary>
        public bool IsEmpty => Program == null;

        /// <summary>The <see cref="ShaderProgram"/> the uniforms belong to.</summary>
        public readonly ShaderProgram Program;

        /// <summary>All of the (non-block) uniforms from the <see cref="ShaderProgram"/>.</summary>
        private readonly ShaderUniform[] uniforms;

        /// <summary>The amount of <see cref="ShaderUniform"/>-s in the <see cref="ShaderProgram"/>.</summary>
        public int Count => uniforms.Length;

        /// <summary>Gets the unsorted <see cref="ShaderUniform"/>-s from this list.</summary>
        public ReadOnlySpan<ShaderUniform> Uniforms => new ReadOnlySpan<ShaderUniform>(uniforms);

        /// <summary>A not-always-correct list with all the textures currently applied to the sampler uniforms.</summary>
        private readonly List<Texture> textureList;

        /// <summary>Whether this list contains at least one sampler-type (or sampler-array-type) <see cref="ShaderUniform"/>.</summary>
        internal readonly bool hasSamplerUniforms;

        /// <summary>
        /// Gets a <see cref="ShaderUniform"/> by name. If there's no such name, returns an empty <see cref="ShaderUniform"/>.
        /// </summary>
        /// <param name="name">The name (as declared in the shaders) of the <see cref="ShaderUniform"/> to get.</param>
        public ShaderUniform this[string name] => GetUniformByName(name);

        private ShaderUniformList(ShaderProgram program, int totalUniformCount, int totalUniformBlockCount)
        {
            Program = program;
            uniforms = new ShaderUniform[totalUniformCount - totalUniformBlockCount];

            int samplerUniformsTextureCount = 0;
            uint arrIndex = 0;
            for (uint i = 0; i < totalUniformCount; i++)
            {
                string name = Program.GL.GetActiveUniform(program.Handle, i, out int size, out UniformType type);
                int location = Program.GL.GetUniformLocation(program.Handle, name);

                if (location < 0) //If the location is -1, then it's probably a uniform block so let's not add it to the uniform list
                    continue;

                ShaderUniform uniform = new ShaderUniform(program, location, name, size, type);
                uniforms[arrIndex++] = uniform;
                if (uniform.IsSamplerType)
                    samplerUniformsTextureCount += uniform.Size;
            }

            if (samplerUniformsTextureCount == 0)
            {
                hasSamplerUniforms = false;
                textureList = null;
            }
            else
            {
                hasSamplerUniforms = true;
                textureList = new List<Texture>(samplerUniformsTextureCount);
            }
        }

        public static bool operator ==(ShaderUniformList left, ShaderUniformList right) => left.Equals(right);
        public static bool operator !=(ShaderUniformList left, ShaderUniformList right) => !left.Equals(right);

        /// <summary>
        /// When using sampler uniforms, this will make sure they all work together properly. This is called
        /// by <see cref="ShaderProgram.EnsurePreDrawStates"/> after the program is ensured to be in use.
        /// </summary>
        internal void EnsureSamplerUniformsSet()
        {
            // Quick explanation of this method:
            // This method binds all the textures needed for the ShaderProgram's sampler-type
            // uniforms to different texture units. Then, it tells each ShaderUniform to ensure
            // it's samplers are using the correct texture units for their textures.
            // This is necessary because otherwise, when using multiple samplers, you can't ensure
            // they will all be using the correct texture, since texture units might mix up.

            if (hasSamplerUniforms)
            {
                if (Program.areSamplerUniformsDirty)
                    RemakeTextureList();

                Program.GraphicsDevice.BindAllTextures(textureList);

                for (int i = 0; i < uniforms.Length; i++)
                    if (uniforms[i].IsSamplerType)
                        uniforms[i].ApplyUniformTextureValues();
            }
        }

        /// <summary>
        /// Recreates the <see cref="textureList"/> list. This is, clears it and then adds all the
        /// sampler uniform's texture values avoiding duplicates, then marks the list as not dirty.
        /// </summary>
        private void RemakeTextureList()
        {
            textureList.Clear();

            for (int i = 0; i < uniforms.Length; i++)
            {
                if (!uniforms[i].IsSamplerType)
                    continue;

                ReadOnlySpan<Texture> textures = uniforms[i].Textures;
                for (int c = 0; c < textures.Length; c++)
                {
                    Texture t = textures[c];
                    if (t == null)
                        throw new InvalidOperationException("Tried to draw with no texture set on uniform named: " + uniforms[i].Name);

                    if (t.IsDisposed)
                        throw new InvalidOperationException("Tried to draw with a disposed texture set on uniform named: " + uniforms[i].Name);

                    if (!textureList.Contains(t))
                        textureList.Add(t);
                }
            }

            Program.areSamplerUniformsDirty = false;
        }

        /// <summary>
        /// Gets a <see cref="ShaderUniform"/> by name. If there's no such name, returns an empty <see cref="ShaderUniform"/>.
        /// </summary>
        /// <param name="name">The name (as declared in the shaders) of the <see cref="ShaderUniform"/> to get.</param>
        public ShaderUniform GetUniformByName(ReadOnlySpan<char> name)
        {
            for (int i = 0; i < uniforms.Length; i++)
                if (name.SequenceEqual(uniforms[i].Name))
                    return uniforms[i];

            return default;
        }

        public override string ToString()
        {
            return string.Concat(nameof(Count) + "=", Count.ToString());
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Program.GetHashCode();
                for (int i = 0; i < uniforms.Length; i++)
                    hashCode = (hashCode * 397) ^ uniforms[i].GetHashCode();
                return hashCode;
            }
        }

        public bool Equals(ShaderUniformList other)
        {
            return Program == other.Program
                && uniforms == other.uniforms;
        }

        public override bool Equals(object obj)
        {
            if (obj is ShaderBlockUniformList shaderBlockUniformList)
                return Equals(shaderBlockUniformList);
            return false;
        }

        /// <summary>
        /// Creates a <see cref="ShaderUniformList"/> and queries the uniforms for a given <see cref="ShaderProgram"/>.<para/>
        /// The <see cref="ShaderProgram"/> must already have had it's block uniforms queried prior to this.
        /// If there are no uniforms, this method returns null.
        /// </summary>
        internal static ShaderUniformList CreateForProgram(ShaderProgram program)
        {
            program.GL.GetProgram(program.Handle, ProgramPropertyARB.ActiveUniforms, out int totalUniformCount);
            int totalUniformBlockCount = program.BlockUniforms == null ? 0 : program.BlockUniforms.TotalUniformCount;

            if (totalUniformCount - totalUniformBlockCount <= 0)
                return default;
            return new ShaderUniformList(program, totalUniformCount, totalUniformBlockCount);
        }
    }
}
