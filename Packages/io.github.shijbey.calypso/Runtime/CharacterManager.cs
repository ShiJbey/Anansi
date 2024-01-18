using System.Collections.Generic;
using UnityEngine;

namespace Calypso
{
    /// <summary>
    /// Maintains a look-up table of characters that exist in the game
    /// </summary>
    public class CharacterManager : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// A look-up table of actor IDs mapped to their instances.
        /// </summary>
        private Dictionary<string, Actor> m_characterLookupTable;

        #endregion

        #region Properties

        /// <summary>
        /// All the characters registered with the manager
        /// </summary>
        public IEnumerable<Actor> Characters => m_characterLookupTable.Values;

        #endregion

        #region Unity Messages

        private void Awake()
        {
            m_characterLookupTable = new Dictionary<string, Actor>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Fills the lookup table using entries from the characters collection.
        /// </summary>
        public void InitializeLookUpTable()
        {
            var characters = FindObjectsOfType<Actor>();
            foreach (Actor character in characters)
            {
                AddCharacter(character);
            }
        }

        /// <summary>
        /// Get a character using their ID.
        /// </summary>
        /// <param name="characterID"></param>
        /// <returns></returns>
        public Actor GetCharacter(string characterID)
        {
            if (m_characterLookupTable.TryGetValue(characterID, out var character))
            {
                return character;
            }

            throw new KeyNotFoundException($"Could not find character with ID: {characterID}");
        }

        /// <summary>
        /// Add a character to the manager
        /// </summary>
        /// <param name="character"></param>
        public void AddCharacter(Actor character)
        {
            m_characterLookupTable.Add(character.UniqueID, character);
        }

        #endregion
    }
}
