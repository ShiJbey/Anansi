using UnityEngine;
using Calypso;

public class MockCharacterTransition : MonoBehaviour
{
    [SerializeField]
    private string m_speakerID;

    [SerializeField]
    private SpeakerSpriteController m_spriteController;

    [SerializeField]
    private string[] m_speakerTags;



    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Q))
        {
            m_spriteController.SetSpeaker(m_speakerID, m_speakerTags);
        }

        if (Input.GetKeyUp(KeyCode.W))
        {
            m_spriteController.HideSpeaker();
        }

        if (Input.GetKeyUp(KeyCode.E))
        {
            m_spriteController.ShowSpeaker();
        }
    }
}
