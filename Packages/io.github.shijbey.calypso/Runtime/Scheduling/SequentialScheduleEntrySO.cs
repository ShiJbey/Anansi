using System.Collections.Generic;
using UnityEngine;

namespace Calypso.Scheduling
{
    [CreateAssetMenu(fileName = "NewScheduleEntry", menuName = "Calypso/ScheduleEntry")]
    public class SequentialScheduleEntrySO : ScheduleEntryScriptableObject
    {
        [SerializeField]
        protected List<ActionScriptableObject> _actions;

        public override IScheduleEntry CreateScheduleEntry()
        {
            var entry = new SequentialScheduleEntry(name, _startTime, _endTime, _isRepeatable);

            foreach (var a in _actions)
            {
                entry.AddAction(a);
            }

            return entry;
        }
    }
}


