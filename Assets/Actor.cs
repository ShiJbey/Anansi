using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace Calypso.Unity
{
    public class Actor : MonoBehaviour
    {
        #region Fields
        [SerializeField]
        private string _displayName;

        [SerializeField]
        private string _uniqueID;

        [SerializeField]
        private List<ActorSprite> _sprites = new List<ActorSprite>();

        [SerializeField]
        private Location _startingLocation;

        // Set within Start method
        private Location _currentLocation;

        // Filled within start using sprites provided in the inspector
        private Dictionary<string, ActorSprite> _spriteDictionary = new Dictionary<string, ActorSprite>();
        #endregion

        #region Properties
        public string DisplayName { get { return _displayName; } }

        public string UniqueID { get { return _uniqueID;} }
        #endregion

        #region Actions and Events
        public UnityAction<Location> OnLocationChanged;
        #endregion

        #region Unity Lifecycle Methods
        private void Awake()
        {
            foreach(var sprite in _sprites)
            {
                _spriteDictionary[sprite.name] = sprite;
            }
        }

        private void Start()
        {
            if (_startingLocation != null)
            {
                MoveToLocation(_startingLocation);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Get the Sprite mapped to the given name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Sprite GetSprite(string name)
        {
            return _spriteDictionary[name].sprite;
        }

        /// <summary>
        /// Move an Actor to a new location.
        /// </summary>
        /// <param name="location"></param>
        public void MoveToLocation(Location location)
        {
            // Remove the character from their current location if they have one
            if (_currentLocation !=  null)
            {
                _currentLocation.RemoveActor(this);
            }

            // Add the character to the new location
            if (location != null)
            {
                _currentLocation = location;
                location.AddActor(this);
            }

            OnLocationChanged?.Invoke(location);
        }
        #endregion
    }

    [System.Serializable]
    public struct ActorSprite
    {
        public string name;
        public Sprite sprite;
    }
}

