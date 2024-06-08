using System.Xml;

namespace Anansi.Scheduling
{
    /// <summary>
    /// A class of object responsible for creating new precondition instances.
    /// </summary>
    public interface IPreconditionFactory
    {
        public IPrecondition CreatePrecondition(XmlNode node);
    }
}
