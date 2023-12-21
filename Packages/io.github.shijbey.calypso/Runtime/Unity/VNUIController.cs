using UnityEngine;
using UnityEngine.Events;

namespace Calypso.Unity
{
    public class VNUIController : MonoBehaviour
    {
        [SerializeField]
        private BackgroundController background;

        [SerializeField]
        private CharacterSpriteController characterSprite;

        [SerializeField]
        private DialoguePanelController dialoguePanel;

        [SerializeField]
        private StatusBarController statusBar;

        [SerializeField]
        private GameObject interactionPanel;
    }
}
