using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TDRS;


namespace Calypso
{
    /// <summary>
    /// User-facing MonoBehavior for defining basic metadata for characters
    ///
    /// <para>
    /// The fields available in the inspector supply values at the start of
    /// the game and should not be relied on to change the state of the
    /// actor while in Play mode. Some values are read-only and others are
    /// synced with the GameManager's database when using the accessor methods.
    /// </para>
    /// </summary>
    public class Actor : MonoBehaviour
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
        /// The location to start the character
        /// </summary>
        [SerializeField]
        protected Location m_startingLocation;

        /// <summary>
        /// The current location of the character
        /// </summary>
        protected Location m_currentLocation;

        /// <summary>
        /// Sprites used to display the character
        /// </summary>
        [SerializeField]
        protected CharacterSprite[] m_sprites;

        /// <summary>
        /// A reference to the games social engine that tracks database info
        /// </summary>
        [SerializeField]
        private SocialEngine m_socialEngine;

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
        public Location Location => m_currentLocation;

        #endregion

        #region Unity Actions

        /// <summary>
        /// Action invoked whenever the character changes their location
        /// </summary>
        public UnityAction<Location> OnLocationChanged;

        #endregion

        #region Unity Messages

        private void Start()
        {
            m_socialEngine.DB.Insert($"{UniqueID}");
            if (m_startingLocation != null)
            {
                SetLocation(m_startingLocation);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Move an Actor to a new location.
        /// </summary>
        /// <param name="location"></param>
        public void SetLocation(Location location)
        {
            // Remove the character from their current location
            if (m_currentLocation != null)
            {
                m_currentLocation.RemoveCharacter(this);
                m_socialEngine.DB.Delete($"{m_currentLocation.UniqueID}.characters.{UniqueID}");
                m_socialEngine.DB.Delete($"{UniqueID}.location.{m_currentLocation.UniqueID}");
                m_currentLocation = null;
            }

            if (location != null)
            {
                location.AddCharacter(this);
                m_currentLocation = location;
                m_socialEngine.DB.Insert($"{location.UniqueID}.characters.{UniqueID}");
                m_socialEngine.DB.Insert($"{UniqueID}.location.{location.UniqueID}");
            }

            if (OnLocationChanged != null) OnLocationChanged.Invoke(location);
        }

        /// <summary>
        /// Get a sprite with the given tags
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public Sprite GetSprite(params string[] tags)
        {
            foreach (var entry in m_sprites)
            {
                var spriteTags = new HashSet<string>(entry.tags);

                foreach (string t in tags)
                {
                    if (!t.Contains(t))
                    {
                        return null;
                    }
                }

                return entry.sprite;
            }

            return null;
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// Associates a sprite image with a set of descriptive tags for the sprite.
        /// </summary>
        [System.Serializable]
        public class CharacterSprite
        {
            /// <summary>
            /// The sprite image to display.
            /// </summary>
            public Sprite sprite;

            /// <summary>
            /// Tags used to retrieve the image.
            /// Examples: neutral, smiling, scowling, sad, blushing, laughing
            /// </summary>
            public string[] tags;
        }

        #endregion
    }
}
