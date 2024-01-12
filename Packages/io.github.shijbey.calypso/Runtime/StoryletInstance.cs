using System.Collections.Generic;
using Ink.Runtime;

namespace Calypso
{
    public class StoryletInstance
    {
        #region Properties

        /// <summary>
        /// The ID of the knot within an Ink Story.
        /// </summary>
        public string KnotID => Storylet.KnotID;

        /// <summary>
        /// The Ink Story this storylet belongs to.
        /// </summary>
        public Story Story => Storylet.Story;

        /// <summary>
        /// A reference to the storylet this is an instance of
        /// </summary>
        public Storylet Storylet { get; }

        /// <summary>
        /// The weight of this storylet instance
        /// </summary>
        public int Weight { get; }

        public Dictionary<string, string> PreconditionBindings { get; }

        #endregion

        #region Constructors

        public StoryletInstance(
            Storylet storylet,
            Dictionary<string, string> preconditionBindings,
            int weight
        )
        {
            Storylet = storylet;
            PreconditionBindings = new Dictionary<string, string>(
                preconditionBindings
            );
            Weight = weight;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initialize the story state using the instance information
        /// </summary>
        /// <returns></returns>
        public Story InitializeStory()
        {
            // Set the variables from the preconditionBindings
            foreach (var substitution in Storylet.VariableSubstitutions)
            {
                Storylet.Story.variablesState[substitution.Key] =
                    PreconditionBindings[substitution.Value];
            }

            return Storylet.Story;
        }

        #endregion
    }
}
