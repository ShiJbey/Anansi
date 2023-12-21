using System.Collections.Generic;
using UnityEngine;

namespace Calypso.Unity
{
    /// <summary>
    /// A place where characters can be. Locations are displayed as the background images
    /// in the game.
    /// </summary>
    public class Location : MonoBehaviour
    {
        #region Protected Fields

        /// <summary>
        /// The name of the location as displayed in the UI.
        /// </summary>
        [SerializeField]
        protected string _displayName;

        /// <summary>
        /// An ID that uniquely identifies this location among other locations in the game.
        /// </summary>
        [SerializeField]
        protected string _uniqueID;

        /// <summary>
        /// A reference to story database
        /// </summary>
        [SerializeField]
        protected StoryDatabase _storyDatabase;

        /// <summary>
        /// A collection of background sprites associated with this location
        /// </summary>
        [SerializeField]
        protected LocationBackground[] _backgrounds;

        /// <summary>
        /// Collection of all characters currently at this location
        /// </summary>
        protected List<Actor> _actors = new List<Actor>();

        #endregion

        #region Public Properties

        /// <summary>
        /// The name of the location as displayed in the UI.
        /// </summary>
        public string DisplayName => _displayName;

        /// <summary>
        /// An ID that uniquely identifies this location among other locations in the game.
        /// </summary>
        public string UniqueID => _uniqueID;

        /// <summary>
        /// Collection of all characters currently at this location
        /// </summary>
        public IEnumerable<Actor> Characters => _actors;

        #endregion

        #region Unity Lifecycle Methods

        void Start()
        {
            _storyDatabase.db.Add($"{UniqueID}", true);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get a background sprite with a given set of tags.
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public Sprite GetBackground(params string[] tags)
        {
            foreach (var entry in _backgrounds)
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

        public void AddCharacter(Actor actor)
        {
            _actors.Add(actor);
        }

        public void RemoveCharacter(Actor actor)
        {
            _actors.Remove(actor);
        }
        #endregion

        #region Helper Classes

        /// <summary>
        /// Associates a background sprite with a collection of descriptive tags
        /// </summary>
        [System.Serializable]
        public class LocationBackground
        {
            /// <summary>
            /// The sprite to display.
            /// </summary>
            public Sprite sprite;

            /// <summary>
            /// Tags describing the sprite.
            /// </summary>
            public string[] tags;
        }

        #endregion
    }
}
