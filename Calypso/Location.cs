using System;
namespace Calypso
{
	/// <summary>
	/// A place where characters and items may be
	/// </summary>
	public class Location
	{
        private string _name;

        public string Name
        {
            get { return _name; }
            set { this._name = value; }
        }

        public Location(string name)
		{
			_name = name;
		}

        public override string ToString()
        {
            return $"Location({_name})";
        }
    }
}

