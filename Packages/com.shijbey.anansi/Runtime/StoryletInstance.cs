using System.Collections.Generic;
using Ink.Runtime;

namespace Anansi
{
	/// <summary>
	/// A pairing of storylet with a specific set of bindings from running a storylet's
	/// precondition query.
	/// </summary>
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
		/// Get the label displayed when this storylet is used as a choice.
		/// </summary>
		public string ChoiceLabel => Storylet.ChoiceLabel;

		/// <summary>
		/// A reference to the storylet this is an instance of.
		/// </summary>
		public Storylet Storylet { get; }

		/// <summary>
		/// The weight of this storylet instance.
		/// </summary>
		public int Weight { get; set; }

		/// <summary>
		/// RePraxis variable names mapped to their bound values from the database.
		/// </summary>
		public Dictionary<string, object> PreconditionBindings { get; }

		#endregion

		#region Constructors

		public StoryletInstance(
			Storylet storylet,
			Dictionary<string, object> preconditionBindings,
			int weight
		)
		{
			Storylet = storylet;
			PreconditionBindings = new Dictionary<string, object>(
				preconditionBindings
			);
			Weight = weight;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Sets the instance-specific variable values in the main story.
		/// </summary>
		public void BindInstanceVariables()
		{
			// Set the variables from the preconditionBindings
			foreach ( var substitution in Storylet.VariableSubstitutions )
			{
				Storylet.Story.variablesState[substitution.Key] =
					PreconditionBindings[substitution.Value];
			}
		}

		#endregion
	}
}
