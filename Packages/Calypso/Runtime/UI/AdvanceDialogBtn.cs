using UnityEngine;
using UnityEngine.UI;

namespace Calypso.UI
{
    /// <summary>
    /// A button that signals to the dialog manager to advance the dialogue
    /// </summary>
    public class AdvanceDialogBtn : MonoBehaviour
    {
        private DialogueManager _dialogManager;

        void Start()
        {
            _dialogManager = FindObjectOfType<DialogueManager>();

            if (_dialogManager == null)
            {
                Debug.LogError("Dialog Manager was not found.");
            }
        }

        void Update()
        {
            // Hide the button when the player cannot advance the story
            GetComponent<Button>().interactable = _dialogManager.CanAdvanceDialogue();
        }

        public void OnClick()
        {
            // Advance the dialog of the story
            _dialogManager.AdvanceDialogue();
        }
    }
}
