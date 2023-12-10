using System.Collections.Generic;

namespace Calypso.Scheduling
{
    /// <summary>
    /// Manages a collection of schedule entries to determine what a
    /// character should do given the current time.
    /// </summary>
    public class Schedule : ISchedule
    {
        /// <summary>
        /// All the entries that belong to a schedule
        /// </summary>
        private Dictionary<Days, List<IScheduleEntry>> _entries;

        /// <summary>
        /// The schedule entry that is currently active given the current schedule.
        /// </summary>
        private IScheduleEntry _activeEntry;

        public Schedule()
        {
            _entries = new Dictionary<Days, List<IScheduleEntry>>();
            _activeEntry = null;
        }

        public void AddEntry(Days day, IScheduleEntry entry)
        {
            if (!_entries.ContainsKey(day))
            {
                _entries[day] = new List<IScheduleEntry>();
            }

            _entries[day].Add(entry);
        }

        public IScheduleEntry GetEntry(SimDateTime currentTime)
        {
            int ordinalDate = currentTime.TotalNumDays;

            List<IScheduleEntry> entriesForDay;

            _entries.TryGetValue(currentTime.Day, out entriesForDay);

            if (entriesForDay is null)
            {
                _activeEntry = null;
                return _activeEntry;
            }

            foreach (var entry in entriesForDay)
            {
                if (
                    entry.StartTime <= currentTime.Hour
                    && entry.EndTime >= currentTime.Hour
                    && (
                        entry.LastCompleted < ordinalDate
                        || entry.IsRepeatable
                    )
                )
                {
                    _activeEntry = entry;
                    return entry;
                }
            }

            _activeEntry = null;
            return _activeEntry;
        }

        public bool RemoveEntry(Days day, IScheduleEntry entry)
        {
            List<IScheduleEntry> entriesForDay;

            _entries.TryGetValue(day, out entriesForDay);

            if (entriesForDay == null)
            {
                return false;
            }
            else
            {
                return entriesForDay.Remove(entry);
            }
        }
    }
}

