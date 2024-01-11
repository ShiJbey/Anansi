using UnityEngine;
using Calypso;

public class MockCharacterTransition : MonoBehaviour
{
    private int index = -1;

    [SerializeField]
    private Actor[] characters;

    [SerializeField]
    private CharacterSpriteController characterController;

    [SerializeField]
    private DialoguePanelController dialoguePanel;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Q))
        {
            if (characters.Length == 0) return;

            index++;

            if (index == characters.Length) index = 0;

            Actor character = characters[index];

            characterController.SetSpeaker(character.GetSprite());
            dialoguePanel.SetSpeakerName(character.DisplayName);
        }

        if (Input.GetKeyUp(KeyCode.W))
        {
            characterController.Hide();
        }
    }
}
