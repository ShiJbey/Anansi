using System.Collections.Generic;
using Ink.Runtime;
using RePraxis;

namespace Calypso
{
    /// <summary>
    /// A storylet tracks metadata about a single knot entry within an Ink Story.
    /// Calypso uses this class to create StoryletInstance instances from the
    /// various results returned by the storylet query.
    /// </summary>
    public class Storylet
    {
        #region Properties

        /// <summary>
        /// The ID of the knot within an Ink Story.
        /// </summary>
        public string KnotID { get; }

        /// <summary>
        /// The Ink Story this storylet belongs to.
        /// </summary>
        public Story Story { get; }

        /// <summary>
        /// The number of Storylets that must elapse before this storylet may be used again.
        /// </summary>
        public int Cooldown { get; }

        /// <summary>
        /// Will this content always be selected if available
        /// </summary>
        public bool IsMandatory { get; }

        /// <summary>
        /// The number of Storylets that must elapse before this storylet may be used again.
        /// </summary>
        public int CooldownTimeRemaining { get; private set; }

        /// <summary>
        /// Can this story be used more than once.
        /// </summary>
        public bool IsRepeatable { get; }

        /// <summary>
        /// The number of times this storylet has been seen by the player.
        /// </summary>
        public uint TimesPlayed { get; private set; }

        /// <summary>
        /// Tags associated with this storylet, used when filtering storylets during
        /// content selection.
        /// </summary>
        public HashSet<string> Tags { get; }

        /// <summary>
        /// A query that needs to pass before the storylet is allowed to run.
        /// </summary>
        public DBQuery Precondition { get; }

        /// <summary>
        /// Variable substitutions to apply from the bindings retrieved from the _preconditionQuery.
        /// </summary>
        public Dictionary<string, string> VariableSubstitutions { get; }

        /// <summary>
        /// The weight of this storylet instance
        /// </summary>
        public int Weight { get; }

        public int FirstLineTagOffset { get; }

        #endregion

        #region Constructors

        public Storylet(
            string knotID,
            Story story,
            int cooldown,
            bool isRepeatable,
            bool isMandatory,
            int weight,
            IEnumerable<string> tags,
            DBQuery precondition,
            Dictionary<string, string> variableSubstitutions,
            int firstLineTagOffset
        )
        {
            KnotID = knotID;
            Story = story;
            Cooldown = cooldown;
            CooldownTimeRemaining = 0;
            IsRepeatable = isRepeatable;
            Weight = weight;
            Tags = new HashSet<string>(tags);
            Precondition = precondition;
            VariableSubstitutions = variableSubstitutions;
            IsMandatory = isMandatory;
            FirstLineTagOffset = firstLineTagOffset;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Decrement the cool down time and return the remaining time.
        /// </summary>
        /// <returns></returns>
        public int DecrementCooldown()
        {
            CooldownTimeRemaining -= 1;
            return CooldownTimeRemaining;
        }

        /// <summary>
        /// Reset the cooldown time for the storylet and return the remaining time.
        /// </summary>
        /// <returns></returns>
        public int ResetCooldown()
        {
            CooldownTimeRemaining = Cooldown;
            return CooldownTimeRemaining;
        }

        /// <summary>
        /// Increment the number of times that this storylet has been visited by the player.
        /// </summary>
        public void IncrementTimesPlayed()
        {
            TimesPlayed += 1;
        }

        /// <summary>
        /// Reset the number of times this storylet has been visited
        /// </summary>
        public void ResetTimesPlayed()
        {
            TimesPlayed = 0;
        }

        #endregion
    }
}
