using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Calypso.Unity;
using System.Linq;

namespace Calypso
{
    public class GameManager : MonoBehaviour
    {
        #region Fields
        [SerializeField]
        private ActorGroup _actors;

        [SerializeField]
        private DialogueManager _dialogueManager;

        private StoryDatabase _storyDatabase = new StoryDatabase();

        private TimeManager _timeManager;

        [SerializeField]
        private Calypso.Unity.Actor _player;

        private Calypso.Unity.Actor _displayedCharacter;
        #endregion

        #region Actions and Events
        public static UnityAction<Unity.Location> OnPlayerLocationChanged;
        #endregion

        #region Unity Lifecycle Methods
        private void OnEnable()
        {
            DialogueManager.OnConversationEnd += ClearDisplayedCharacter;
        }

        private void OnDisable()
        {
            DialogueManager.OnConversationEnd -= ClearDisplayedCharacter;
        }

        private void Awake()
        {
            _timeManager = GetComponent<TimeManager>();

            if (_timeManager == null)
            {
                throw new System.NullReferenceException("Cannot find time manager.");
            }

            _player.OnLocationChanged += (location) =>
            {
                Debug.Log($"Player moved to location, {location.DisplayName}");
                _dialogueManager.SetBackground(location.GetCurrentBackground());
                OnPlayerLocationChanged?.Invoke(location);

                // Select character they could talk to
                var character = SelectDisplayedActor(location);

                if (character == null) return;

                _dialogueManager.ShowCharacter(character);

                _displayedCharacter = character;

                var conversation = character.GetComponent<ConversationManager>().SelectConversation(_storyDatabase);

                _dialogueManager.StartConversation(conversation);
            };
        }

        private void Update()
        {
            if (_displayedCharacter == null)
            {
                if (Input.GetKeyDown(KeyCode.C))
                {
                    var character = SelectDisplayedActor(_player.Location);

                    if (character == null) return;

                    _dialogueManager.ShowCharacter(character);

                    _displayedCharacter = character;

                    var conversation = character.GetComponent<ConversationManager>().SelectConversation(_storyDatabase);

                    _dialogueManager.StartConversation(conversation);
                }
            }
        }
        #endregion


        /// <summary>
        /// Choose a random character at the location that the player
        /// can talk to
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        private Unity.Actor SelectDisplayedActor(Unity.Location location)
        {
            var potentialCharacters = location.ActorsPresent
                .Where((a) => !a.name.Equals("Player")).ToList();

            if (potentialCharacters.Count == 0) return null;

            int selectedIndex = UnityEngine.Random.Range(0, potentialCharacters.Count());
            var selectedActor = potentialCharacters[selectedIndex];

            return selectedActor;
        }

        private void ClearDisplayedCharacter()
        {
            _dialogueManager.HideCharacter();
            _displayedCharacter = null;
        }
    }
}

