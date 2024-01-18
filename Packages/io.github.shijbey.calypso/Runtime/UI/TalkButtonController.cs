using UnityEngine;
using UnityEngine.UI;

namespace Calypso
{
    /// <summary>
    /// Handles interactivity and button clicks for the button that starts conversations
    /// with NPCs
    /// </summary>
    public class TalkButtonController : MonoBehaviour
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
            m_button.onClick.AddListener(HandleClick);
        }

        // Update is called once per frame
        private void Update()
        {
            PollCanTalk();
        }

        private void OnDisable()
        {
            // Clean up our listener
            m_button.onClick.RemoveListener(HandleClick);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Toggle button interactivity based on if the player can talk to anyone.
        /// </summary>
        public void PollCanTalk()
        {
            m_button.interactable = m_gameManager.CanPlayerTalk();
        }

        /// <summary>
        /// Tell the GameManager to start a conversation when the button is clicked.
        /// </summary>
        public void HandleClick()
        {
            m_gameManager.StartConversation();
        }

        #endregion
    }
}
