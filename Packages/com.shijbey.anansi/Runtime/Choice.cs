using System.Collections.Generic;

namespace Anansi
{
	/// <summary>
	/// A class containing context metadata about a choice presented to the user.
	/// </summary>
	public class Choice
	{
		public string Text { get; set; }
		public int Index { get; }
		public List<string> Tags { get; }
		public Ink.Runtime.Choice InkChoice { get; }
		public StoryletInstance StoryletInstance { get; }

		public Choice(Ink.Runtime.Choice inkChoice)
		{
			Text = inkChoice.text;
			Index = inkChoice.index;
			Tags = inkChoice.tags != null ? new List<string>( inkChoice.tags ) : new List<string>();
			InkChoice = inkChoice;
			StoryletInstance = null;
		}

		public Choice(
			int index,
			string text,
			IEnumerable<string> tags,
			StoryletInstance storyletInstance
		)
		{
			Text = text;
			Index = index;
			Tags = new List<string>( tags );
			InkChoice = null;
			StoryletInstance = storyletInstance;
		}
	}
}
