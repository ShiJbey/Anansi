namespace Calypso.Relationships
{
    public interface IRelationshipEvent
    {
        /// <summary>
        /// Get the name of the event
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Get a text description of the event for the GUI
        /// </summary>
        public string description { get; }

        /// <summary>
		/// Generate a text description of this event.
		/// </summary>
		/// <returns></returns>
		public string GetDescription();

        /// <summary>
        /// Called when an event is added to a relationship
        /// </summary>
        /// <param name="relationship"></param>
        public void OnAdd(IRelationship relationship);

        /// <summary>
        /// Called when the event is removed from a relationship
        /// </summary>
        /// <param name="relationship"></param>
        public void OnRemove(IRelationship relationship);

        /// <summary>
        /// Check if this event is still active/valid.
        ///
        /// Some events may have timeouts that cause them to expire.
        /// </summary>
        /// <returns></returns>
        public bool IsValid();

        /// <summary>
        /// Updates the event
        /// </summary>
        public void OnUpdate();
    }
}
