using UnityEngine;
using System.Linq;
using Calypso;

public class MockBGTransition : MonoBehaviour
{
    [SerializeField]
    private string m_locationID;

    [SerializeField]
    private BackgroundSpriteController m_backgroundSpriteController;

    [SerializeField]
    private string[] m_tags;

    void Start()
    {
        m_backgroundSpriteController =
            FindObjectOfType<BackgroundSpriteController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.B))
        {
            m_backgroundSpriteController.SetBackground(m_locationID, m_tags);
        }

        if (Input.GetKeyUp(KeyCode.N))
        {
            m_backgroundSpriteController.ShowBackground();
        }

        if (Input.GetKeyUp(KeyCode.M))
        {
            m_backgroundSpriteController.HideBackground();
        }
    }
}
