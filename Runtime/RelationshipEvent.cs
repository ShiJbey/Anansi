using System;
namespace Calypso
{
	/// <summary>
	/// Something that happened between two characters to cause a change in
	/// their repuation
	/// </summary>
	public abstract class RelationshipEvent
	{
		/// <summary>
		/// A string name for this event type.
		/// </summary>
		protected string _eventType;

		/// <summary>
		/// The amount that the reputation changes by after this event
		/// </summary>
		protected int _reputationChange;


		public string EventType { get { return _eventType; } }
		public int RepuationChange { get { return _reputationChange; } }


		public RelationshipEvent(string eventType, int reputationChange)
		{
			_eventType = eventType;
			_reputationChange = reputationChange;
		}

        public override string ToString()
        {
			return $"{this.GetDescription()}: {this.RepuationChange.ToString()}";

		}

        /// <summary>
        /// Generate a text description of this event.
        /// </summary>
        /// <returns></returns>
        public abstract string GetDescription();


		/// <summary>
		/// Check if this event is still active/valid.
		///
		/// Some events may have timeouts that cause them to expire.
		/// </summary>
		/// <returns></returns>
		public abstract bool IsValid(GameWorld world);

		/// <summary>
		/// Updates the event
		/// </summary>
		public abstract void OnUpdate(GameWorld world);
	}
}

