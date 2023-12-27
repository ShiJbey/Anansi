using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace Calypso.Unity
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
        #region Protected Fields

        /// <summary>
        /// The name of this character for display in the UI when speaking
        /// </summary>
        [SerializeField]
        protected string _displayName;

        /// <summary>
        /// A unique ID for this character to be identified as within the StoryDatabase
        /// </summary>
        [SerializeField]
        protected string _uniqueID;

        /// <summary>
        /// The location to start the character
        /// </summary>
        [SerializeField]
        protected Location _startingLocation;

        /// <summary>
        /// The current location of the character
        /// </summary>
        protected Location _currentLocation;

        /// <summary>
        /// Sprites used to display the character
        /// </summary>
        [SerializeField]
        protected CharacterSprite[] _sprites;

        /// <summary>
        /// A reference to the story database
        /// </summary>
        [SerializeField]
        protected StoryDatabase _storyDatabase;

        #endregion

        #region Public Properties

        /// <summary>
        /// The characters name displayed in the UI
        /// </summary>
        public string DisplayName => _displayName;

        /// <summary>
        /// The character's unique ID
        /// </summary>
        public string UniqueID => _uniqueID;

        /// <summary>
        /// The character's current location.
        /// </summary>
        public Location Location => _currentLocation;

        #endregion

        #region Unity Actions

        /// <summary>
        /// Action invoked whenever the character changes their location
        /// </summary>
        public UnityAction<Location> OnLocationChanged;

        #endregion

        #region Unity Lifecycle Methods

        private void Start()
        {
            _storyDatabase.DB.Insert($"{UniqueID}", true);
            if (_startingLocation != null)
            {
                MoveToLocation(_startingLocation);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Move an Actor to a new location.
        /// </summary>
        /// <param name="location"></param>
        public void MoveToLocation(Location location)
        {
            // Remove the character from their current location
            if (_currentLocation != null)
            {
                _currentLocation.RemoveCharacter(this);
                _storyDatabase.DB.Delete($"{_currentLocation.UniqueID}.characters.{UniqueID}");
                _storyDatabase.DB.Delete($"{UniqueID}.location.{_currentLocation.UniqueID}");
                _currentLocation = null;
            }

            if (location != null)
            {
                location.AddCharacter(this);
                _currentLocation = location;
                _storyDatabase.DB.Insert($"{location.UniqueID}.characters.{UniqueID}", true);
                _storyDatabase.DB.Insert($"{UniqueID}.location.{location.UniqueID}", true);
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
            foreach (var entry in _sprites)
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
