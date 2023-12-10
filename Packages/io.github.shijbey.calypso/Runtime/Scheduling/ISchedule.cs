namespace Calypso.Scheduling
{
    /// <summary>
    /// An interface for all objects that can be used as schedules.
    /// <para>
    /// Schedules maintain a collection of IScheduleEntry instances
    /// that are selected from based on the current time in the sim.
    /// </para>
    /// </summary>
    public interface ISchedule
    {
        /// <summary>
        /// Add a new entry to the schedule.
        /// </summary>
        /// <param name="entry"></param>
        public void AddEntry(Days day, IScheduleEntry entry);

        /// <summary>
        /// Remove an entry from the schedule.
        /// </summary>
        /// <param name="entry"></param>
        /// <returns><c>true</c> if the entry was removed, <c>false</c> otherwise.</returns>
        public bool RemoveEntry(Days day, IScheduleEntry entry);

        /// <summary>
        /// Get the current entry for the given time.
        /// </summary>
        /// <param name="currentTime"></param>
        /// <returns>A schedule entry or null if no entries</returns>
        public IScheduleEntry GetEntry(SimDateTime currentTime);
    }
}

