using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace Calypso
{
    /// <summary>
    /// Handles dialogue for the current conversation.
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// A reference to the
        /// </summary>
        [SerializeField]
        private GameManager m_gameManager;

        [SerializeField]
        private TextAsset m_globalsFile;

        private DialogueVariableSync m_variableSync;

        /// <summary>
        /// The current story displayed to the player
        /// </summary>
        private StoryletInstance m_storyletInstance = null;

        private string m_currentSpeaker = "";

        private bool m_isFirstLine = false;

        #endregion

        #region Unity Messages

        private void Awake()
        {
            m_variableSync = new DialogueVariableSync();

            if (m_globalsFile != null)
            {
                Ink.Runtime.Story globalsStory = new Ink.Runtime.Story(m_globalsFile.text);
                foreach (string name in globalsStory.variablesState)
                {
                    Ink.Runtime.Object value =
                        globalsStory.variablesState.GetVariableWithName(name);

                    m_variableSync.Variables[name] = value;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get the next line of dialogue
        /// </summary>
        /// <returns></returns>
        public string GetNextLine()
        {
            string text = m_storyletInstance.Story.Continue(); // gets next line

            // text = text?.Trim(); // removes white space from text

            // Process tags
            ProcessTags(m_storyletInstance.Story.currentTags);

            if (m_isFirstLine) m_isFirstLine = false;

            return text;
        }

        /// <summary>
        /// Get the current choices for this dialogue
        /// </summary>
        /// <returns></returns>
        public string[] GetChoices()
        {
            return m_storyletInstance.Story.currentChoices.Select(choice => choice.text).ToArray();
        }

        /// <summary>
        /// Make a choice
        /// </summary>
        /// <param name="choiceIndex"></param>
        public void MakeChoice(int choiceIndex)
        {
            m_storyletInstance.Story.ChooseChoiceIndex(choiceIndex);
        }

        /// <summary>
        /// Check if the dialogue manager is waiting for the player to make a choice
        /// </summary>
        /// <returns></returns>
        public bool HasChoices()
        {
            return m_storyletInstance.Story.currentChoices.Count > 0;
        }

        /// <summary>
        /// Check if the dialogue can continue further
        /// </summary>
        /// <returns></returns>
        public bool CanContinue()
        {
            return m_storyletInstance.Story.canContinue;
        }

        /// <summary>
        /// Check if the dialogue has reached its end
        /// </summary>
        /// <returns></returns>
        public bool AtDialogueEnd()
        {
            return !CanContinue() && !HasChoices();
        }

        /// <summary>
        /// Reset the dialogue manager
        /// </summary>
        public void Reset()
        {
            if (m_storyletInstance != null)
            {
                m_gameManager.SetSpeaker(
                    (string)m_storyletInstance.Story.variablesState["speaker"]
                );
                m_currentSpeaker = "";
                m_variableSync.StopListening(m_storyletInstance.Story);
                m_storyletInstance.Story.onError -= HandleStoryErrors;
                m_storyletInstance.Story.UnbindExternalFunction("SetBackground");
                m_storyletInstance.Story.UnbindExternalFunction("SetSpeakerSprite");
                m_storyletInstance.Story.ResetState();
            }

            m_storyletInstance = null;
        }

        /// <summary>
        /// Set the current conversation
        /// </summary>
        /// <param name="story"></param>
        public void SetConversation(StoryletInstance storyletInstance)
        {
            Reset();

            m_storyletInstance = storyletInstance;
            m_storyletInstance.Storylet.IncrementTimesPlayed();
            m_storyletInstance.Storylet.ResetCooldown();

            m_storyletInstance.Story.onError += HandleStoryErrors;
            m_variableSync.StartListening(m_storyletInstance.Story);
            m_storyletInstance.InitializeStory();
            m_storyletInstance.Story.ChoosePathString(m_storyletInstance.KnotID);
            m_storyletInstance.Storylet.IncrementTimesPlayed();
            m_isFirstLine = true;

            m_currentSpeaker = (string)m_storyletInstance.Story.variablesState["speaker"];

            m_storyletInstance.Story.BindExternalFunction("SetBackground", (string locationID, string tags) =>
            {
                Location location = m_gameManager.Locations.GetLocation(locationID);

                string[] tagsArr = tags.Split(",").Select(s => s.Trim()).Where(s => s != "").ToArray();

                m_gameManager.UI_Controller.Background.SetBackground(location.GetBackground(tagsArr));
            });

            m_storyletInstance.Story.BindExternalFunction("SetSpeakerSprite", (string characterID, string tags) =>
            {
                Actor character = m_gameManager.Characters.GetCharacter(characterID);

                string[] tagsArr = tags.Split(",").Select(s => s.Trim()).Where(s => s != "").ToArray();

                m_gameManager.UI_Controller.CharacterSprite.SetSpeaker(character.GetSprite(tagsArr));
            });
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Process a list of Ink tags
        /// </summary>
        /// <param name="tags"></param>
        private void ProcessTags(IList<string> tags)
        {
            Queue<string> tagsQueue = new Queue<string>();
            if (tags != null)
            {
                for (int i = 0; i < tags.Count; i++)
                {
                    if (m_isFirstLine && i < m_storyletInstance.Storylet.FirstLineTagOffset)
                    {
                        continue;
                    }
                    tagsQueue.Enqueue(tags[i]);
                }
            }

            while (tagsQueue.Count > 0)
            {
                // Get the next line off the queue
                string line = tagsQueue.Dequeue().Trim();

                // Get the different parts of the line
                string[] parts = line.Split(">>").Select(s => s.Trim()).ToArray();

                if (parts.Length != 2)
                {
                    throw new System.ArgumentException(
                        $"Invalid expression '{line}' in knot '{m_storyletInstance.KnotID}'."
                    );
                }

                string command = parts[0];
                List<string> arguments = parts[1].Split(" ")
                    .Select(s => s.Trim()).ToList();

                switch (command)
                {
                    case "speaker":
                        string speakerID = arguments[0];

                        if (m_currentSpeaker != speakerID)
                        {
                            m_currentSpeaker = speakerID;
                            arguments.RemoveAt(0);
                            string[] speakerTags = arguments.ToArray();
                            m_gameManager.SetSpeaker(
                                speakerID, speakerTags
                            );
                        }

                        break;
                }

            }
        }

        /// <summary>
        /// Log story errors
        /// </summary>
        /// <param name="message"></param>
        /// <param name="errorType"></param>
        private void HandleStoryErrors(string message, Ink.ErrorType errorType)
        {
            if (errorType == Ink.ErrorType.Warning)
            {
                Debug.LogWarning(message);
            }
            else
            {
                Debug.LogError(message);
            }
        }

        #endregion
    }
}
