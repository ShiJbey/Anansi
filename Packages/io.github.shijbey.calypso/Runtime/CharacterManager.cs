using System.Collections.Generic;
using UnityEngine;

namespace Calypso
{
    /// <summary>
    /// Maintains a look-up table of characters that exist in the game
    /// </summary>
    [DefaultExecutionOrder(-1)]
    public class CharacterManager : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// The collection of characters that exist in the game.
        /// </summary>
        [SerializeField]
        private List<Actor> m_characters;

        /// <summary>
        /// A look-up table of actor IDs mapped to their instances.
        /// </summary>
        private Dictionary<string, Actor> m_characterLookupTable;

        #endregion

        #region Properties

        /// <summary>
        /// All the characters registered with the manager
        /// </summary>
        public IEnumerable<Actor> Characters => m_characters;

        #endregion

        #region Unity Messages

        private void Awake()
        {
            m_characterLookupTable = new Dictionary<string, Actor>();
        }

        private void Start()
        {
            InitializeLookUpTable();
        }

        #endregion

        #region Public Methods

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

        #endregion

        #region Private Methods

        /// <summary>
        /// Fills the lookup table using entries from the characters collection.
        /// </summary>
        private void InitializeLookUpTable()
        {
            foreach (Actor character in m_characters)
            {
                m_characterLookupTable.Add(character.UniqueID, character);
            }
        }

        #endregion
    }
}
