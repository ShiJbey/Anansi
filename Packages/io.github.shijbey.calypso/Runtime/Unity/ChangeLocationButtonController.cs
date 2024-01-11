using UnityEngine;
using UnityEngine.UI;

namespace Calypso
{
    /// <summary>
    /// Controls interactivity for the button that opens the location selection menu
    /// </summary>
    public class ChangeLocationButtonController : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// A reference to the Button script on this GameObject
        /// </summary>
        private Button m_button;

        /// <summary>
        /// A reference to the GameManager
        /// </summary>
        [SerializeField]
        private GameManager m_gameManager;

        #endregion

        #region Unity Messages

        // Start is called before the first frame update
        private void Start()
        {
            m_button = GetComponent<Button>();
        }

        // Update is called once per frame
        private void Update()
        {
            PollCanChangeLocation();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Toggle button interactivity based on if the player can talk to anyone.
        /// </summary>
        public void PollCanChangeLocation()
        {
            m_button.interactable = m_gameManager.CanPlayerChangeLocation();
        }

        #endregion
    }
}
