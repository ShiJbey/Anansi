namespace Calypso.Scheduling
{
    /// <summary>
    /// An interface for all Actions that can be executed as part of an <c>IScheduleEntry<c/>.
    /// </summary>
    public interface IAction
    {
        public string ID { get; }

        /// <summary>
        /// Execute the action.
        /// </summary>
        public void Execute();
    }
}
