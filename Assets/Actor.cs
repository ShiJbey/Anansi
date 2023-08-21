using System.Collections.Generic;
using UnityEngine;


namespace Calypso.Unity
{
    public class Actor : MonoBehaviour
    {
        [SerializeField]
        private string _displayName;

        [SerializeField]
        private string _uniqueID;

        [SerializeField]
        private List<ActorSprite> _sprites = new List<ActorSprite>();

        // Filled within start using sprites provided in the inspector
        private Dictionary<string, ActorSprite> _spriteDictionary = new Dictionary<string, ActorSprite>();

        public string DisplayName { get { return _displayName; } }

        public string UniqueID { get { return _uniqueID;} }

        private void Awake()
        {
            foreach(var sprite in _sprites)
            {
                _spriteDictionary[sprite.name] = sprite;
            }
        }

        /// <summary>
        /// Get the Sprite mapped to the given name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Sprite GetSprite(string name)
        {
            return _spriteDictionary[name].sprite;
        }
    }

    [System.Serializable]
    public struct ActorSprite
    {
        public string name;
        public Sprite sprite;
    }
}

