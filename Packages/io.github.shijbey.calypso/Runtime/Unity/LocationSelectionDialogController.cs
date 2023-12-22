using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Calypso.Unity
{
    /// <summary>
    /// Handles displaying options for locations where the player may move to on the map.
    /// </summary>
    public class LocationSelectionDialogController : MonoBehaviour
    {
        /// <summary>
        /// A reference to the game's location manager.
        /// </summary>
        [SerializeField]
        private LocationManager locationManager;

        /// <summary>
        /// A reference to the prefab used to create choice buttons.
        /// </summary>
        [SerializeField]
        private Button choiceButtonPrefab;

        /// <summary>
        /// A reference to the container that holds the choice buttons.
        /// </summary>
        [SerializeField]
        private RectTransform choiceButtonContainer;

        /// <summary>
        /// The on-screen position of the dialogue panel.
        /// </summary>
        private Vector3 onScreenPosition;

        /// <summary>
        /// The off-screen position of the dialogue panel.
        /// </summary>
        private Vector3 offScreenPosition;

        /// <summary>
        /// Reference to this MonoBehaviour's RectTransform.
        /// </summary>
        private RectTransform rectTransform;

        /// <summary>
        /// A reference to the UI element holding the choices.
        /// </summary>
        private List<Button> _choiceButtons;

        private bool isVisible = true;

        private void Awake()
        {
            _choiceButtons = new List<Button>();
        }

        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();

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
            ClearChoices();

            foreach (Location location in locationManager.Locations)
            {
                // creates the button from a prefab
                Button choiceButton = Instantiate(choiceButtonPrefab);
                choiceButton.transform.SetParent(choiceButtonContainer.transform, false);
                _choiceButtons.Add(choiceButton);

                // sets text on the button
                var buttonText = choiceButton.GetComponentInChildren<TMP_Text>();
                buttonText.text = location.DisplayName;

                //  Adds the onClick callback
                choiceButton.onClick.AddListener(() =>
                {
                    ClearChoices();
                    Hide();
                });
            }

            rectTransform.position = onScreenPosition;
            isVisible = true;
        }

        /// <summary>
        /// Hide the choice dialog box
        /// </summary>
        public void Hide()
        {
            rectTransform.position = offScreenPosition;
            isVisible = false;
        }

        public void Toggle()
        {
            if (isVisible)
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
            foreach (Button button in _choiceButtons)
            {
                Destroy(button.gameObject);
            }

            _choiceButtons.Clear();
        }
    }
}
