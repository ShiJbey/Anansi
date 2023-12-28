using Calypso.RePraxis;
using UnityEngine;


namespace Calypso
{
    /// <summary>
    /// Wraps a RePraxis database to make it available to Unity Scripts
    /// </summary>
    public class StoryDatabase : MonoBehaviour
    {
        #region Protected Fields

        /// <summary>
        /// Reference to the fully instantiated database
        /// </summary>
        private RePraxisDatabase db = new RePraxisDatabase();

        #endregion

        public RePraxisDatabase DB => db;
    }
}
