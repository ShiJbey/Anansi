using System.Collections.Generic;
using UnityEngine;
using Calypso.Unity;
using System;

namespace Calypso
{
	/// <summary>
	/// A measure of how one Actor feels toward another
	/// </summary>
	public class Relationship : MonoBehaviour
	{
		#region Fields
		/// <summary>
		/// The Actor that owns this relationship
		/// </summary>
		[SerializeField]
		private Actor _owner;

		/// <summary>
		/// The Actor this relationship directed toward
		/// </summary>
		[SerializeField]
		private Actor _target;

		/// <summary>
		/// A single measure of relationship affinity
		/// </summary>
		[Range(-50, 50)]
		[SerializeField]
		private int _baseReputation = 0;

		/// <summary>
		/// Previous events that have transpired between these two characters
		/// </summary>
		private List<RelationshipEvent> _eventHistory = new List<RelationshipEvent>();

		// Reference to GameManager singleton
		private GameManager _gameManager;
		private RelationshipManager _relationshipManager;

		#endregion

		#region Properties
		public Actor Owner { get { return _owner; } }
		public Actor Target { get { return _target; } }
		public int Reputation { get { return _baseReputation; } }
		public List<RelationshipEvent> History { get { return _eventHistory; } }
		#endregion

		#region Unity Lifecycle Methods
		private void Awake()
		{
			_gameManager = FindObjectOfType<GameManager>();
			if (_gameManager == null)
			{
				throw new NullReferenceException(
					"Cannot find GameManager.");
			}

			_relationshipManager = FindObjectOfType<RelationshipManager>();
			if (_relationshipManager == null)
			{
				throw new NullReferenceException(
					"Cannot find RelationshipManager.");
			}
		}

		private void Start()
		{
			_relationshipManager.RegisterRelationship(this);
			_gameManager.Database[
				$"{Owner.UniqueID}.relationship.{Target.UniqueID}.reputation"
			] = Reputation;
		}
		#endregion

		public void SetBaseReputation(int value)
		{
			_baseReputation = value;
		}

		public void OnUpdate()
		{
			// for (int i = _eventHistory.Count; i >= 0;)
			// {
			// 	RelationshipEvent entry = _eventHistory[i];

			// 	entry.OnUpdate(world);

			// 	if (entry.IsValid(world))
			// 	{
			// 		--i;
			// 	}
			// 	else
			// 	{
			// 		_eventHistory.RemoveAt(i);
			// 	}
			// }
		}
	}
}

