using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Calypso.Unity;
using Calypso.RePraxis;
using System.Linq;

namespace Calypso.Unity
{
    public class GameManager : MonoBehaviour
    {
        #region Protected Fields

        /// <summary>
        /// A reference to the player's character.
        /// </summary>
        [SerializeField]
        protected Actor _player;

        /// <summary>
        /// A reference to the character currently displayed on the screen that the player has
        /// the option to talk to.
        /// </summary>
        protected Actor _displayedCharacter;

        [SerializeField]
        protected DialogueManager _dialogueManager;

        #endregion

        #region Unity Actions

        /// <summary>
        /// An action invoked whenever the player changes location.
        /// </summary>
        public static UnityAction<Location> OnPlayerLocationChanged;

        #endregion

        #region Unity Lifecycle Methods

        private void Start()
        {
            // _player.OnLocationChanged += (location) =>
            // {
            //     // Debug.Log($"Player moved to location, {location.DisplayName}");
            //     _dialogueManager.SetBackground(location.GetBackground());
            //     if (OnPlayerLocationChanged != null) OnPlayerLocationChanged.Invoke(location);

            //     // Select character they could talk to
            //     var character = SelectDisplayedActor(location);

            //     if (character == null) return;

            //     _dialogueManager.ShowCharacter(character);

            //     _displayedCharacter = character;
            // };
        }

        private void Update()
        {
            // if (_displayedCharacter == null && _player.Location != null)
            // {
            //     var character = SelectDisplayedActor(_player.Location);

            //     if (character == null) return;

            //     _dialogueManager.ShowCharacter(character);

            //     _displayedCharacter = character;
            // }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Choose a random character at the location that the player
        /// can talk to
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        private Actor SelectDisplayedActor(Location location)
        {
            var potentialCharacters = location.Characters
                .Where((a) => !a.name.Equals("Player")).ToList();

            if (potentialCharacters.Count == 0) return null;

            int selectedIndex = UnityEngine.Random.Range(0, potentialCharacters.Count());
            var selectedActor = potentialCharacters[selectedIndex];

            return selectedActor;
        }

        #endregion
    }
}
