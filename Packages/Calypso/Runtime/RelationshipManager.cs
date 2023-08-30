using System.Collections.Generic;
using UnityEngine;

namespace Calypso.Unity
{
	/// <summary>
	/// Tracks relationships between characters
	/// </summary>
	public class RelationshipManager : MonoBehaviour
	{
		#region Fields
		// The following prefab is used to create new relationship instances
		[SerializeField]
		private GameObject relationshipPrefab;

		private Dictionary<Actor, Dictionary<Actor, Relationship>> _relationships
			 = new Dictionary<Actor, Dictionary<Actor, Relationship>>();
		#endregion

		public void RegisterRelationship(Relationship relationship)
		{
			if (_relationships.ContainsKey(relationship.Owner) == false)
			{
				_relationships[relationship.Target] = new Dictionary<Actor, Relationship>();
			}

			_relationships[relationship.Owner][relationship.Target] = relationship;
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
				var relationship = Instantiate(relationshipPrefab).GetComponent<Relationship>();

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

