using System.Collections.Generic;

namespace TrippyGL
{
    /// <summary>
    /// A read-only list of all transform feedback variables.
    /// </summary>
    public class TransformFeedbackVariableDescriptionList
    {
        private readonly TransformFeedbackVariableDescription[] descriptions;

        /// <summary>Gets the total amount of variables on this list.</summary>
        public int Count { get { return descriptions.Length; } }

        /// <summary>
        /// Gets a variable from this list.
        /// </summary>
        /// <param name="index">The index of the variable to get.</param>
        public TransformFeedbackVariableDescription this[int index] { get { return descriptions[index]; } }

        /// <summary>The total amount of components used by all the variables (and padding!) on this list.</summary>
        public int ComponentCount { get; }

        /// <summary>The amount of buffer bindings needed for this list's variables.</summary>
        public int BufferBindingsNeeded { get; }

        /// <summary>The total number of attributes. This will equal this.Count when there are no padding descriptors.</summary>
        public int AttribCount { get; }

        /// <summary>Whether there is at least one padding descriptor on this list.</summary>
        public bool ContainsPadding { get; }

        /// <summary>
        /// Creates a list of TransformFeedbackVariableDescription-s from an array of user-specified descriptions.
        /// The list will be formatted so padding variables are merged and placed adjacent to another variable from the same buffer where possible.
        /// </summary>
        /// <param name="variableDescriptions">The user-specified array of TransformFeedbackVariableDescription-s. No reference to this array will be held.</param>
        internal TransformFeedbackVariableDescriptionList(TransformFeedbackVariableDescription[] variableDescriptions)
        {
            List<TransformFeedbackVariableDescription> list = new List<TransformFeedbackVariableDescription>(variableDescriptions.Length);

            #region GenerateList

            // TODO: Make sure padding variables are never alone if possible!
            // We need to implement the following case:
            // (skips buff1) (skips buff1) (var1 buff2) (var2 buff1) should be turned into:
            // (var1 buff2) (skips buff1) (skipb buff1) (var2 buff1)

            // TODO: WE NEED TO ADD SKIPPING TO THE FIRST VARIABLES WHEN NON-CONSECUTIVE VARIABLES SHARE SUBSET
            // (var1 buff1) (var2 buff2) (var3 buff1) in this case we'll send each into a different buffer indice,
            // but var1 is gonna need var.ComponentCount skipping after itself and var3 is gonna need var1.ComponentCount skippings before itself
            // Man, fuck this whole non-consecutive-subset thing!

            for (int i = 0; i < variableDescriptions.Length; i++)
            {
                if (variableDescriptions[i].IsPadding)
                    AddNewPaddingVariable(ref variableDescriptions[i]);
                else
                    AddNewAttribVariable(ref variableDescriptions[i]);
            }

            // This function takes care of adding padding variables to the list
            void AddNewPaddingVariable(ref TransformFeedbackVariableDescription desc)
            {
                // Adds a new padding variable to 'list', but merges it with a previous padding variable if possible.
                // So if there are two 1-component padding specified, they are turned into a single 2-component padding descriptor.
                // the 'desc' parameter should always be a padding descriptor!
                // If no padding variable is available for merging and there is another variable going into the same buffer subset,
                // then the new padding variable will be placed next to that same-subset variable

                for (int c = list.Count - 1; c >= 0; c--) // We loop the list from the end to the start, in descending order.
                {
                    if (list[c].BufferSubset == desc.BufferSubset)
                    {
                        if (list[c].IsPadding && list[c].PaddingComponentCount < 4)
                        { // We found a previous subsequent variable for less-than-4-components padding on the same buffer. Let's merge them!
                            int paddingTotal = list[c].PaddingComponentCount + desc.PaddingComponentCount;
                            if (paddingTotal <= 4) // If paddingTotal <= 4, it's only one variable. We overwrite.
                                list[c] = new TransformFeedbackVariableDescription(desc.BufferSubset, paddingTotal);
                            else
                            { // If paddingTotal > 4, we overwrite the previous to be 4 and insert next to it the rest of the padding
                                list[c] = new TransformFeedbackVariableDescription(desc.BufferSubset, 4);
                                list.Insert(c + 1, new TransformFeedbackVariableDescription(desc.BufferSubset, paddingTotal - 4));
                            }
                            return;
                        }
                        else
                        {
                            // If there is a non-padding variable on the same buffer before this padding (or it's 4-component padding) then we can't merge
                            // In this case, we just insert the padding next to that variable (whether it's padding or not)
                            list.Insert(c + 1, desc);
                            return;
                        }
                    }
                }

                // Got to the end? Then there was nothing going into desc.BufferSubset yet. desc is the first
                list.Add(desc); // That means the buffer subset's first variable is padding
            }

            // This function takes care of adding attrib variables to the list
            void AddNewAttribVariable(ref TransformFeedbackVariableDescription desc)
            {
                list.Add(desc);
            }

            #endregion GenerateList

            // We turn the list into the actual array we'll be storing
            descriptions = list.ToArray();

            #region CalculateValues

            // Finally, let's calculate some values based on the array we have
            ComponentCount = 0;
            ContainsPadding = false;
            AttribCount = 0;
            BufferBindingsNeeded = 0;
            BufferObjectSubset previousSubset = null; // The subset from the previous variable, when it changes we need a new buffer index
            List<BufferObjectSubset> usedSubsets = new List<BufferObjectSubset>(variableDescriptions.Length); // All the subsets that we've used

            for (int i = 0; i < descriptions.Length; i++)
            {
                if (descriptions[i].BufferSubset != previousSubset)
                {
                    previousSubset = descriptions[i].BufferSubset;
                    BufferBindingsNeeded++;

                    if (usedSubsets.Contains(previousSubset))
                        ContainsPadding = true; // We're gonna need to add padding to this variable to get it stored the way we'd want to
                    else
                        usedSubsets.Add(previousSubset);
                }

                if (descriptions[i].IsPadding)
                    ContainsPadding = true;
                else
                    AttribCount++;

                if (!ContainsPadding && descriptions[i].IsPadding)
                    ContainsPadding = true;

                ComponentCount += descriptions[i].ComponentCount;
            }

            #endregion CalculateValues
        }

        /// <summary>
        /// Calculates the offset into a subset for a variable, measured in components.
        /// </summary>
        /// <param name="variableIndex">The index in this list of the variable who's offset to calculate.</param>
        internal int CalculateVariableOffsetIntoSubset(int variableIndex)
        {
            int offset = 0;

            // We loop from the start to the variable index and add up the components of all the variables in the same buffer
            for (int i = 0; i < variableIndex; i++)
            {
                if (descriptions[i].BufferSubset == descriptions[variableIndex].BufferSubset)
                    offset += descriptions[i].ComponentCount;
            }

            return offset;
        }
    }
}
