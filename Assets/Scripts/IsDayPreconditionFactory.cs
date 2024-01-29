using System.Xml;
using Calypso;
using Calypso.Scheduling;
using UnityEngine;

public class IsDayPreconditionFactory : MonoBehaviour, IPreconditionFactory
{
	public IPrecondition CreatePrecondition(XmlNode node)
	{
		XmlElement preconditionElement = (XmlElement)node;

		int day = int.Parse( preconditionElement.GetAttribute( "day" ) );

		return new IsDayPrecondition( day );
	}

	// Start is called before the first frame update
	void Awake()
	{
		PreconditionLibrary.factories["IsDay"] = this;
	}

}
