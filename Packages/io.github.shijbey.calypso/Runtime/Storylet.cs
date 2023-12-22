using System.Collections.Generic;
using Ink.Runtime;
using Calypso.RePraxis;

namespace Calypso
{
    /// <summary>
    /// A storylet tracks metadata about a single knot entry within an Ink Story.
    /// Calypso uses this class to create StoryletInstance instances from the
    /// various results returned by the storylet query.
    /// </summary>
    public class Storylet
    {
        #region Public Properties

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
        public int Cooldown { get; set; }

        /// <summary>
        /// The number of Storylets that must elapse before this storylet may be used again.
        /// </summary>
        public int CooldownTimeRemaining { get; private set; }

        /// <summary>
        /// Can this story be used more than once.
        /// </summary>
        public bool IsRepeatable { get; set; }

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
        public DBQuery Query { get; set; }

        /// <summary>
        /// Variable substitutions to apply from the bindings retrieved from the _preconditionQuery.
        /// </summary>
        public Dictionary<string, string> VariableSubstitutions { get; }

        #endregion

        #region Constructors

        public Storylet(
            string knotID,
            Story story
        )
        {
            KnotID = knotID;
            Story = story;
            Cooldown = 0;
            IsRepeatable = true;
            Tags = new HashSet<string>();
            Query = new DBQuery();
            VariableSubstitutions = new Dictionary<string, string>();
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

        #endregion
    }
}
