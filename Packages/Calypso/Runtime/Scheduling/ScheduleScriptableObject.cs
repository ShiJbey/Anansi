using System.Collections.Generic;
using UnityEngine;

namespace Calypso.Scheduling
{
    /// <summary>
    /// A ScriptableObject interface for implementing schedules for characters
    /// </summary>
    [CreateAssetMenu(fileName = "NewSchedule", menuName = "Calypso/Scheduling/Schedule")]
    public class ScheduleScriptableObject : ScriptableObject
    {
        [SerializeField]
        private List<ScheduleEntryScriptableObject> mondaySchedule;
        [SerializeField]
        private List<ScheduleEntryScriptableObject> tuesdaySchedule;
        [SerializeField]
        private List<ScheduleEntryScriptableObject> wednesdaySchedule;
        [SerializeField]
        private List<ScheduleEntryScriptableObject> thursdaySchedule;
        [SerializeField]
        private List<ScheduleEntryScriptableObject> fridaySchedule;
        [SerializeField]
        private List<ScheduleEntryScriptableObject> saturdaySchedule;
        [SerializeField]
        private List<ScheduleEntryScriptableObject> sundaySchedule;

        /// <summary>
        /// Creates a new schedule instance from the ScriptableObject's data fields
        /// </summary>
        /// <returns></returns>
        public ISchedule CreateSchedule()
        {
            var schedule = new Schedule();

            foreach (var entry in mondaySchedule)
            {
                schedule.AddEntry(Days.Monday, entry.CreateScheduleEntry());
            }

            foreach (var entry in tuesdaySchedule)
            {
                schedule.AddEntry(Days.Tuesday, entry.CreateScheduleEntry());
            }

            foreach (var entry in wednesdaySchedule)
            {
                schedule.AddEntry(Days.Wednesday, entry.CreateScheduleEntry());
            }

            foreach (var entry in thursdaySchedule)
            {
                schedule.AddEntry(Days.Thursday, entry.CreateScheduleEntry());
            }

            foreach (var entry in fridaySchedule)
            {
                schedule.AddEntry(Days.Friday, entry.CreateScheduleEntry());
            }

            foreach (var entry in saturdaySchedule)
            {
                schedule.AddEntry(Days.Saturday, entry.CreateScheduleEntry());
            }

            foreach (var entry in sundaySchedule)
            {
                schedule.AddEntry(Days.Sunday, entry.CreateScheduleEntry());
            }

            return schedule;
        }
    }
}
