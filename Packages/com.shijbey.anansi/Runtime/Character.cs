using UnityEngine;

namespace Anansi
{
	/// <summary>
	/// A character within the story
	/// </summary>
	public class Character : MonoBehaviour
	{
		#region Fields

		/// <summary>
		/// The name of this character for display in the UI when speaking
		/// </summary>
		[SerializeField]
		protected string m_displayName;

		/// <summary>
		/// A unique ID for this character to be identified as within the StoryDatabase
		/// </summary>
		[SerializeField]
		protected string m_uniqueID;

		/// <summary>
		/// A reference to the sprite controller attached to this GameObject
		/// </summary>
		protected SpriteController m_spriteController;

		#endregion

		#region Properties

		/// <summary>
		/// The characters name displayed in the UI
		/// </summary>
		public string DisplayName => m_displayName;

		/// <summary>
		/// The character's unique ID
		/// </summary>
		public string UniqueID => m_uniqueID;

		/// <summary>
		/// The character's current location.
		/// </summary>
		public Location Location { get; set; }

		#endregion

		#region Unity Messages

		private void Awake()
		{
			m_spriteController = GetComponent<SpriteController>();
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Set the currently displayed sprite using the given tags
		/// </summary>
		/// <param name="tags"></param>
		public void SetSprite(params string[] tags)
		{
			if ( m_spriteController == null ) return;

			m_spriteController.SetSpriteFromTags( tags );
		}

		#endregion
	}
}
