using Calypso.Unity;
using UnityEngine;

public class MockTriggerSchedule : MonoBehaviour
{
    private TimeManager timeManager;

    // Start is called before the first frame update
    void Start()
    {
        timeManager = FindObjectOfType<TimeManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.T))
        {
            timeManager.AdvanceTime();
        }
    }
}
