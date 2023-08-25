using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace Calypso.Unity
{
    public class Actor : MonoBehaviour
    {
        #region Fields

        [Header("Identifier Information")]
        [SerializeField]
        private string _displayName;

        [SerializeField]
        private string _uniqueID;

        [Space(5)]
        [Header("Mood")]
        [SerializeField]
        private CharacterMood _mood = CharacterMood.Neutral;

        [Space(5)]
        [Header("Positioning")]
        [SerializeField]
        private Location _startingLocation;

        // Set within Start method
        private Location _currentLocation;

        [Space(5)]
        [Header("Sprite Settings")]
        [SerializeField]
        private string _spriteSet = "default";

        [SerializeField]
        private List<CharacterSpriteSet> _sprites;

        private Sprite _currentSprite;

        // Filled within start using sprites provided in the inspector
        private Dictionary<string, Dictionary<CharacterMood, Sprite>> _spriteDictionary =
            new Dictionary<string, Dictionary<CharacterMood, Sprite>>();
        #endregion

        #region Properties
        public string DisplayName => _displayName;
        public string UniqueID => _uniqueID;
        public Sprite Sprite => _currentSprite;
        public Unity.Location Location => _currentLocation;
        #endregion

        #region Actions and Events
        public UnityAction<Location> OnLocationChanged;
        #endregion

        #region Unity Lifecycle Methods
        private void Awake()
        {
            foreach (var spriteSet in _sprites)
            {
                var moodDict = new Dictionary<CharacterMood, Sprite>();

                foreach (var entry in spriteSet.sprites)
                {
                    moodDict[entry.mood] = entry.sprite;
                }

                _spriteDictionary[spriteSet.name] = moodDict;
            }
        }

        private void Start()
        {
            if (_startingLocation != null)
            {
                MoveToLocation(_startingLocation);
            }

            SetSpriteSet(_spriteSet);
            SetMood(_mood);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Move an Actor to a new location.
        /// </summary>
        /// <param name="location"></param>
        public void MoveToLocation(Location location)
        {
            // Remove the character from their current location if they have one
            if (_currentLocation != null)
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

        /// <summary>
        /// Set the character's current mood and update their sprite
        /// </summary>
        /// <param name="mood"></param>
        public void SetMood(CharacterMood mood)
        {
            _mood = mood;

            if (_spriteDictionary[_spriteSet].ContainsKey(mood))
            {
                _currentSprite = _spriteDictionary[_spriteSet][mood];
            }
            else
            {
                Debug.LogError(
                    $"Cannot find sprite for '{mood}' in set '{_spriteSet}'.");
            }
        }

        /// <summary>
        /// Set the current sprite set used by the character
        /// </summary>
        /// <param name="name"></param>
        public void SetSpriteSet(string name)
        {
            if (_spriteDictionary.ContainsKey(name))
            {
                _spriteSet = name;
                _currentSprite = _spriteDictionary[name][_mood];
            }
            else
            {
                Debug.LogError(
                    $"Cannot find sprite set for '{name}'.");
            }
        }
        #endregion
    }

    [System.Serializable]
    public struct CharacterSpriteSet
    {
        public string name;
        public List<CharacterSprite> sprites;
    }

    [System.Serializable]
    public struct CharacterSprite
    {
        public CharacterMood mood;
        public Sprite sprite;
    }

    public enum CharacterMood
    {
        Neutral = 0,
        Smiling = 1,
        Scowling = 2,
        Sad = 3,
        Surprised = 4,
        Blushing = 5,
        Angry = 6,
        Laughing = 7,
        Embarrassed = 8
    }
}

