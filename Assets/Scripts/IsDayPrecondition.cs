using Calypso;
using Calypso.Unity;
using UnityEngine;

public class IsDayPrecondition : IPrecondition
{
    public int Day { get; }

    public IsDayPrecondition(int day)
    {
        Day = day;
    }

    public bool CheckPrecondition(GameObject gameObject)
    {
        var timeManager = GameObject.FindObjectOfType<TimeManager>();

        var currentDate = timeManager.Date;

        return currentDate.Day == Day;
    }
}
