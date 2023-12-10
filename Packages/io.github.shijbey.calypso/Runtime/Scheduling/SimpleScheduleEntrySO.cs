using UnityEngine;

namespace Calypso.Scheduling
{
    [CreateAssetMenu(fileName = "NewScheduleEntry", menuName = "Calypso/ScheduleEntry")]
    public class SimpleScheduleEntrySO : ScheduleEntryScriptableObject
    {
        [SerializeField]
        private ActionScriptableObject _action;

        public override IScheduleEntry CreateScheduleEntry()
        {
            var entry = new SimpleScheduleEntry(name, _startTime, _endTime, _isRepeatable);

            entry.AddAction(_action);

            return entry;
        }
    }
}

