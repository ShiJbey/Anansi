using System;
namespace Calypso
{
	/// <summary>
	/// Manages all the locations and characters present
	/// </summary>
	public class GameWorld
	{

		private List<Actor> _actors;
		private RelationshipManager _relationshipManager;

		public List<Actor> Actors { get { return _actors; } }
		public RelationshipManager RelationshipManager { get { return _relationshipManager; } }

		public GameWorld()
		{
			_actors = new List<Actor>();
			_relationshipManager = new RelationshipManager();
		}
	}
}

