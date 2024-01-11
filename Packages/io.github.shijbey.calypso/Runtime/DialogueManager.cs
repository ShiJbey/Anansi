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

        /// <summary>
        /// The current story displayed to the player
        /// </summary>
        private Ink.Runtime.Story m_story = null;

        #endregion

        #region Public Methods

        /// <summary>
        /// Get the next line of dialogue
        /// </summary>
        /// <returns></returns>
        public string GetNextLine()
        {
            string text = m_story.Continue(); // gets next line

            // text = text?.Trim(); // removes white space from text

            // Process tags
            ProcessTags(m_story.currentTags);

            return text;
        }

        /// <summary>
        /// Get the current choices for this dialogue
        /// </summary>
        /// <returns></returns>
        public string[] GetChoices()
        {
            return m_story.currentChoices.Select(choice => choice.text).ToArray();
        }

        /// <summary>
        /// Make a choice
        /// </summary>
        /// <param name="choiceIndex"></param>
        public void MakeChoice(int choiceIndex)
        {
            m_story.ChooseChoiceIndex(choiceIndex);
        }

        /// <summary>
        /// Check if the dialogue manager is waiting for the player to make a choice
        /// </summary>
        /// <returns></returns>
        public bool HasChoices()
        {
            return m_story.currentChoices.Count > 0;
        }

        /// <summary>
        /// Check if the dialogue can continue further
        /// </summary>
        /// <returns></returns>
        public bool CanContinue()
        {
            return m_story.canContinue;
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
            if (m_story != null)
            {
                m_story.UnbindExternalFunction("SetBackground");
                m_story.UnbindExternalFunction("SetSpeakerSprite");
                m_story.ResetState();
            }

            m_story = null;
        }

        /// <summary>
        /// Set the current conversation
        /// </summary>
        /// <param name="story"></param>
        public void SetConversation(Ink.Runtime.Story story)
        {
            Reset();

            m_story = story;

            m_story.BindExternalFunction("SetBackground", (string locationID, string tags) =>
            {
                Location location = m_gameManager.Locations.GetLocation(locationID);

                string[] tagsArr = tags.Split(",").Select(s => s.Trim()).Where(s => s != "").ToArray();

                m_gameManager.UI_Controller.Background.SetBackground(location.GetBackground(tagsArr));
            });

            m_story.BindExternalFunction("SetSpeakerSprite", (string characterID, string tags) =>
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
            foreach (string line in tags)
            {
                if (line.Contains("speaker"))
                {
                    string speakerName = line.Split(':')[1].Trim();
                    m_gameManager.UI_Controller.DialoguePanel.SetSpeakerName(speakerName);
                    continue;
                }
            }
        }

        #endregion
    }
}
