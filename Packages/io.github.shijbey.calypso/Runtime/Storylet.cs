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
        #region Protected Attributes

        /// <summary>
        /// The ID of the knot within an Ink Story.
        /// </summary>
        protected readonly string _knotID;

        /// <summary>
        /// A reference to the story that this storylet belongs to.
        /// </summary>
        protected readonly Story _story;

        /// <summary>
        /// The number of Storylets that must elapse before this storylet may be used again.
        /// </summary>
        protected int _cooldownTimeRemaining;

        #endregion

        #region Public Properties

        /// <summary>
        /// The ID of the knot within an Ink Story.
        /// </summary>
        public string KnotID => _knotID;

        /// <summary>
        /// The Ink Story this storylet belongs to.
        /// </summary>
        public Story Story => _story;

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
        public HashSet<string> Tags { get; private set; }

        /// <summary>
        /// A query that needs to pass before the storylet is allowed to run.
        /// </summary>
        public DBQuery Query { get; set; }

        /// <summary>
        /// Variable substitutions to apply from the bindings retrieved from the _preconditionQuery.
        /// </summary>
        public Dictionary<string, string> VariableSubstitutions { get; private set; }

        #endregion

        #region Constructors

        public Storylet(
            string knotID,
            Story story,
            int cooldown,
            bool isRepeatable,
            IEnumerable<string> tags,
            DBQuery preconditionQuery,
            Dictionary<string, string> variableSubstitutions
        )
        {
            _knotID = knotID;
            _story = story;
            Cooldown = cooldown;
            IsRepeatable = isRepeatable;
            Tags = new HashSet<string>(tags);
            Query = preconditionQuery;
            VariableSubstitutions = new Dictionary<string, string>(variableSubstitutions);
        }

        public Storylet(
            string knotID,
            Story story
        )
        {
            _knotID = knotID;
            _story = story;
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
            _cooldownTimeRemaining -= 1;
            return _cooldownTimeRemaining;
        }

        /// <summary>
        /// Reset the cooldown time for the storylet and return the remaining time.
        /// </summary>
        /// <returns></returns>
        public int ResetCooldown()
        {
            _cooldownTimeRemaining = Cooldown;
            return _cooldownTimeRemaining;
        }

        /// <summary>
        /// Increment the number of times that this storylet has been visited by the player.
        /// </summary>
        public void IncrementTimesPlayed()
        {
            TimesPlayed += 1;
        }

        /// <summary>
        /// Get a collection instances of this storylet.
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public IEnumerable<StoryletInstance> GetInstances(StoryDatabase db)
        {
            return new StoryletInstance[0];
        }

        /// <summary>
        /// Get a collection instances of this storylet.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="bindings"></param>
        /// <returns></returns>
        public IEnumerable<StoryletInstance> GetInstances(StoryDatabase db, Dictionary<string, string> bindings)
        {
            return new StoryletInstance[0];
        }

        #endregion
    }
}
