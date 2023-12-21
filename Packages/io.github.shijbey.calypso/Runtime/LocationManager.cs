using System.Collections.Generic;
using UnityEngine;

namespace Calypso.Unity
{
    /// <summary>
    /// Maintains a look-up table of locations that exist in the game
    /// </summary>
    public class LocationManager : MonoBehaviour
    {
        /// <summary>
        /// The collection of locations that exist in the game (Set in Unity Inspector).
        /// </summary>
        [SerializeField]
        private List<Location> locations = new List<Location>();

        /// <summary>
        /// IDs of locations mapped to their instances.
        /// </summary>
        private Dictionary<string, Location> locationLookupTable =
            new Dictionary<string, Location>();

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
            foreach (Location location in locations)
            {
                locationLookupTable.Add(location.UniqueID, location);
            }
        }

        /// <summary>
        /// Get a location using their ID.
        /// </summary>
        /// <param name="locationID"></param>
        /// <returns></returns>
        public Location GetLocation(string locationID)
        {
            if (locationLookupTable.TryGetValue(locationID, out var location))
            {
                return location;
            }

            throw new KeyNotFoundException($"Could not find location with ID: {locationID}");
        }

    }
}
