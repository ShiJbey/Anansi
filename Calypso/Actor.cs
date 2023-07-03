using System;
namespace Calypso
{
	/// <summary>
	/// Actors represent characters in the story world. 
	/// </summary>
	public class Actor
	{
		/// <summary>
		/// Text name for displaying to the player
		/// </summary>
		private string _name;

		/// <summary>
		/// A unique identifier for refering to this character in writing
		/// </summary>
		private string _uid;

		public string Name {
			get { return _name; }
			set { this._name = value; }
		}

		public string UID
		{
			get { return _uid; }
		}


		public Actor(string name, string uid)
		{
			_name = name;
			_uid = uid;
		}

		public override string ToString()
		{
			return $"Actor({_name})";
		}
	}
}

