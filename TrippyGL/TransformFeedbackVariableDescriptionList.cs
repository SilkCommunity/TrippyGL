using System;
using System.Collections.Generic;

namespace TrippyGL
{
    /// <summary>
    /// A read-only list of all transform feedback variables
    /// </summary>
    public class TransformFeedbackVariableDescriptionList
    {
        private readonly TransformFeedbackVariableDescription[] descriptions;

        /// <summary>Gets the total amount of variables on this list</summary>
        public int Count { get { return descriptions.Length; } }

        /// <summary>
        /// Gets a variable from this list
        /// </summary>
        /// <param name="index">The index of the variable to get</param>
        public TransformFeedbackVariableDescription this[int index] { get { return descriptions[index]; } }

        /// <summary>The total amount of components used by all the variables (and padding!) on this list</summary>
        public int ComponentCount { get; private set; }

        /// <summary>The amount of different buffer subsets present on this list</summary>
        public int BufferSubsetCount { get; private set; }

        /// <summary>The total number of attributes. This will equal this.Count when there are no padding descriptors</summary>
        public int AttribCount { get; private set; }

        /// <summary>Whether there is at least one padding descriptor on this list</summary>
        public bool ContainsPadding { get; private set; }

        internal TransformFeedbackVariableDescriptionList(TransformFeedbackVariableDescription[] descriptions)
        {
            List<TransformFeedbackVariableDescription> list = new List<TransformFeedbackVariableDescription>(descriptions.Length);
            List<BufferObjectSubset> allSubsets = new List<BufferObjectSubset>(descriptions.Length);
            AttribCount = 0;

            for (int i = 0; i < descriptions.Length; i++)
            {
                if (!allSubsets.Contains(descriptions[i].BufferSubset))
                    allSubsets.Add(descriptions[i].BufferSubset);

                if (descriptions[i].IsPadding)
                {
                    ContainsPadding = true;
                    AddNewPaddingVariable(ref descriptions[i]);
                    ComponentCount += descriptions[i].PaddingComponentCount;
                }
                else
                {
                    AttribCount++;
                    list.Add(descriptions[i]);
                    ComponentCount += descriptions[i].ComponentCount;
                }
            }

            void AddNewPaddingVariable(ref TransformFeedbackVariableDescription desc)
            {
                // Adds a new padding variable to 'list', but merges it with a previous padding variable if possible.
                // So if there are two 1-component padding specified, they are turned into a single 2-component padding descriptor.
                // the 'desc' parameter should always be a padding descriptor!

                for (int c = list.Count - 1; c >= 0; c--) // We loop the list from the end to the start, in descending order.
                {
                    if (list[c].BufferSubset == desc.BufferSubset)
                    {
                        if (list[c].IsPadding && list[c].PaddingComponentCount < 4)
                        { // We found a previous subsequent variable for less-than-4-components padding on the same buffer. Let's merge them!
                            int paddingTotal = list[c].PaddingComponentCount + desc.PaddingComponentCount;
                            if (paddingTotal <= 4)
                                list[c] = new TransformFeedbackVariableDescription(desc.BufferSubset, paddingTotal);
                            else
                            {
                                list[c] = new TransformFeedbackVariableDescription(desc.BufferSubset, 4);
                                list.Add(new TransformFeedbackVariableDescription(desc.BufferSubset, paddingTotal - 4));
                            }
                            return;
                        }
                        else
                        { // There is a non-padding variable before this padding? Then we can't merge it with any previous padding (there would be something in the middle)
                            list.Add(desc);
                            return;
                        }
                    }
                }

                // Got to the end? Then there was nothing going into desc.BufferSubset yet. desc is the first
                list.Add(desc);
            }

            BufferSubsetCount = allSubsets.Count;
            this.descriptions = list.ToArray();
        }
    }
}
