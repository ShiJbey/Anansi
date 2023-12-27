using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Calypso.Scheduling;

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

        [SerializeField]
        private BackgroundController backgroundController;

        [SerializeField]
        private DialoguePanelController dialoguePanelController;

        [SerializeField]
        private CharacterSpriteController characterSpriteController;

        [SerializeField]
        private StatusBarController statusBarController;

        [SerializeField]
        private InteractionPanelController interactionPanel;

        [SerializeField]
        private CharacterManager characterManager;

        [SerializeField]
        private LocationManager locationManager;

        [SerializeField]
        private LocationSelectionDialogController locationSelectionMenu;

        private TimeManager timeManager;

        #endregion

        #region Unity Lifecycle Methods

        private void Start()
        {
            timeManager = GetComponent<TimeManager>();

            TimeManager.OnTimeChanged += (dateTime) =>
            {
                statusBarController.SetDateTimeText(dateTime.ToString());

                // Reset who gets displayed
                _displayedCharacter = null;
                interactionPanel.SetTalkButtonEnabled(false);
                characterSpriteController.Hide();

                // Handle NPC schedules
                foreach (Actor character in characterManager.Characters)
                {
                    if (character == _player) continue;

                    var scheduleManager = character.gameObject.GetComponent<ScheduleManager>();

                    ScheduleEntry entry = scheduleManager.GetEntry(dateTime);

                    if (entry == null) continue;

                    character.MoveToLocation(
                        locationManager.GetLocation(entry.Location)
                    );
                }
            };

            statusBarController.SetDateTimeText(timeManager.Date.ToString());

            _player.OnLocationChanged += (location) =>
            {
                if (location == null)
                {
                    characterSpriteController.Hide();
                    dialoguePanelController.SetSpeakerName("");
                    _displayedCharacter = null;
                    interactionPanel.SetTalkButtonEnabled(false);
                    return;
                }

                backgroundController.ChangeBackground(location.GetBackground());
                statusBarController.SetLocationText(location.DisplayName);

                // Select character they could talk to
                var character = SelectDisplayedActor(location);

                if (character == null)
                {
                    _displayedCharacter = null;
                    interactionPanel.SetTalkButtonEnabled(false);
                    characterSpriteController.Hide();
                    return;
                };

                characterSpriteController.ChangeSpeaker(character.GetSprite());
                dialoguePanelController.SetSpeakerName(character.DisplayName);

                _displayedCharacter = character;
                interactionPanel.SetTalkButtonEnabled(true);
            };
            locationSelectionMenu.OnPlayerChangeLocation.AddListener(ChangePlayerLocation);
        }

        private void Update()
        {
            if (_displayedCharacter == null && _player.Location != null)
            {
                var character = SelectDisplayedActor(_player.Location);

                if (character == null)
                {
                    _displayedCharacter = null;
                    interactionPanel.SetTalkButtonEnabled(false);
                    characterSpriteController.Hide();
                    return;
                };

                characterSpriteController.ChangeSpeaker(character.GetSprite());
                dialoguePanelController.SetSpeakerName(character.DisplayName);

                _displayedCharacter = character;
                interactionPanel.SetTalkButtonEnabled(true);
            }
        }

        /// <summary>
        /// Method called when the player clicks the talk button in the Interaction panel
        /// </summary>
        public void StartConversation()
        {
            if (_displayedCharacter == null) return;

            // This is where we combine storylets from the locations, player, and npc.
            IEnumerable<Storylet> allStorylets = new List<Storylet>()
                .Concat(_player.GetComponent<StoryletController>().GetStorylets())
                .Concat(_player.Location.GetComponent<StoryletController>().GetStorylets())
                .Concat(_displayedCharacter.GetComponent<StoryletController>().GetStorylets())
                .ToList();

            // Now run queries for all storylets, see if they are runnable, and calculate their
            // weights
            List<(float, StoryletInstance)> weightedStorylets =
                new List<(float, StoryletInstance)>();

            foreach (Storylet storylet in allStorylets)
            {
                weightedStorylets.Add((1.0f, new StoryletInstance(storylet)));
            }

            if (weightedStorylets.Count == 0) return;

            var mandatoryStorylets = weightedStorylets.Where((s) => s.Item2.Storylet.Mandatory).ToList();

            if (mandatoryStorylets.Count() > 0)
            {
                weightedStorylets = mandatoryStorylets;
            }

            StoryletInstance selectedStorylet =
                weightedStorylets.RandomElementByWeight(entry => entry.Item1).Item2;


            // Add error message handling to loaded stories
            selectedStorylet.Storylet.Story.onError += (msg, type) =>
                            {
                                if (type == Ink.ErrorType.Warning)
                                    Debug.LogWarning(msg);
                                else
                                    Debug.LogError(msg);
                            };

            selectedStorylet.Storylet.Story.ResetState();
            selectedStorylet.Storylet.Story.ChoosePathString(selectedStorylet.Storylet.KnotID);

            selectedStorylet.Storylet.IncrementTimesPlayed();

            _dialogueManager.StartConversation(selectedStorylet.Storylet.Story);
        }

        #endregion

        #region Public Methods

        public void ChangePlayerLocation(string locationID)
        {
            if (_player.Location && _player.Location.UniqueID == locationID) return;

            Location location = locationManager.GetLocation(locationID);
            _player.MoveToLocation(location);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Decrement the cooldowns of all storylets.
        /// </summary>
        // private void DecrementCooldowns()
        // {
        //     foreach (Storylet storylet in _storylets)
        //     {
        //         if (storylet.CooldownTimeRemaining > 0)
        //         {
        //             storylet.DecrementCooldown();
        //         }
        //     }
        // }


        /// <summary>
        /// Choose a random character at the location that the player
        /// can talk to
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        private Actor SelectDisplayedActor(Location location)
        {
            var potentialCharacters = location.Characters
                .Where(a => a.UniqueID != "player").ToList();

            if (potentialCharacters.Count == 0) return null;

            int selectedIndex = UnityEngine.Random.Range(0, potentialCharacters.Count());
            var selectedActor = potentialCharacters[selectedIndex];

            return selectedActor;
        }

        #endregion
    }
}
