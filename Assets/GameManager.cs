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
        #endregion

        #region Actions and Events
        public static UnityAction<Unity.Location> OnPlayerLocationChanged;
        #endregion

        #region Unity Lifecylce Methods
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
                UpdateBackgroundImage(location);
                OnPlayerLocationChanged?.Invoke(location);
            };
        }

        // Start is called before the first frame update
        void Start()
        {
            // TESTING CODE
            // Select a character at random and initiate a conversation

            int selectedIndex = UnityEngine.Random.Range(0, _actors.Actors.Count);
            var selectedActor = _actors.Actors[selectedIndex];

            var conversation = selectedActor.GetComponent<ConversationManager>().SelectConversation(_storyDatabase);

            _dialogueManager.StartConversation(conversation);
        }

        private void FixedUpdate()
        {
            if (Input.GetKeyUp(KeyCode.Space))
            {
                _timeManager.AdvanceTime();
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

        private void UpdateBackgroundImage(Unity.Location location)
        {
            _dialogueManager.SetBackground(location.GetCurrentBackground());
        }
    }
}

