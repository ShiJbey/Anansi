using System.Collections.Generic;
using TDRS;
using UnityEngine;

namespace Calypso
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

        /// <summary>
        /// A reference to the game's social engine
        /// </summary>
        [SerializeField]
        protected SocialEngine m_socialEngine;

        /// <summary>
        /// A collection of background sprites associated with this location
        /// </summary>
        [SerializeField]
        protected LocationBackground[] m_backgrounds;

        /// <summary>
        /// Collection of all characters currently at this location
        /// </summary>
        protected List<Actor> m_actors = new List<Actor>();

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
        public IEnumerable<Actor> Characters => m_actors;

        #endregion

        #region Unity Messages

        void Start()
        {
            m_socialEngine.DB.Insert($"{UniqueID}");
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
            foreach (var entry in m_backgrounds)
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

        /// <summary>
        /// Remove a character from the location
        /// </summary>
        /// <param name="actor"></param>
        public void AddCharacter(Actor actor)
        {
            m_actors.Add(actor);
        }

        /// <summary>
        /// Add a character to the location
        /// </summary>
        /// <param name="actor"></param>
        public void RemoveCharacter(Actor actor)
        {
            m_actors.Remove(actor);
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
