using UnityEngine;
using Calypso.Unity;

public class MockBGTransition : MonoBehaviour
{
    private int locationIndex = -1;

    [SerializeField]
    private Location[] locations;

    [SerializeField]
    private BackgroundController backgroundController;

    [SerializeField]
    private StatusBarController statusBar;

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyUp(KeyCode.B))
        {
            if (locations.Length == 0) return;

            locationIndex++;

            if (locationIndex == locations.Length) locationIndex = 0;

            Location location = locations[locationIndex];

            backgroundController.ChangeBackground(location.GetBackground());
            statusBar.SetLocationText(location.DisplayName);
        }
    }
}
