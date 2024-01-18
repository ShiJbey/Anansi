using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Calypso.Scheduling;
using TDRS;

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

        [SerializeField]
        private SocialEngine m_socialEngine;

        private bool m_isGameInitialized = false;

        private SpeakerSpriteController m_speakerSpriteController;

        private BackgroundSpriteController m_backgroundController;

        #endregion

        #region Properties

        public VNUIController UI_Controller => m_uiController;

        public CharacterManager Characters => m_characterManager;

        public LocationManager Locations => m_locationManager;

        public SpeakerSpriteController SpriteController => m_speakerSpriteController;

        #endregion

        #region Unity Messages

        private void Awake()
        {
            m_speakerSpriteController = GetComponent<SpeakerSpriteController>();
            if (m_speakerSpriteController == null)
            {
                throw new Exception(
                    $"${gameObject.name} is missing SpeakerSpriteController component."
                );
            }

            m_backgroundController = GetComponent<BackgroundSpriteController>();
            if (m_speakerSpriteController == null)
            {
                throw new Exception(
                    $"${gameObject.name} is missing BackgroundSpriteController component."
                );
            }
        }

        private void Update()
        {
            if (!m_isGameInitialized)
            {
                m_isGameInitialized = true;

                m_characterManager.InitializeLookUpTable();
                m_locationManager.InitializeLookUpTable();

                foreach (var character in m_characterManager.Characters)
                {
                    m_socialEngine.DB.Insert($"{character.UniqueID}");
                }

                foreach (var location in m_locationManager.Locations)
                {
                    m_socialEngine.DB.Insert($"{location.UniqueID}");
                }

                m_backgroundController.ResetBackgrounds();
                m_speakerSpriteController.ResetSprites();

                SetPlayerLocation(m_player.startingLocation);

                Tick();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Tick the simulation
        /// </summary>
        public void Tick()
        {
            m_timeManager.AdvanceTime();

            var currentDate = m_timeManager.DateTime;

            // Add the current date to the social engine's database

            m_socialEngine.DB.Insert(
                $"date.time_of_day!{Enum.GetName(typeof(TimeOfDay), currentDate.TimeOfDay)}");
            m_socialEngine.DB.Insert(
                $"date.day!{currentDate.Day}");
            m_socialEngine.DB.Insert(
                $"date.weekday!{Enum.GetName(typeof(WeekDay), currentDate.WeekDay)}");
            m_socialEngine.DB.Insert(
                $"date.week!{currentDate.Week}");

            m_uiController.StatusBar.SetDateTimeText(currentDate.ToString());

            // Reset who gets displayed
            m_displayedCharacter = null;
            m_speakerSpriteController.HideSpeaker();

            // Handle NPC schedules
            foreach (Actor character in m_characterManager.Characters)
            {
                if (character == m_player) continue;

                var scheduleManager = character.gameObject.GetComponent<ScheduleManager>();

                ScheduleEntry entry = scheduleManager.GetEntry(currentDate);

                if (entry == null) continue;

                character.SetLocation(
                    m_locationManager.GetLocation(entry.Location)
                );
            }

            if (m_displayedCharacter == null && m_player.Location != null)
            {
                var character = SelectDisplayedActor(m_player.Location);

                if (character == null)
                {
                    m_displayedCharacter = null;
                    m_speakerSpriteController.HideSpeaker();
                    return;
                };

                m_speakerSpriteController.SetSpeaker(character.UniqueID);
                m_uiController.DialoguePanel.SetSpeakerName(character.DisplayName);

                m_displayedCharacter = character;
            }
        }

        /// <summary>
        /// Method called when the player clicks the talk button in the Interaction panel
        /// </summary>
        public void StartConversation()
        {
            if (m_displayedCharacter == null) return;
            if (m_dialogueManager.IsDialogueActive == true) return;

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
        public void SetStoryLocation(string locationID, params string[] tags)
        {
            Location location = m_locationManager.GetLocation(locationID);
            m_backgroundController.SetBackground(locationID, tags);
            m_uiController.StatusBar.SetLocationText(location.DisplayName);
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

            if (location == null)
            {
                m_speakerSpriteController.HideSpeaker();
                m_uiController.DialoguePanel.SetSpeakerName("");
                m_displayedCharacter = null;
                return;
            }

            SetStoryLocation(location.UniqueID);

            // Select character they could talk to
            var character = SelectDisplayedActor(location);

            if (character == null)
            {
                m_displayedCharacter = null;
                m_speakerSpriteController.HideSpeaker();
                return;
            };

            m_speakerSpriteController.SetSpeaker(character.UniqueID);
            m_uiController.DialoguePanel.SetSpeakerName(character.DisplayName);

            m_displayedCharacter = character;
        }

        public void SetSpeaker(string characterID, params string[] tags)
        {
            var character = m_characterManager.GetCharacter(characterID);
            m_uiController.DialoguePanel.SetSpeakerName(character.DisplayName);
            m_speakerSpriteController.SetSpeaker(characterID, tags);
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
