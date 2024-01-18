using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Calypso
{
    /// <summary>
    /// Handles displaying options for locations where the player may move to on the map.
    /// </summary>
    public class LocationSelectionDialogController : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// A reference to the game manager
        /// </summary>
        [SerializeField]
        private GameManager m_gameManager;

        /// <summary>
        /// A reference to the prefab used to create choice buttons.
        /// </summary>
        [SerializeField]
        private Button m_choiceButtonPrefab;

        /// <summary>
        /// A reference to the container that holds the choice buttons.
        /// </summary>
        [SerializeField]
        private RectTransform m_choiceButtonContainer;

        /// <summary>
        /// The on-screen position of the dialogue panel.
        /// </summary>
        private Vector3 m_onScreenPosition;

        /// <summary>
        /// The off-screen position of the dialogue panel.
        /// </summary>
        private Vector3 m_offScreenPosition;

        /// <summary>
        /// Reference to this MonoBehaviour's RectTransform.
        /// </summary>
        private RectTransform m_rectTransform;

        /// <summary>
        /// A reference to the UI element holding the choices.
        /// </summary>
        private List<Button> m_choiceButtons;

        /// <summary>
        /// Is the panel currently visible on the screen
        /// </summary>
        private bool m_isVisible = true;

        #endregion

        #region Unity Messages

        private void Awake()
        {
            m_choiceButtons = new List<Button>();
            m_rectTransform = gameObject.transform as RectTransform;
        }

        private void Start()
        {
            // Configure the on and off-screen positions

            Vector3 startingPos = m_rectTransform.position;
            m_onScreenPosition = new Vector3(startingPos.x, startingPos.y, startingPos.z);

            m_offScreenPosition = new Vector3(
                startingPos.x,
                -(m_rectTransform.rect.height + 200),
                startingPos.z
            );

            Hide();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Displays the dialog box
        /// </summary>
        public void Show()
        {
            ClearChoices();

            IList<Location> eligibleLocations = m_gameManager.GetLocationsPlayerCanTravelTo();

            foreach (Location location in eligibleLocations)
            {
                // creates the button from a prefab
                Button choiceButton = Instantiate(m_choiceButtonPrefab);
                choiceButton.transform.SetParent(m_choiceButtonContainer.transform, false);
                m_choiceButtons.Add(choiceButton);

                // sets text on the button
                var buttonText = choiceButton.GetComponentInChildren<TMP_Text>();
                buttonText.text = location.DisplayName;


                //  Adds the onClick callback
                choiceButton.onClick.AddListener(() =>
                {
                    Location loc = location;
                    ClearChoices();
                    Hide();
                    SetPlayerLocation(loc);
                });
            }

            m_rectTransform.position = m_onScreenPosition;
            m_isVisible = true;
        }

        /// <summary>
        /// Hide the dialog box
        /// </summary>
        public void Hide()
        {
            m_rectTransform.position = m_offScreenPosition;
            m_isVisible = false;
        }

        /// <summary>
        /// Toggle the visibility of the dialog box
        /// </summary>
        public void Toggle()
        {
            if (m_isVisible)
            {
                Hide();
            }
            else
            {
                Show();
            }
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

        #region Private Message

        /// <summary>
        /// Change the location of the player and the current background
        /// </summary>
        /// <param name="location"></param>
        private void SetPlayerLocation(Location location)
        {
            m_gameManager.SetPlayerLocation(location);
            m_gameManager.SetStoryLocation(location.UniqueID);
        }

        #endregion
    }
}
