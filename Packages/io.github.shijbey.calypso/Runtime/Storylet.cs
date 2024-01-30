using System.Collections.Generic;
using UnityEngine;

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
		public Ink.Runtime.Story Story { get; }

		/// <summary>
		/// What type of storylet is this.
		/// </summary>
		public StoryletType StoryletType { get; }

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
		public RePraxis.DBQuery Precondition { get; set; }

		/// <summary>
		/// Bindings to input into the precondition query.
		/// </summary>
		public Dictionary<string, string> InputBindings { get; }

		/// <summary>
		/// Variable substitutions to apply using the precondition query results.
		/// </summary>
		public Dictionary<string, string> VariableSubstitutions { get; }

		/// <summary>
		/// The weight of this storylet instance
		/// </summary>
		public int Weight { get; set; }

		/// <summary>
		/// (Location storylets only) A list of locations connected to this one
		/// </summary>
		public List<string> ConnectedLocations { get; }

		/// <summary>
		/// (Location storylets only) The ID of the location associated with this storylet.
		/// </summary>
		public string LocationID { get; set; }

		/// <summary>
		/// (Action storylets only) The ID of the action associated with this storylet.
		/// </summary>
		public string ActionID { get; set; }

		/// <summary>
		/// (Action storylets only) A list of locations where this action can be performed
		/// </summary>
		public List<string> EligibleLocations { get; }

		/// <summary>
		/// Text shown to the player when this storylet is an option within the
		/// action selection menu.
		/// </summary>
		public string ChoiceLabel { get; set; }

		#endregion

		#region Constructors

		public Storylet(
			string knotID,
			Ink.Runtime.Story story,
			StoryletType storyletType
		)
		{
			KnotID = knotID;
			Story = story;
			StoryletType = storyletType;
			Cooldown = 0;
			CooldownTimeRemaining = 0;
			IsRepeatable = true;
			IsMandatory = false;
			Precondition = null;
			ChoiceLabel = "";
			ConnectedLocations = new List<string>();
			EligibleLocations = new List<string>();
			Tags = new HashSet<string>();
			VariableSubstitutions = new Dictionary<string, string>();
			InputBindings = new Dictionary<string, string>();
			LocationID = "";
			ActionID = "";
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Check if a storylet has the given tags.
		/// </summary>
		/// <param name="tags"></param>
		/// <returns></returns>
		public bool HasTags(IList<string> tags)
		{
			foreach ( string tag in tags )
			{
				if ( !Tags.Contains( tag ) ) return false;
			}

			return true;
		}

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
