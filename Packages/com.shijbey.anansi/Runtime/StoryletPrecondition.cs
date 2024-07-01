using System.Collections.Generic;

namespace Anansi
{
	/// <summary>
	/// Data used to query a story's logic database to see if a storylet is eligible
	/// to be visited.
	/// </summary>
	public class StoryletPrecondition
	{
		/// <summary>
		/// Variable to output from the query and bind to knot input parameters.
		/// </summary>
		public List<string> OutputVars { get; set; }

		/// <summary>
		/// A query that needs to pass before the storylet is allowed to run.
		/// </summary>
		public RePraxis.DBQuery Query { get; set; }

		public StoryletPrecondition()
		{
			OutputVars = new List<string>();
			Query = new RePraxis.DBQuery();
		}

		public StoryletPrecondition(RePraxis.DBQuery query)
		{
			OutputVars = new List<string>();
			Query = query;
		}
	}
}
