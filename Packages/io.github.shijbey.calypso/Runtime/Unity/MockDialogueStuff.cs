using UnityEngine;
using Calypso.Unity;

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

        if (Input.GetKeyUp(KeyCode.A))
        {
            dialoguePanelController.DisplayText(@"
                Lorem ipsum dolor sit amet, consectetur adipiscing elit,
                sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.");
        }

        if (Input.GetKeyUp(KeyCode.S))
        {
            if (!dialoguePanelController.IsHidden)
            {
                dialoguePanelController.ChoiceDialog.DisplayChoices(new string[]{
                    "Go home", "Go to the store", "Go to the library"
                });
            }
        }
    }
}
