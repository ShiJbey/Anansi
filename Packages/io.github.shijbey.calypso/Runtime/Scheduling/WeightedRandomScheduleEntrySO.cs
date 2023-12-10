using System;
using System.Collections.Generic;
using UnityEngine;

namespace Calypso.Scheduling
{
    /// <summary>
    /// A schedule entry
    /// </summary>
    [CreateAssetMenu(
        fileName = "NewScheduleEntry",
        menuName = "Calypso/WeightedRandomScheduleEntry")]
    public class WeightedRandomScheduleEntrySO : ScheduleEntryScriptableObject
    {
        [SerializeField]
        private List<WeightedRandomAction> _actions;

        public override IScheduleEntry CreateScheduleEntry()
        {
            var entry = new WeightedRandomScheduleEntry(name, _startTime, _endTime, _isRepeatable);

            foreach (var a in _actions)
            {
                entry.AddAction(a.action, a.weight);
            }

            return entry;
        }
    }

    [Serializable]
    public struct WeightedRandomAction
    {
        public int weight;
        public ActionScriptableObject action;
    }
}

