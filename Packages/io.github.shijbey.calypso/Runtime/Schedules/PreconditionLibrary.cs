using System.Collections.Generic;

namespace Calypso.Scheduling
{
    /// <summary>
    /// Shared repository of precondition factory instances used when constructing new IPrecondition
    /// instances.
    /// </summary>
    public static class PreconditionLibrary
    {
        public static Dictionary<string, IPreconditionFactory> factories =
            new Dictionary<string, IPreconditionFactory>();
    }
}
