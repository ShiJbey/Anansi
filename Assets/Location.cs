using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;

namespace Calypso.Unity
{
    public class Location : MonoBehaviour
    {
        #region Fields
        [SerializeField]
        private string _displayName;

        [SerializeField]
        private string _uniqueID;

        [SerializeField]
        private List<LocationBackground> _backgrounds = new List<LocationBackground>();

        private Dictionary<string, LocationBackground> _bgDictionary = new Dictionary<string, LocationBackground>();

        private LocationBackground _currentBackground;

        private List<Actor> _actors = new List<Actor>();
        #endregion

        #region Properties
        public string DisplayName => _displayName;
        public string UniqueID => _uniqueID;
        public Actor[] ActorsPresent => _actors.ToArray();
        #endregion

        #region Unity Lifecycle Methods
        private void Awake()
        {
            foreach(var entry in _backgrounds)
            {
                _bgDictionary[entry.name] = entry;
            }

            _currentBackground = _bgDictionary["default"];
        }
        #endregion

        #region Methods
        /// <summary>
        /// Get the background sprite for the given name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Sprite GetBackgroundSprite(string name)
        {
            return _bgDictionary[name].sprite;
        }

        public Sprite GetCurrentBackground()
        {
            return _currentBackground.sprite;
        }

        /// <summary>
        /// Add an actor to this location
        /// </summary>
        /// <param name="actor"></param>
        public void AddActor(Unity.Actor actor)
        {
            _actors.Add(actor);
        }

        /// <summary>
        /// Remove an actor from this location
        /// </summary>
        /// <param name="actor"></param>
        /// <returns></returns>
        public bool RemoveActor(Unity.Actor actor)
        {
            return _actors.Remove(actor);
        }
        #endregion
    }

    [System.Serializable]
    public struct LocationBackground
    {
        public string name;
        public Sprite sprite;
    }

}
