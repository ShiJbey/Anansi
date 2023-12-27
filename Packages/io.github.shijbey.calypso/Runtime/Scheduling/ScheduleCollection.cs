using System;
using System.Collections.Generic;

namespace Calypso.Scheduling
{
    /// <summary>
    /// A collection of schedules with entries to select from
    /// </summary>
    public class ScheduleCollection
    {
        /// <summary>
        /// All the entries within the collection
        /// </summary>
        protected List<Schedule> _schedules;

        /// <summary>
        /// Get the number of schedules in the collection.
        /// </summary>
        /// <returns></returns>
        public int Count => _schedules.Count;

        public ScheduleCollection()
        {
            _schedules = new List<Schedule>();
        }

        public ScheduleCollection(IEnumerable<Schedule> schedules)
        {
            _schedules = new List<Schedule>();

            foreach (var entry in schedules)
            {
                AddSchedule(entry);
            }
        }


        /// <summary>
        /// Get all entries for a given date/time.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public ScheduleEntry[] GetEntries(SimDateTime dateTime)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Add a new schedule to this collection.
        /// </summary>
        /// <param name="schedule"></param>
        public void AddSchedule(Schedule schedule)
        {
            _schedules.Add(schedule);
        }

        /// <summary>
        /// Remove a schedule from this collection.
        /// </summary>
        /// <param name="schedule"></param>
        public bool RemoveSchedule(Schedule schedule)
        {
            return _schedules.Remove(schedule);
        }


    }
}
