using UnityEngine;
using System.Linq;
using Calypso;

public class MockBGTransition : MonoBehaviour
{
    private int locationIndex = -1;

    [SerializeField]
    private LocationManager locationManager;

    private Location[] locations;

    [SerializeField]
    private BackgroundController backgroundController;

    [SerializeField]
    private StatusBarController statusBar;

    void Start()
    {
        locations = locationManager.Locations.ToArray();
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyUp(KeyCode.B))
        {
            if (locations.Length == 0) return;

            locationIndex++;

            if (locationIndex == locations.Length) locationIndex = 0;

            Location location = locations[locationIndex];

            backgroundController.SetBackground(location.GetBackground());
            statusBar.SetLocationText(location.DisplayName);
        }
    }
}
