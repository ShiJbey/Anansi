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

        // We use this to sync information
        private GameManager _gameManager;
        #endregion

        #region Properties
        public string DisplayName => _displayName;
        public string UniqueID => _uniqueID;
        public Sprite Sprite => _currentSprite;
        public Location Location => _currentLocation;
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

            _gameManager = FindObjectOfType<GameManager>();

            if (_gameManager == null)
            {
                throw new System.NullReferenceException(
                    "Cannot find GameManager.");
            }

            // Register the character with the GameManager
            _gameManager.RegisterCharacter(this);
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
                _gameManager.Database.Remove($"{UniqueID}.location");
            }

            // Add the character to the new location
            if (location != null)
            {
                _currentLocation = location;
                location.AddActor(this);
                _gameManager.Database[$"{UniqueID}.location!{_currentLocation.UniqueID}"] = true;
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
            // Sync this information with the database
            _gameManager.Database[$"{UniqueID}.mood"] = _mood.ToString();


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

