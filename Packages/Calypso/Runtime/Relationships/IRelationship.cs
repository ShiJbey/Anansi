using System.Collections.Generic;


namespace Calypso.Relationships
{
    /// <summary>
    /// Interface implemented by objects that can be treated as social relationships
    /// </summary>
    public interface IRelationship
    {
        /// <summary>
        /// The unique ID of the owner of the relationship
        /// </summary>
        public string OwnerID { get; }

        /// <summary>
        /// The unique ID of the target of the relationship
        /// </summary>
        public string TargetID { get; }

        /// <summary>
        /// The platonic affinity of the owner for the target
        /// </summary>
        public RelationshipStat Friendship { get; }

        /// <summary>
        /// The romantic affinity of the owner for the target
        /// </summary>
        public RelationshipStat Romance { get; }

        /// <summary>
        /// Previous events that have transpired between these two characters
        /// </summary>
        public IEnumerable<IRelationshipEvent> History { get; }

        /// <summary>
        /// Add an event to the relationship
        /// </summary>
        /// <param name="e"></param>
        public void AddRelationshipEvent(IRelationshipEvent e);

        /// <summary>
        /// Remove an event from the relationship
        /// </summary>
        /// <param name="e"></param>
        public void RemoveRelationshipEvent(IRelationshipEvent e);

        /// <summary>
        /// Updates timers on relationship events, removing any expired events
        /// </summary>
        public void UpdateEvents();
    }
}
