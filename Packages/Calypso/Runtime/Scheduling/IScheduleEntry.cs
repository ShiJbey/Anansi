namespace Calypso.Scheduling
{
    /// <summary>
    /// An interface for all entries that can be added to a schedule.
    /// </summary>
    public interface IScheduleEntry
    {
        #region Methods
        /// <summary>
        /// Add another action to this entry
        /// </summary>
        public void AddAction(IAction action);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lastAction"></param>
        public void Execute(SimDateTime time);
        #endregion

        #region Properties
        /// <summary>
        /// Get the ID of the schedule entry.
        /// </summary>
        public string ID { get; }

        /// <summary>
        ///  Get the hour that the entry starts (in 24-hour time).
        /// </summary>
        public int StartTime { get; }

        /// <summary>
        /// Get the hour that the entry ends (in 24-hour time).
        /// </summary>
        public int EndTime { get; }

        /// <summary>
        /// Get/Set the ordinal date when the entry was last completed
        /// </summary>
        public int LastCompleted { get; set; }

        /// <summary>
        /// Can this entry be repeated multiple times per day.
        /// </summary>
        public bool IsRepeatable { get; }
        #endregion
    }
}