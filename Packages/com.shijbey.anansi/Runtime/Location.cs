using System.Collections.Generic;
using UnityEngine;

namespace Anansi
{
	/// <summary>
	/// A place where characters can be. Locations are displayed as the background images
	/// in the game.
	/// </summary>
	public class Location : MonoBehaviour
	{
		#region Fields

		/// <summary>
		/// The name of the location as displayed in the UI.
		/// </summary>
		[SerializeField]
		protected string m_displayName;

		/// <summary>
		/// An ID that uniquely identifies this location among other locations in the game.
		/// </summary>
		[SerializeField]
		protected string m_uniqueID;

		[SerializeField]
		protected Location[] m_connectedLocations;

		/// <summary>
		/// An additional tag added to the location sprite selection to filter based on time
		/// of the day.
		/// </summary>
		protected string m_timeOfDayVariant = "morning";

		/// <summary>
		/// Collection of all characters currently at this location
		/// </summary>
		protected List<Character> m_actors = new List<Character>();

		/// <summary>
		/// A reference to the sprite controller attached to this GameObject
		/// </summary>
		protected SpriteController m_spriteController;

		#endregion

		#region Properties

		/// <summary>
		/// The name of the location as displayed in the UI.
		/// </summary>
		public string DisplayName => m_displayName;

		/// <summary>
		/// An ID that uniquely identifies this location among other locations in the game.
		/// </summary>
		public string UniqueID => m_uniqueID;

		/// <summary>
		/// Collection of all characters currently at this location
		/// </summary>
		public IEnumerable<Character> Characters => m_actors;

		public IEnumerable<Location> ConnectedLocations => m_connectedLocations;

		public string TimeOfDayVariant
		{
			get
			{
				return m_timeOfDayVariant;
			}

			set
			{
				m_timeOfDayVariant = value;
			}
		}

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

		/// <summary>
		/// Remove a character from the location
		/// </summary>
		/// <param name="actor"></param>
		public void AddCharacter(Character actor)
		{
			m_actors.Add( actor );
		}

		/// <summary>
		/// Add a character to the location
		/// </summary>
		/// <param name="actor"></param>
		public void RemoveCharacter(Character actor)
		{
			m_actors.Remove( actor );
		}

		#endregion
	}
}
