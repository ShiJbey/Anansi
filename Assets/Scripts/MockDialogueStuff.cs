using UnityEngine;
using Calypso;

public class MockDialogueStuff : MonoBehaviour
{
    [SerializeField]
    private DialoguePanelController dialoguePanelController;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.D))
        {
            if (dialoguePanelController.IsHidden)
            {
                dialoguePanelController.Show();
            }
            else
            {
                dialoguePanelController.Hide();
            }
        }

        if (Input.GetKeyUp(KeyCode.Alpha0))
        {
            dialoguePanelController.JumpToEndOfText();
        }
    }
}
