using System;
namespace Calypso
{
	/// <summary>
	/// Tracks relationships between characters
	/// </summary>
	public class RelationshipManager
	{
		private Dictionary<Actor, Dictionary<Actor, Relationship>> _relationships;

		public RelationshipManager()
		{
			_relationships = new Dictionary<Actor, Dictionary<Actor, Relationship>>();
		}

        public void OnUpdate(GameWorld world)
        {
			foreach (KeyValuePair<Actor, Dictionary<Actor, Relationship>> actorRelationships in _relationships)
			{
				foreach (KeyValuePair<Actor, Relationship> entry in actorRelationships.Value)
				{
					entry.Value.OnUpdate(world);
				}
			}
        }

		public bool RelationshipExists(Actor owner, Actor target)
		{
			return _relationships.ContainsKey(owner) && _relationships[owner].ContainsKey(target);
        }

		public Relationship GetRelationship(Actor owner, Actor target)
		{
			if (_relationships.ContainsKey(owner) && _relationships[owner].ContainsKey(target))
			{
				return _relationships[owner][target];
			}
			else
			{
				Relationship relationship = new Relationship(owner, target);

				if (_relationships.ContainsKey(owner) == false)
				{
					_relationships[owner] = new Dictionary<Actor, Relationship>();
				}

				_relationships[owner][target] = relationship;

				return relationship;
			}
		}
    }
}

