using System;
using System.Collections.Generic;
using UnityEngine;

namespace Calypso.Scheduling
{
    /// <summary>
    /// This MonoBehaviour handles where a character should be
    /// at certain times during the day. It provides users with
    /// an Inspector-friendly interface to provide entries to the
    /// character's schedule.
    /// </summary>
    public class CharacterSchedule : MonoBehaviour
    {
        #region Fields
        /// <summary>
        /// The ID of the active schedule (Displayed in Unity's Inspector).
        /// </summary>
        [SerializeField]
        private string _activeScheduleID;
        /// <summary>
        /// Reference to the current schedule used by the character
        /// </summary>
        private ISchedule _activeSchedule;
        /// <summary>
        /// All schedules associated with a character
        /// </summary>
        [SerializeField]
        private List<ScheduleListEntry> _schedules;

        private Dictionary<string, ISchedule> _scheduleInstances;
        #endregion

        public ISchedule ActiveSchedule => _activeSchedule;

        #region Methods
        private void Awake()
        {
            _scheduleInstances = new Dictionary<string, ISchedule>();

            foreach (var entry in _schedules)
            {
                _scheduleInstances[entry.scheduleID] = entry.schedule.CreateSchedule();
            }

            SetActiveSchedule(_activeScheduleID);
        }

        /// <summary>
        /// Change the schedule used for action selection.
        /// </summary>
        /// <param name="scheduleID"></param>
        public void SetActiveSchedule(string scheduleID)
        {
            if (_scheduleInstances.ContainsKey(scheduleID))
            {
                _activeSchedule = _scheduleInstances[scheduleID];
                return;
            }

            throw new Exception($"Cannot find schedule with ID: {scheduleID}.");
        }
        #endregion

        /// <summary>
        /// Pairs a schedule with an string ID for use in Unity's Inspector
        /// </summary>
        [Serializable]
        public struct ScheduleListEntry
        {
            public string scheduleID;
            public ScheduleScriptableObject schedule;
        }
    }
}

