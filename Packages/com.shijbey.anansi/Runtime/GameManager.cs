using UnityEngine;
using TDRS;

namespace Anansi
{
	/// <summary>
	/// The GameManager coordinates the start of the game.
	///
	/// <remark>
	/// You can replace this manager with another class in your own projects. Just ensure that it
	/// calls <c>StoryController.Initialize()</c> and <c>StoryController.StartStory</c>
	/// </remark>
	/// </summary>
	[DefaultExecutionOrder( 2 )]
	public class GameManager : MonoBehaviour
	{
		#region Fields

		/// <summary>
		/// Manages all the progression of the story and all storylets.
		/// </summary>
		[SerializeField]
		private StoryController m_storyController;

		#endregion

		#region Unity Messages

		private void Start()
		{
			SocialEngineController.Instance.Initialize();
			SocialEngineController.Instance.RegisterAgentsAndRelationships();
			m_storyController.Initialize();
			m_storyController.StartStory();
		}

		#endregion
	}
}
