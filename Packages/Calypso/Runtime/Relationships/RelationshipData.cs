using System.Collections.Generic;


namespace Calypso.Relationships
{
    /// <summary>
    /// Tracks information about a relationship from one character to another
    /// </summary>
    public class RelationshipData : IRelationship
    {
        #region Fields
        private string _ownerID;
        private string _targetID;
        private RelationshipStat _friendship;
        private RelationshipStat _romance;
        private List<IRelationshipEvent> _eventHistory = new List<IRelationshipEvent>();
        #endregion

        #region Properties
        /// <summary>
        /// The unique ID of the owner of the relationship
        /// </summary>
        public string OwnerID => _ownerID;
        /// <summary>
        /// The unique ID of the target of the relationship
        /// </summary>
        public string TargetID => _targetID;
        /// <summary>
        /// The platonic affinity of the owner for the target
        /// </summary>
        public RelationshipStat Friendship => _friendship;
        /// <summary>
        /// The romantic affinity of the owner for the target
        /// </summary>
        public RelationshipStat Romance => _romance;
        /// <summary>
        /// Previous events that have transpired between these two characters
        /// </summary>
        public IEnumerable<IRelationshipEvent> History => _eventHistory;
        #endregion

        public RelationshipData(string owner, string target)
        {
            _ownerID = owner;
            _targetID = target;
            _friendship = new RelationshipStat(0);
            _romance = new RelationshipStat(0);
            _eventHistory = new List<IRelationshipEvent>();
        }

        #region Methods
        /// <summary>
        /// Add an event to the relationship
        /// </summary>
        /// <param name="e"></param>
        public void AddRelationshipEvent(IRelationshipEvent e)
        {
            _eventHistory.Add(e);
            e.OnAdd(this);
        }

        /// <summary>
        /// Remove an event from the relationship
        /// </summary>
        /// <param name="e"></param>
        public void RemoveRelationshipEvent(IRelationshipEvent e)
        {
            _eventHistory.Remove(e);
            e.OnRemove(this);
        }

        /// <summary>
        /// Updates timers on relationship events, removing any expired events
        /// </summary>
        public void UpdateEvents()
        {
            foreach (var entry in _eventHistory)
            {
                entry.OnUpdate();
                if (entry.IsValid() == false)
                {
                    RemoveRelationshipEvent(entry);
                }
            }
        }
        #endregion
    }
}
