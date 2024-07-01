using System.Collections.Generic;

namespace Anansi
{
	/// <summary>
	/// A storylet tracks metadata about a single knot entry within an Ink Story.
	/// Anansi uses this class to create StoryletInstance instances from the
	/// various results returned by the storylet query.
	/// </summary>
	public class Storylet
	{
		#region Properties

		/// <summary>
		/// The ID of the knot within an Ink Story.
		/// </summary>
		public string ID { get; }

		/// <summary>
		/// The Ink Story this storylet belongs to.
		/// </summary>
		public Ink.Runtime.Story InkStory { get; }

		/// <summary>
		/// The number of Storylets that must elapse before this storylet may be used again.
		/// </summary>
		public int Cooldown { get; set; }

		/// <summary>
		/// Will this content always be selected if available
		/// </summary>
		public bool IsMandatory { get; set; }

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
		public StoryletPrecondition Precondition { get; set; }

		/// <summary>
		/// Bindings to input into the precondition query.
		/// </summary>
		public Dictionary<string, object> InputBindings { get; }

		/// <summary>
		/// Variable substitutions to apply using the precondition query results.
		/// </summary>
		public Dictionary<string, string> VariableSubstitutions { get; }

		/// <summary>
		/// The weight of this storylet instance
		/// </summary>
		public int Weight { get; set; }

		/// <summary>
		/// The name of the Ink function to use to calculate the weight of a particular instance.
		/// </summary>
		public string WeightFunctionName { get; set; }

		/// <summary>
		/// Text shown to the player when this storylet is an option within the
		/// action selection menu.
		/// </summary>
		public string ChoiceLabel { get; set; }

		/// <summary>
		/// Is this storylet not under cooldown and not reached its max number of plays.
		/// </summary>
		public bool IsEligible
		{
			get
			{
				bool hasCooldown = Cooldown > 0;

				if ( hasCooldown && CooldownTimeRemaining > 0 ) return false;

				if ( !IsRepeatable && TimesPlayed > 0 ) return false;

				return true;
			}
		}

		#endregion

		#region Constructors

		public Storylet(
			string knotID,
			Ink.Runtime.Story story
		)
		{
			ID = knotID;
			InkStory = story;
			Cooldown = 0;
			CooldownTimeRemaining = 0;
			IsRepeatable = true;
			IsMandatory = false;
			Precondition = new StoryletPrecondition();
			ChoiceLabel = "";
			Tags = new HashSet<string>();
			VariableSubstitutions = new Dictionary<string, string>();
			InputBindings = new Dictionary<string, object>();
			Weight = 1;
			WeightFunctionName = null;
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
