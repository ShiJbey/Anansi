using System;

namespace Calypso.RePraxis
{
    /// <summary>
    /// Exception raised when trying to set a value in the database with an incorrect cardinality.
    /// </summary>
    public class CardinalityException : Exception
    {
        public CardinalityException() { }

        public CardinalityException(string message) : base(message) { }

        public CardinalityException(string message, Exception inner) : base(message, inner) { }
    }
}
