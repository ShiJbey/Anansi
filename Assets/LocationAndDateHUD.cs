using Calypso;
using Calypso.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Displays the current time and the player's current location
/// </summary>
public class LocationAndDateHUD : MonoBehaviour
{
    [SerializeField]
    private TMPro.TMP_Text _locationText;

    [SerializeField]
    private TMPro.TMP_Text _dateText;

    [SerializeField]
    private TMPro.TMP_Text _timeText;

    private void OnEnable()
    {
        TimeManager.OnTimeChanged += OnTimeChange;
        GameManager.OnPlayerLocationChanged += OnLocationChange;
    }

    private void OnDisable()
    {
        TimeManager.OnTimeChanged -= OnTimeChange;
        GameManager.OnPlayerLocationChanged -= OnLocationChange;
    }

    private void OnLocationChange(Calypso.Unity.Location location)
    {
        _locationText.text = location.DisplayName;
    }

    private void OnTimeChange(SimDateTime date)
    {
        _dateText.text = date.ToString();
        _timeText.text = date.TimeOfDay.ToString();
    }
}
