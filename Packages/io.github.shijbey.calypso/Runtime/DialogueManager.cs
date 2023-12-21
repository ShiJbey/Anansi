using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using Ink.Runtime;

namespace Calypso
{
    /// <summary>
    /// Handles dialogue for the current conversation. The DialogManager updates the
    /// UI to reflect the current story state.
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        #region Fields
        [SerializeField]
        private TMP_Text _dialogText;

        [SerializeField]
        private GameObject _dialoguePanel;

        [SerializeField]
        private TMP_Text _speakerName;

        [SerializeField]
        private Image _speakerSprite;

        [SerializeField]
        private RectTransform _choiceButtonContainer;

        [SerializeField]
        private Button _choiceButtonPrefab;

        [SerializeField]
        private GameObject _choicePanel;

        [SerializeField]
        private List<TextStyle> _textStyles = new List<TextStyle>();

        [SerializeField]
        private Image _backgroundImage;

        [SerializeField]
        private GameObject _interactionPanel;

        [SerializeField]
        private Button _talkButton;

        /// <summary>
        /// A copy of the textStyles list indexed by name
        /// </summary>
        private Dictionary<string, TextStyle> _textStylesDictionary = new Dictionary<string, TextStyle>();

        /// <summary>
        /// The current conversation presented to the player
        /// </summary>
        private Conversation _currentConversation;
        #endregion

        #region Actions and Events
        public static UnityAction OnConversationStart;
        public static UnityAction OnConversationEnd;
        #endregion

        #region Unity Lifecycle Methods
        private void Awake()
        {
            foreach (var textStyle in _textStyles)
            {
                _textStylesDictionary[textStyle.name] = textStyle;
            }
        }

        private void Start()
        {
            // _interactionPanel.SetActive(true);
            // _talkButton.onClick.AddListener(StartConversation)
        }
        #endregion

        #region Dialogue Management Methods
        public void StartConversation(Conversation conversation)
        {
            OnConversationStart?.Invoke();
            ShowDialoguePanel();
            Debug.Log($"Started conversation ({conversation.ID}) with {conversation.Speaker.DisplayName}.");
            _currentConversation = conversation;
            AdvanceDialogue();
        }

        public void EndConversation()
        {
            Debug.Log(
                $"Started conversation ({_currentConversation.ID}) with {_currentConversation.Speaker.DisplayName}.");
            _currentConversation.Story.ResetState();
            _currentConversation = null;
            HideDialoguePanel();
            OnConversationEnd?.Invoke();
        }

        /// <summary>
        /// Returns true if we are at the end of the dialogue
        /// </summary>
        /// <returns></returns>
        public bool AtDialogueEnd()
        {
            var hasChoices = _currentConversation.Story.currentChoices.Count > 0;
            return !hasChoices && !_currentConversation.Story.canContinue;
        }

        /// <summary>
        /// Checks if the player can advance the dialogue
        /// </summary>
        /// <returns></returns>
        public bool CanAdvanceDialogue()
        {
            if (_currentConversation == null)
            {
                return false;
            }

            if (AtDialogueEnd())
            {
                // Yes, this means we have reached the end of the conversation
                return true;
            }

            return _currentConversation.Story.canContinue;
        }

        /// <summary>
        /// Advances the dialog of the conversation
        /// </summary>
        public void AdvanceDialogue()
        {
            if (AtDialogueEnd())
            {
                EndConversation();
            }
            else if (CanAdvanceDialogue())
            {
                string text = _currentConversation.Story.Continue(); // gets next line

                text = text?.Trim(); // removes white space from text
                string style = "default";

                foreach (string line in _currentConversation.Story.currentTags)
                {
                    if (line.Contains("speaker"))
                    {
                        string speakerName = line.Split(':')[1].Trim();
                        SetSpeakerName(speakerName);
                        continue;
                    }

                    if (line.Contains("thought"))
                    {
                        style = "thought";
                        continue;
                    }
                }

                SetDialogText(text, style);

                if (_currentConversation.Story.currentChoices.Count > 0)
                {
                    DisplayChoices();
                }
            }
            else if (_currentConversation.Story.currentChoices.Count > 0)
            {
                DisplayChoices();
            }
        }
        #endregion

        #region Character Sprite Methods
        /// <summary>
        /// Display the speaker's sprite
        /// </summary>
        public void ShowCharacter(Unity.Actor actor)
        {
            _speakerSprite.sprite = actor.GetSprite();
            _speakerSprite.gameObject.SetActive(true);
        }

        /// <summary>
        /// Hide the speaker's sprite
        /// </summary>
        public void HideCharacter()
        {
            _speakerSprite.sprite = null;
            _speakerSprite.gameObject.SetActive(false);
        }
        #endregion

        #region Dialog HUD Methods
        /// <summary>
        /// Hide the dialog box
        /// </summary>
        public void HideDialoguePanel()
        {
            // _dialoguePanel.SetActive(false);
        }

        /// <summary>
        /// Display the dialog box
        /// </summary>
        public void ShowDialoguePanel()
        {
            // _dialoguePanel.SetActive(true);
        }

        /// <summary>
        /// Display the current choices available to the player
        /// </summary>
        private bool DisplayChoices()
        {
            // checks if choices are already being displayed
            if (_choiceButtonContainer.GetComponentsInChildren<Button>().Length > 0)
            {
                Debug.Log("Choices already displayed.");
                return false;
            }

            ShowChoicePanel();

            for (int i = 0; i < _currentConversation.Story.currentChoices.Count; i++) // iterates through all choices
            {
                var choice = _currentConversation.Story.currentChoices[i];
                Debug.Log(choice.text);
                var button = AddChoice(choice.text, () =>
                {
                    _currentConversation.Story.ChooseChoiceIndex(choice.index); // tells ink which choice was selected
                    ClearChoices(); // removes choices from the screen
                    HideChoicePanel();
                    AdvanceDialogue();
                }); // creates a choice button
            }

            return true;
        }

        /// <summary>
        /// Add a choice button to the displayed choices
        /// </summary>
        /// <param name="text"></param>
        /// <param name="onClick"></param>
        /// <returns></returns>
        public Button AddChoice(string text, UnityAction onClick)
        {
            // creates the button from a prefab
            var choiceButton = Instantiate(_choiceButtonPrefab);
            choiceButton.transform.SetParent(_choiceButtonContainer.transform, false);

            // sets text on the button
            var buttonText = choiceButton.GetComponentInChildren<TMP_Text>();
            buttonText.text = text;

            //  Adds the onClick callback
            choiceButton.onClick.AddListener(onClick);

            return choiceButton;
        }

        /// <summary>
        /// Destroy all current choice buttons
        /// </summary>
        public void ClearChoices()
        {
            foreach (var button in _choiceButtonContainer.GetComponentsInChildren<Button>())
            {
                Destroy(button.gameObject);
            }
        }

        /// <summary>
        /// Displays the choice dialog box
        /// </summary>
        public void ShowChoicePanel()
        {
            _choicePanel.SetActive(true);
        }

        /// <summary>
        /// Hide the choice dialog box
        /// </summary>
        public void HideChoicePanel()
        {
            _choicePanel.SetActive(false);
        }

        /// <summary>
        /// Set the current text in the dialog box
        /// </summary>
        /// <param name="text"></param>
        /// <param name="style"></param>
        public void SetDialogText(string text, string style = "default")
        {
            SetTextStyle(style);
            _dialogText.text = text;
        }

        /// <summary>
        /// Set the current font color and font style for the dialog box
        /// </summary>
        /// <param name="styleName"></param>
        public void SetTextStyle(string styleName)
        {
            if (_textStylesDictionary.ContainsKey(styleName))
            {
                var entry = _textStylesDictionary[styleName];
                _dialogText.fontStyle = entry.fontStyle;
                _dialogText.color = entry.fontColor;
            }
            else
            {
                Debug.LogWarning(
                    $"Unknown text style '{styleName}'. Reverting to defaults.");
                _dialogText.fontStyle = FontStyles.Normal;
                _dialogText.color = Color.white;
            }
        }

        /// <summary>
        /// Set the name in the dialog box of the character speaking
        /// </summary>
        /// <param name="name"></param>
        public void SetSpeakerName(string name)
        {
            _speakerName.text = name;
        }
        #endregion

        #region Background Image Methods
        /// <summary>
        /// Set the displayed background image
        /// </summary>
        /// <param name="image"></param>
        public void SetBackground(Sprite image)
        {
            _backgroundImage.sprite = image;
        }
        #endregion
    }

    [System.Serializable]
    public struct TextStyle
    {
        [SerializeField]
        public string name;

        [SerializeField]
        public Color fontColor;

        [SerializeField]
        public FontStyles fontStyle;
    }
}
