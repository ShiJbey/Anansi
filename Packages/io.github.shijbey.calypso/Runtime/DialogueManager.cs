using UnityEngine;
using Calypso.Unity;
using System.Linq;
using System.Collections;

namespace Calypso
{
    /// <summary>
    /// Handles dialogue for the current conversation.
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        #region Protected Fields

        /// <summary>
        /// A reference to the Dialog Panel UI
        /// </summary>
        [SerializeField]
        private DialoguePanelController dialoguePanel;

        [SerializeField]
        private CharacterSpriteController characterSpriteController;

        [SerializeField]
        private BackgroundController backgroundController;

        [SerializeField]
        private LocationManager locationManager;

        [SerializeField]
        private CharacterManager characterManager;

        /// <summary>
        /// The current story displayed to the player
        /// </summary>
        private Ink.Runtime.Story _story = null;

        /// <summary>
        /// Reference to the coroutine responsible for displaying the dialogue of a story
        /// </summary>
        private Coroutine dialogueCoroutine = null;

        private bool advDialogueButtonPressed = false;

        private int userChoiceIndex = -1;

        #endregion

        #region Unity Lifecycle Methods

        private void Start()
        {
            dialoguePanel.OnAdvanceText.AddListener(() => { advDialogueButtonPressed = true; });
            dialoguePanel.ChoiceDialog.OnChoiceSelected.AddListener((idx) => userChoiceIndex = idx);
        }

        #endregion

        #region Methods

        /// <summary>
        /// A coroutine responsible for displaying text and handling user input.
        /// </summary>
        /// <returns></returns>
        private IEnumerator HandleConversation()
        {
            while (true)
            {
                if (_story.canContinue)
                {
                    // Display next line of dialogue
                    string text = _story.Continue(); // gets next line

                    text = text?.Trim(); // removes white space from text

                    foreach (string line in _story.currentTags)
                    {
                        if (line.Contains("speaker"))
                        {
                            string speakerName = line.Split(':')[1].Trim();
                            dialoguePanel.SetSpeakerName(speakerName);
                            continue;
                        }
                    }

                    dialoguePanel.DisplayText(text);

                    dialoguePanel.SetContinueButtonInteractable(true);

                    yield return new WaitUntil(() => advDialogueButtonPressed == true);

                    advDialogueButtonPressed = false;
                }
                else if (_story.currentChoices.Count > 0)
                {
                    dialoguePanel.SetContinueButtonInteractable(false);

                    // Display choices
                    dialoguePanel.ChoiceDialog.DisplayChoices(
                        _story.currentChoices.Select(c => c.text).ToArray()
                    );

                    yield return new WaitUntil(() => userChoiceIndex != -1);

                    _story.ChooseChoiceIndex(userChoiceIndex);

                    userChoiceIndex = -1;
                }
                else
                {
                    // End story
                    dialoguePanel.SetContinueButtonInteractable(false);
                    _story.UnbindExternalFunction("SetBackground");
                    _story.UnbindExternalFunction("SetSpeakerSprite");
                    _story.ResetState();
                    _story = null;
                    dialoguePanel.Hide();
                    break;
                }
            }
        }

        /// <summary>
        /// Start a new conversation using the given story.
        /// </summary>
        /// <param name="story"></param>
        public void StartConversation(Ink.Runtime.Story story)
        {
            _story = story;

            _story.BindExternalFunction("SetBackground", (string locationID, string tags) =>
            {
                Location location = locationManager.GetLocation(locationID);

                string[] tagsArr = tags.Split(",").Select(s => s.Trim()).Where(s => s != "").ToArray();

                backgroundController.ChangeBackground(location.GetBackground(tagsArr));
            });

            _story.BindExternalFunction("SetSpeakerSprite", (string characterID, string tags) =>
            {
                Actor character = characterManager.GetCharacter(characterID);

                string[] tagsArr = tags.Split(",").Select(s => s.Trim()).Where(s => s != "").ToArray();

                characterSpriteController.ChangeSpeaker(character.GetSprite(tagsArr));
            });

            if (dialogueCoroutine != null)
            {
                StopCoroutine(dialogueCoroutine);
            }

            dialogueCoroutine = StartCoroutine(HandleConversation());
        }

        #endregion
    }
}
