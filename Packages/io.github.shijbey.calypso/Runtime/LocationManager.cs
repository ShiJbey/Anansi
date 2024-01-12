using System.Collections.Generic;
using UnityEngine;

namespace Calypso
{
    /// <summary>
    /// Maintains a look-up table of locations that exist in the game
    /// </summary>
    [DefaultExecutionOrder(-1)]
    public class LocationManager : MonoBehaviour
    {
        /// <summary>
        /// The collection of locations that exist in the game (Set in Unity Inspector).
        /// </summary>
        [SerializeField]
        private List<Location> m_locations = new List<Location>();

        /// <summary>
        /// IDs of locations mapped to their instances.
        /// </summary>
        private Dictionary<string, Location> m_locationLookupTable =
            new Dictionary<string, Location>();


        public IEnumerable<Location> Locations => m_locations;

        // Start is called before the first frame update
        void Start()
        {
            InitializeLookUpTable();
        }

        /// <summary>
        /// Fills the lookup table using entries from the locations collection.
        /// </summary>
        private void InitializeLookUpTable()
        {
            foreach (Location location in m_locations)
            {
                m_locationLookupTable.Add(location.UniqueID, location);
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

    }
}
