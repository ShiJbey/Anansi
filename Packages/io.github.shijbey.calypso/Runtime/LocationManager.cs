using System.Collections.Generic;
using UnityEngine;

namespace Calypso
{
    /// <summary>
    /// Maintains a look-up table of locations that exist in the game
    /// </summary>
    public class LocationManager : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// IDs of locations mapped to their instances.
        /// </summary>
        private Dictionary<string, Location> m_locationLookupTable;

        #endregion

        #region Properties

        /// <summary>
        /// All the locations known by the manager
        /// </summary>
        public IEnumerable<Location> Locations => m_locationLookupTable.Values;

        #endregion

        #region Unity Messages

        private void Awake()
        {
            m_locationLookupTable = new Dictionary<string, Location>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Fills the lookup table using entries from the locations collection.
        /// </summary>
        public void InitializeLookUpTable()
        {
            var locations = FindObjectsOfType<Location>();
            foreach (Location location in locations)
            {
                AddLocation(location);
            }
        }

        /// <summary>
        /// Get a location using their ID.
        /// </summary>
        /// <param name="locationID"></param>
        /// <returns></returns>
        public Location GetLocation(string locationID)
        {
            if (m_locationLookupTable.TryGetValue(locationID, out var location))
            {
                return location;
            }

            throw new KeyNotFoundException($"Could not find location with ID: {locationID}");
        }

        /// <summary>
        /// Add a location to the manager
        /// </summary>
        /// <param name="location"></param>
        public void AddLocation(Location location)
        {
            m_locationLookupTable[location.UniqueID] = location;
        }

        #endregion
    }
}
