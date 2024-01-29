using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

namespace Calypso
{
    public class ChoiceDialogController : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// The on-screen position of the dialogue panel.
        /// </summary>
        private Vector3 m_onScreenPosition;

        /// <summary>
        /// The off-screen position of the dialogue panel.
        /// </summary>
        private Vector3 m_offScreenPosition;

        /// <summary>
        /// A reference to the container that holds the choice buttons.
        /// </summary>
        [SerializeField]
        private RectTransform m_choiceButtonContainer;

        /// <summary>
        /// A reference to the prefab used to create choice buttons.
        /// </summary>
        [SerializeField]
        private Button m_choiceButtonPrefab;

        /// <summary>
        /// A reference to the UI element holding the choices.
        /// </summary>
        private List<Button> m_choiceButtons;

        /// <summary>
        /// Reference to this MonoBehaviour's RectTransform.
        /// </summary>
        private RectTransform m_rectTransform;

        #endregion

        #region Properties

        /// <summary>
        /// Returns True if there are choices present
        /// </summary>
        public bool AreChoicesDisplayed => m_choiceButtons.Count > 0;

        #endregion

        #region Actions and Events

        /// <summary>
        /// Event invoked when a choice is selected. This event accepts the index of the choice
        /// as the argument.
        /// </summary>
        public UnityEvent<int> OnChoiceSelected;

        #endregion

        #region Unity Messages

        private void Awake()
        {
            m_choiceButtons = new List<Button>();
            m_rectTransform = gameObject.GetComponent<RectTransform>();

        }

        private void Start()
        {
            // Configure the on and off-screen positions
            // Vector3 startingPos = m_rectTransform.anchoredPosition;

            // m_onScreenPosition = new Vector3(0, 0, 0);

            // m_offScreenPosition = new Vector3(
            //     0,
            //     -1000,
            //     0
            // );

            // m_onScreenPosition = new Vector3(startingPos.x, startingPos.y, startingPos.z);

            // m_offScreenPosition = new Vector3(
            //     startingPos.x,
            //     -10000,
            //     startingPos.z
            // );

            // Hide();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Displays the choice dialog box
        /// </summary>
        public void Show()
        {
            // m_rectTransform.anchoredPosition = m_onScreenPosition;
        }

        /// <summary>
        /// Hide the choice dialog box
        /// </summary>
        public void Hide()
        {
            // m_rectTransform.anchoredPosition = m_offScreenPosition;
        }

        /// <summary>
        /// Display a set of choices to the user
        /// </summary>
        /// <param name="choices"></param>
        public void SetChoices(string[] choices)
        {
            ClearChoices();

            for (int i = 0; i < choices.Length; i++) // iterates through all choices
            {
                // creates the button from a prefab
                Button choiceButton = Instantiate(m_choiceButtonPrefab);
                choiceButton.transform.SetParent(m_choiceButtonContainer.transform, false);
                m_choiceButtons.Add(choiceButton);

                // sets text on the button
                var buttonText = choiceButton.GetComponentInChildren<TMP_Text>();
                buttonText.text = choices[i];

                int choiceIndex = i;

                //  Adds the onClick callback
                choiceButton.onClick.AddListener(() =>
                {
                    int idx = choiceIndex;
                    // Hide();
                    OnChoiceSelected?.Invoke(idx);
                    ClearChoices();
                });
            }

            // Show();
        }

        /// <summary>
        /// Destroy all current choice buttons
        /// </summary>
        public void ClearChoices()
        {
            foreach (Button button in m_choiceButtons)
            {
                Destroy(button.gameObject);
            }

            m_choiceButtons.Clear();
        }

        #endregion
    }
}
