using System.Collections.Generic;

namespace Calypso
{
	/// <summary>
	/// A measure of how one Actor feels toward another
	/// </summary>
	public class Relationship
	{
		/// <summary>
		/// The Actor that owns this relationship
		/// </summary>
		private Actor _owner;

		/// <summary>
		/// The Actor this relationship directed toward
		/// </summary>
		private Actor _target;

		/// <summary>
		/// A single measure of relationship affinity
		/// </summary>
		private int _baseReputation;

		/// <summary>
		/// Previous events that have transpired between these two characters
		/// </summary>
		private List<RelationshipEvent> _eventHistory;

		public Actor Owner { get { return _owner; } }
		public Actor Target { get { return _target; } }
		public int Reputation { get { return _baseReputation; } }
		public List<RelationshipEvent> History { get { return _eventHistory; } }

		public Relationship(Actor owner, Actor target)
		{
			_owner = owner;
			_target = target;
			_baseReputation = 0;
			_eventHistory = new List<RelationshipEvent>();
		}

		public void SetBaseReputation(int value)
		{
			_baseReputation = value;
		}

        public override string ToString()
        {
			return $"Relationship(from:{this.Owner.Name}, to: {this.Target.Name}, reputation: {this.Reputation})";
		}

		public void OnUpdate(GameWorld world)
		{
			for (int i = _eventHistory.Count; i >= 0;)
			{
				RelationshipEvent entry = _eventHistory[i];

				entry.OnUpdate(world);

				if (entry.IsValid(world))
				{
					--i;
				}
				else
				{
					_eventHistory.RemoveAt(i);
				}
			}
		}
    }
}

