using System.Collections;
using System.Collections.Generic;
using Calypso.Scheduling;
using Calypso.Unity;
using UnityEngine;

public class MockTriggerSchedule : MonoBehaviour
{

    private CharacterSchedule schedule;
    private TimeManager timeManager;

    // Start is called before the first frame update
    void Start()
    {
        schedule = GetComponent<CharacterSchedule>();
        timeManager = FindObjectOfType<TimeManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            var scheduleEntry = schedule.ActiveSchedule.GetEntry(timeManager.Date);
            scheduleEntry.Execute(timeManager.Date);
        }
    }
}
