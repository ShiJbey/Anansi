using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using System;

namespace Calypso.Unity
{
    /// <summary>
    /// Controls the text box that displays dialogue text and choices to the player
    /// </summary>
    public class DialoguePanelController : MonoBehaviour
    {
        #region Protected Fields

        /// <summary>
        /// A reference to the button that advances to the next set of dialogue
        /// </summary>
        [SerializeField]
        private Button advanceDialogueButton;

        /// <summary>
        /// A reference to the component displaying the dialogue text.
        /// </summary>
        [SerializeField]
        private TMP_Text _dialogueText;

        /// <summary>
        /// A reference to the component displaying the speaker's name.
        /// </summary>
        [SerializeField]
        private TMP_Text _speakerName;

        /// <summary>
        /// The on-screen position of the dialogue panel.
        /// </summary>
        private Vector3 onScreenPosition;

        /// <summary>
        /// The off-screen position of the dialogue panel.
        /// </summary>
        private Vector3 offScreenPosition;

        /// <summary>
        /// A reference to GameObject that manages choice buttons.
        /// </summary>
        [SerializeField]
        private ChoiceDialogController _choiceDialog;

        /// <summary>
        /// A reference to the coroutine that handles sliding the dialogue panel on and off screen
        /// </summary>
        private Coroutine panelSlideCoroutine = null;

        /// <summary>
        /// Tracks if the panel is hidden or moving into the hidden state.
        /// </summary>
        private bool panelHidden = false;

        /// <summary>
        /// The fade-out duration time in seconds.
        /// </summary>
        [SerializeField]
        protected float slideOutSeconds;

        /// <summary>
        /// The fade-in duration time in seconds.
        /// </summary>
        [SerializeField]
        protected float slideInSeconds;

        [SerializeField]
        private RectTransform rectTransform;

        #endregion

        #region Unity Events

        public UnityEvent OnAdvanceText;

        public UnityEvent OnShow;

        public UnityEvent OnHide;

        #endregion

        #region Unity Lifecycle Methods

        // Start is called before the first frame update
        private void Start()
        {
            Vector3 startingPos = rectTransform.position;
            onScreenPosition = new Vector3(startingPos.x, startingPos.y, startingPos.z);

            offScreenPosition = new Vector3(
                startingPos.x,
                startingPos.y - (rectTransform.rect.height + 200),
                startingPos.z
            );

            advanceDialogueButton.onClick.AddListener(() =>
            {
                if (OnAdvanceText != null) OnAdvanceText.Invoke();
            });

            Hide();
        }

        #endregion

        public ChoiceDialogController ChoiceDialog
        {
            get
            {
                if (_choiceDialog != null) return _choiceDialog;
                throw new NullReferenceException("ChoiceDialog for DialoguePanel is null.");
            }
        }

        #region Public Methods

        /// <summary>
        /// Set the name in the dialog box of the character speaking
        /// </summary>
        /// <param name="name"></param>
        public void SetSpeakerName(string name)
        {
            _speakerName.text = name;
        }

        /// <summary>
        /// Set the text displayed within the dialogue box.
        /// </summary>
        /// <param name="text"></param>
        public void DisplayText(string text)
        {
            if (panelHidden) Show();

            _dialogueText.SetText(text);
        }

        /// <summary>
        /// Make it so the advance button can advance the dialogue
        /// </summary>
        public void EnableContinueButton()
        {
            advanceDialogueButton.gameObject.SetActive(true);
        }

        /// <summary>
        /// Make it so the advance button does not advance the dialogue.
        /// </summary>
        public void DisableContinueButton()
        {
            advanceDialogueButton.gameObject.SetActive(false);
        }

        /// <summary>
        /// Show the dialogue panel.
        /// </summary>
        public void Show()
        {
            panelHidden = false;

            if (panelSlideCoroutine != null)
            {
                StopCoroutine(panelSlideCoroutine);
            }

            panelSlideCoroutine = StartCoroutine(SlidePanelIn());

            if (OnShow != null) OnShow.Invoke();
        }

        /// <summary>
        /// Hide the dialogue and choice panels.
        /// </summary>
        public void Hide()
        {
            panelHidden = true;

            ChoiceDialog.Hide();

            if (panelSlideCoroutine != null)
            {
                StopCoroutine(panelSlideCoroutine);
            }

            panelSlideCoroutine = StartCoroutine(SlidePanelOut());

            if (OnHide != null) OnHide.Invoke();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Is the panel currently hidden or being hidden
        /// </summary>
        public bool IsHidden => panelHidden;

        #endregion

        #region Private Coroutines

        /// <summary>
        /// Displays text using a typewriter effect where each character appears once at a time.
        /// </summary>
        /// <returns></returns>
        private IEnumerator TextTypeWriterEffect()
        {
            yield return null;
        }

        private IEnumerator SlidePanelOut()
        {
            Vector3 initialPosition = rectTransform.position;
            float elapsedTime = 0f;

            while (elapsedTime < slideOutSeconds)
            {
                elapsedTime += Time.deltaTime;
                rectTransform.position =
                    Vector3.Lerp(initialPosition, offScreenPosition, elapsedTime / slideOutSeconds);
                yield return null;
            }
        }

        private IEnumerator SlidePanelIn()
        {
            Vector3 initialPosition = rectTransform.position;
            float elapsedTime = 0f;

            while (elapsedTime < slideInSeconds)
            {
                elapsedTime += Time.deltaTime;
                rectTransform.position =
                    Vector3.Lerp(initialPosition, onScreenPosition, elapsedTime / slideInSeconds);
                yield return null;
            }
        }

        #endregion
    }

}
