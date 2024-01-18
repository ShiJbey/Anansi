using System.Xml;

namespace Calypso.Scheduling
{
    /// <summary>
    /// A class of object responsible for creating new precondition instances.
    /// </summary>
    public interface IPreconditionFactory
    {
        public IPrecondition CreatePrecondition(XmlNode node);
    }
}
