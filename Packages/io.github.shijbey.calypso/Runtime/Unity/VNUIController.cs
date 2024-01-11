using UnityEngine;

namespace Calypso
{
    public class VNUIController : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// A reference to the background controller
        /// </summary>
        [SerializeField]
        private BackgroundController m_background;

        /// <summary>
        /// A reference to the character sprite controller
        /// </summary>
        [SerializeField]
        private CharacterSpriteController m_characterSprite;

        /// <summary>
        /// A reference to the dialogue panel controller
        /// </summary>
        [SerializeField]
        private DialoguePanelController m_dialoguePanel;

        /// <summary>
        /// A reference to the status bar controller
        /// </summary>
        [SerializeField]
        private StatusBarController m_statusBar;

        /// <summary>
        /// A reference to the interaction panel controller
        /// </summary>
        [SerializeField]
        private InteractionPanelController m_interactionPanel;

        #endregion

        #region Properties

        /// <summary>
        /// A reference to the background controller
        /// </summary>
        public BackgroundController Background => m_background;

        /// <summary>
        /// A reference to the character sprite controller
        /// </summary>
        public CharacterSpriteController CharacterSprite => m_characterSprite;

        /// <summary>
        /// A reference to the dialogue panel controller
        /// </summary>
        public DialoguePanelController DialoguePanel => m_dialoguePanel;

        /// <summary>
        /// A reference to the status bar controller
        /// </summary>
        public StatusBarController StatusBar => m_statusBar;

        /// <summary>
        /// A reference to the interaction panel controller
        /// </summary>
        public InteractionPanelController InteractionPanel => InteractionPanel;

        #endregion
    }
}
