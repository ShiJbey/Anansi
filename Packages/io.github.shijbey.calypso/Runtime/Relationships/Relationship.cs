using System.Collections.Generic;
using UnityEngine;
using Calypso.Unity;
using System;
using UnityEngine.Events;

namespace Calypso.Relationships
{
	/// <summary>
	/// A measure of how one Actor feels toward another
	/// </summary>
	public class Relationship : MonoBehaviour, IRelationship
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
		/// The measure of platonic affinity
		/// </summary>
		[Range(-50, 50)]
		[SerializeField]
		private int _baseFriendship;

		/// <summary>
		/// The measure of romantic affinity
		/// </summary>
		[Range(-50, 50)]
		[SerializeField]
		private int _baseRomance;


		// Reference to GameManager singleton
		private StoryDatabase _storyDatabase;

		private RelationshipManager _relationshipManager;

		private RelationshipData _relationshipData;
		#endregion

		#region Properties
		public Actor Owner => _owner;
		public Actor Target => _target;
		public string OwnerID => _owner.UniqueID;
		public string TargetID => _target.UniqueID;
		public RelationshipStat Friendship => _relationshipData.Friendship;
		public RelationshipStat Romance => _relationshipData.Romance;
		public IEnumerable<IRelationshipEvent> History => _relationshipData.History;
		private string DBStringBase => $"{OwnerID}.relationship.{TargetID}";
		#endregion

		#region Actions and Events
		public UnityAction<int> OnFriendshipChanged;
		public UnityAction<int> OnRomanceChanged;
		#endregion

		#region Unity Lifecycle Methods
		private void Awake()
		{
			_storyDatabase = FindAnyObjectByType<StoryDatabase>();
			_relationshipManager = FindObjectOfType<RelationshipManager>();
			if (_relationshipManager == null)
			{
				throw new NullReferenceException(
					"Cannot find RelationshipManager.");
			}
		}

		private void Start()
		{
			InstantiateRelationship();
			_relationshipManager.RegisterRelationship(this);
		}
		#endregion

		#region Helper Methods
		private void InstantiateRelationship()
		{
			_relationshipData = new RelationshipData(OwnerID, TargetID);

			_relationshipData.Friendship.OnValueChanged += InvokeFriendshipCallbacks;
			_relationshipData.Romance.OnValueChanged += InvokeRomanceCallbacks;

			_relationshipData.Friendship.BaseValue = _baseFriendship;
			_relationshipData.Romance.BaseValue = _baseRomance;
		}

		private void InvokeFriendshipCallbacks(int newValue)
		{
			_storyDatabase.db[$"{DBStringBase}.friendship"] = newValue;
			OnFriendshipChanged?.Invoke(newValue);
		}

		private void InvokeRomanceCallbacks(int newValue)
		{
			_storyDatabase.db[$"{DBStringBase}.romance"] = newValue;
			OnRomanceChanged?.Invoke(newValue);
		}
		#endregion

		#region Methods
		/// <summary>
		/// Add an event to the relationship
		/// </summary>
		/// <param name="e"></param>
		public void AddRelationshipEvent(IRelationshipEvent e)
		{
			_relationshipData.AddRelationshipEvent(e);
		}

		/// <summary>
		/// Remove an event from the relationship
		/// </summary>
		/// <param name="e"></param>
		public void RemoveRelationshipEvent(IRelationshipEvent e)
		{
			_relationshipData.RemoveRelationshipEvent(e);
		}

		/// <summary>
		/// Updates timers on relationship events, removing any expired events
		/// </summary>
		public void UpdateEvents()
		{
			_relationshipData.UpdateEvents();
		}
		#endregion
	}
}
