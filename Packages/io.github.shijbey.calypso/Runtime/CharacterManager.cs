using System.Collections.Generic;
using UnityEngine;

namespace Calypso.Unity
{
    /// <summary>
    /// Maintains a look-up table of characters that exist in the game
    /// </summary>
    [DefaultExecutionOrder(-1)]
    public class CharacterManager : MonoBehaviour
    {
        /// <summary>
        /// The collection of characters that exist in the game (Set in Unity Inspector).
        /// </summary>
        [SerializeField]
        private List<Actor> characters = new List<Actor>();

        /// <summary>
        /// IDs of characters mapped to their instances.
        /// </summary>
        private Dictionary<string, Actor> characterLookupTable = new Dictionary<string, Actor>();

        public IEnumerable<Actor> Characters => characters;

        private void Start()
        {
            InitializeLookUpTable();
        }

        /// <summary>
        /// Fills the lookup table using entries from the characters collection.
        /// </summary>
        private void InitializeLookUpTable()
        {
            foreach (Actor character in characters)
            {
                characterLookupTable.Add(character.UniqueID, character);
            }
        }

        /// <summary>
        /// Get a character using their ID.
        /// </summary>
        /// <param name="characterID"></param>
        /// <returns></returns>
        public Actor GetCharacter(string characterID)
        {
            if (characterLookupTable.TryGetValue(characterID, out var character))
            {
                return character;
            }

            throw new KeyNotFoundException($"Could not find character with ID: {characterID}");
        }
    }
}
