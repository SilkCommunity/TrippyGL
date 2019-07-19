using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;

namespace TrippyGL
{
    /// <summary>
    /// A readonly list containing the active attributes of a ShaderProgram
    /// </summary>
    public class ActiveAttribList
    {
        /// <summary>The internal ActiveVertexAttrib array</summary>
        internal readonly ActiveVertexAttrib[] attributes;

        /// <summary>
        /// Gets an ActiveVertexAttrib from the list. While these are by location, remember that some attributes use more than one location
        /// </summary>
        /// <param name="index">The list index of the ActiveVertexAttrib</param>
        /// <returns></returns>
        public ActiveVertexAttrib this[int index] { get { return attributes[index]; } }

        /// <summary>The amount of ActiveVertexAttrib-s stored by this list</summary>
        public int Length { get { return attributes.Length; } }

        internal ActiveAttribList(ShaderProgram program)
        {
            GL.GetProgram(program.Handle, GetProgramParameterName.ActiveAttributes, out int attribCount);
            List<ActiveVertexAttrib> attribList = new List<ActiveVertexAttrib>(attribCount);

            for (int i = 0; i < attribCount; i++)
            {
                ActiveVertexAttrib a = new ActiveVertexAttrib(program, i);
                if (a.Location >= 0)    // Sometimes other stuff shows up, such as gl_InstanceID with location -1.
                    attribList.Add(a);  // We should, of course, filter these out.
            }

            attributes = attribList.ToArray();
            Array.Sort(attributes, (x, y) => x.Location.CompareTo(y.Location));
        }

        /// <summary>
        /// Checks that the names given for some vertex attributes match the names found for the actual vertex attributes
        /// </summary>
        /// <param name="providedNames"></param>
        internal bool CheckThatAttributesMatch(VertexAttribDescription[] providedDesc, string[] providedNames)
        {
            // This function assumes the length of the two given arrays match

            // While all of the attribute names are provided by the user, that doesn't mean all of them are in here.
            // The GLSL compiler may not make an attribute ACTIVE if, for example, it is never used.
            // So, if we see a provided name doesn't match, maybe it isn't active, so let's skip that name and check the next.
            // That said, both arrays are indexed in the same way. So if all attributes are active, we'll basically just
            // check one-by-one, index-by-index that the names on attributes[i] match providedNames[i]

            int nameIndex = -1;

            if (providedNames.Length == 0)
                return attributes.Length == 0;

            for (int i = 0; i < attributes.Length; i++)
            {
                nameIndex++;
                if (nameIndex == providedNames.Length)
                    return false;

                while (providedDesc[nameIndex].AttribType != attributes[i].AttribType || attributes[i].Name != providedNames[nameIndex])
                {
                    if (++nameIndex == providedNames.Length)
                        return false;
                }
            }

            return true;
        }
    }
}
