using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

namespace Calypso.Unity
{
    public class ChoiceDialogController : MonoBehaviour
    {
        #region Protected Fields

        /// <summary>
        /// The on-screen position of the dialogue panel.
        /// </summary>
        private Vector3 onScreenPosition;

        /// <summary>
        /// The off-screen position of the dialogue panel.
        /// </summary>
        private Vector3 offScreenPosition;

        /// <summary>
        /// A reference to the container that holds the choice buttons.
        /// </summary>
        [SerializeField]
        private RectTransform _choiceButtonContainer;

        /// <summary>
        /// A reference to the prefab used to create choice buttons.
        /// </summary>
        [SerializeField]
        private Button _choiceButtonPrefab;

        /// <summary>
        /// A reference to the UI element holding the choices.
        /// </summary>
        private List<Button> _choiceButtons;

        /// <summary>
        /// Reference to this MonoBehaviour's RectTransform.
        /// </summary>
        [SerializeField]
        private RectTransform rectTransform;

        #endregion

        #region Public Properties

        public bool ChoicesDisplayed => _choiceButtons.Count > 0;

        #endregion

        #region Actions

        /// <summary>
        /// Event invoked when a choice is selected. This event accepts the index of the choice
        /// as the argument.
        /// </summary>
        public UnityEvent<int> OnChoiceSelected;

        #endregion

        private void Awake()
        {
            _choiceButtons = new List<Button>();
        }

        private void Start()
        {
            // Configure the on and off-screen positions

            Vector3 startingPos = rectTransform.position;
            onScreenPosition = new Vector3(startingPos.x, startingPos.y, startingPos.z);

            offScreenPosition = new Vector3(
                startingPos.x,
                -(rectTransform.rect.height + 200),
                startingPos.z
            );

            Hide();
        }

        /// <summary>
        /// Displays the choice dialog box
        /// </summary>
        public void Show()
        {
            rectTransform.position = onScreenPosition;
        }

        /// <summary>
        /// Hide the choice dialog box
        /// </summary>
        public void Hide()
        {
            rectTransform.position = offScreenPosition;
        }

        /// <summary>
        /// Display a set of choices to the user
        /// </summary>
        /// <param name="choices"></param>
        public void DisplayChoices(string[] choices)
        {
            ClearChoices();

            for (int i = 0; i < choices.Length; i++) // iterates through all choices
            {
                // creates the button from a prefab
                Button choiceButton = Instantiate(_choiceButtonPrefab);
                choiceButton.transform.SetParent(_choiceButtonContainer.transform, false);
                _choiceButtons.Add(choiceButton);

                // sets text on the button
                var buttonText = choiceButton.GetComponentInChildren<TMP_Text>();
                buttonText.text = choices[i];

                //  Adds the onClick callback
                choiceButton.onClick.AddListener(() =>
                {
                    ClearChoices();
                    Hide();
                    if (OnChoiceSelected != null) OnChoiceSelected.Invoke(i);
                });
            }

            Show();
        }

        /// <summary>
        /// Destroy all current choice buttons
        /// </summary>
        public void ClearChoices()
        {
            foreach (Button button in _choiceButtons)
            {
                Destroy(button.gameObject);
            }

            _choiceButtons.Clear();
        }
    }
}
