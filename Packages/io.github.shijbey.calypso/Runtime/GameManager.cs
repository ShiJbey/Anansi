using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Calypso.Scheduling;
using Ink.Parsed;
using TDRS;
using System.ComponentModel.Design;

namespace Calypso
{
    public class GameManager : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// A reference to the player's character.
        /// </summary>
        [SerializeField]
        protected Actor m_player;

        /// <summary>
        /// A reference to the character currently displayed on the screen that the player has
        /// the option to talk to.
        /// </summary>
        protected Actor m_displayedCharacter;

        [SerializeField]
        protected VNUIController m_uiController;

        [SerializeField]
        private DialogueManager m_dialogueManager;

        [SerializeField]
        private CharacterManager m_characterManager;

        [SerializeField]
        private LocationManager m_locationManager;

        [SerializeField]
        private TimeManager m_timeManager;

        /// <summary>
        /// Is there currently dialogue being displayed on the screen
        /// </summary>
        private bool m_isDialogueRunning;

        [SerializeField]
        private SocialEngine m_socialEngine;

        #endregion

        #region Properties

        public VNUIController UI_Controller => m_uiController;

        public CharacterManager Characters => m_characterManager;

        public LocationManager Locations => m_locationManager;

        #endregion

        #region Unity Messages

        private void Start()
        {
            TimeManager.OnTimeChanged += (dateTime) =>
            {
                m_uiController.StatusBar.SetDateTimeText(dateTime.ToString());

                // Reset who gets displayed
                m_displayedCharacter = null;
                m_uiController.CharacterSprite.Hide();

                // Handle NPC schedules
                foreach (Actor character in m_characterManager.Characters)
                {
                    if (character == m_player) continue;

                    var scheduleManager = character.gameObject.GetComponent<ScheduleManager>();

                    ScheduleEntry entry = scheduleManager.GetEntry(dateTime);

                    if (entry == null) continue;

                    character.SetLocation(
                        m_locationManager.GetLocation(entry.Location)
                    );
                }
            };

            m_uiController.DialoguePanel.OnShow.AddListener(HandleDialogueEnter);
            m_uiController.DialoguePanel.OnHide.AddListener(HandleDialogueExit);

            m_uiController.StatusBar.SetDateTimeText(m_timeManager.DateTime.ToString());

            m_player.OnLocationChanged += (location) =>
            {
                if (location == null)
                {
                    m_uiController.CharacterSprite.Hide();
                    m_uiController.DialoguePanel.SetSpeakerName("");
                    m_displayedCharacter = null;
                    return;
                }

                m_uiController.Background.SetBackground(location.GetBackground());
                m_uiController.StatusBar.SetLocationText(location.DisplayName);

                // Select character they could talk to
                var character = SelectDisplayedActor(location);

                if (character == null)
                {
                    m_displayedCharacter = null;
                    m_uiController.CharacterSprite.Hide();
                    return;
                };

                m_uiController.CharacterSprite.SetSpeaker(character.GetSprite());
                m_uiController.DialoguePanel.SetSpeakerName(character.DisplayName);

                m_displayedCharacter = character;
            };
        }

        private void Update()
        {
            if (m_displayedCharacter == null && m_player.Location != null)
            {
                var character = SelectDisplayedActor(m_player.Location);

                if (character == null)
                {
                    m_displayedCharacter = null;
                    m_uiController.CharacterSprite.Hide();
                    return;
                };

                m_uiController.CharacterSprite.SetSpeaker(character.GetSprite());
                m_uiController.DialoguePanel.SetSpeakerName(character.DisplayName);

                m_displayedCharacter = character;
            }
        }

        private void OnDisable()
        {
            m_uiController.DialoguePanel.OnShow.RemoveListener(HandleDialogueEnter);
            m_uiController.DialoguePanel.OnHide.RemoveListener(HandleDialogueExit);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Method called when the player clicks the talk button in the Interaction panel
        /// </summary>
        public void StartConversation()
        {
            if (m_displayedCharacter == null) return;
            if (m_isDialogueRunning == true) return;

            List<StoryletInstance> instances = new List<StoryletInstance>();

            var playerStorylets = GetStoryletInstances(
                m_player.GetComponent<StoryletController>().Storylets,
                new Dictionary<string, string>()
                {
                    {"?player", m_player.UniqueID},
                    {"?speaker", m_displayedCharacter.UniqueID},
                    {"?location", m_player.Location.UniqueID}
                }
            );
            foreach (StoryletInstance instance in playerStorylets)
            {
                instances.Add(instance);
            }

            var npcStorylets = GetStoryletInstances(
                m_displayedCharacter.GetComponent<StoryletController>().Storylets,
                new Dictionary<string, string>()
                {
                    {"?player", m_player.UniqueID},
                    {"?speaker", m_displayedCharacter.UniqueID},
                    {"?location", m_player.Location.UniqueID}
                }
            );
            foreach (StoryletInstance instance in npcStorylets)
            {
                instances.Add(instance);
            }

            var locationStorylets = GetStoryletInstances(
                m_player.Location.GetComponent<StoryletController>().Storylets,
                new Dictionary<string, string>()
                {
                    {"?player", m_player.UniqueID},
                    {"?speaker", m_displayedCharacter.UniqueID},
                    {"?location", m_player.Location.UniqueID}
                }
            );
            foreach (StoryletInstance instance in locationStorylets)
            {
                instances.Add(instance);
            }

            if (instances.Count == 0) return;

            // Filter storylet instances for mandatory instances
            var mandatoryInstances = instances.Where(instance => instance.Storylet.IsMandatory)
                .ToList();

            if (mandatoryInstances.Count() > 0)
            {
                instances = mandatoryInstances;
            }

            StoryletInstance selectedStorylet =
                instances.RandomElementByWeight(entry => entry.Weight);

            m_dialogueManager.SetConversation(selectedStorylet);
            m_isDialogueRunning = true;
            m_uiController.DialoguePanel.AdvanceDialogue();
        }


        /// <summary>
        /// Checks if the player can talk to a present NPC
        /// </summary>
        /// <returns></returns>
        public bool CanPlayerTalk()
        {
            return m_displayedCharacter != null;
        }

        /// <summary>
        /// Check if the player can change location
        /// </summary>
        /// <returns></returns>
        public bool CanPlayerChangeLocation()
        {
            return true;
        }

        /// <summary>
        /// Sets the games background image to the given location
        /// </summary>
        /// <param name="location"></param>
        public void SetStoryLocation(Location location)
        {
            m_uiController.Background.SetBackground(location.GetBackground());
        }

        /// <summary>
        /// Get all locations that the player can travel to
        /// </summary>
        /// <returns></returns>
        public IList<Location> GetLocationsPlayerCanTravelTo()
        {
            return m_locationManager.Locations
                .Where(location => m_player.Location != location)
                .ToList();
        }

        /// <summary>
        /// Set the player's current location and change the background
        /// </summary>
        /// <param name="location"></param>
        public void SetPlayerLocation(Location location)
        {
            if (m_player.Location != location)
            {
                m_player.SetLocation(location);
            }
        }

        public void SetSpeaker(string characterID, params string[] tags)
        {
            var character = m_characterManager.GetCharacter(characterID);

            m_uiController.DialoguePanel.SetSpeakerName(character.DisplayName);
            m_uiController.CharacterSprite.SetSpeaker(character.GetSprite(tags));
        }

        #endregion

        #region Private Methods

        private void HandleDialogueEnter()
        {
            m_isDialogueRunning = true;
        }

        private void HandleDialogueExit()
        {
            m_isDialogueRunning = false;
        }

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

        private List<StoryletInstance> GetStoryletInstances(
            List<Storylet> storylets,
            Dictionary<string, string> bindings
        )
        {
            List<StoryletInstance> instances = new List<StoryletInstance>();

            foreach (var storylet in storylets)
            {
                // Skip storylets still on cooldown
                if (storylet.CooldownTimeRemaining > 0) continue;

                // Skip storylets that are not repeatable
                if (!storylet.IsRepeatable && storylet.TimesPlayed > 0) continue;

                // Query the social engine database
                if (storylet.Precondition != null)
                {
                    var results = storylet.Precondition.Run(m_socialEngine.DB, bindings);

                    if (!results.Success) continue;

                    foreach (var bindingDict in results.Bindings)
                    {
                        instances.Add(new StoryletInstance(
                        storylet,
                        bindingDict,
                        storylet.Weight
                    ));
                    }
                }
                else
                {
                    instances.Add(new StoryletInstance(
                        storylet,
                        bindings,
                        storylet.Weight
                    ));
                }
            }

            return instances;
        }

        #endregion
    }
}
