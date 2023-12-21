using UnityEngine;

namespace Calypso.Unity
{
    /// <summary>
    /// Controller behaviour for the status bar at the top of the screen
    /// </summary>
    public class StatusBarController : MonoBehaviour
    {
        /// <summary>
        /// A reference to the text displaying the name of th. displayed location.
        /// </summary>
        [SerializeField]
        private TMPro.TMP_Text _locationText;

        /// <summary>
        /// A reference to the text displaying the current time and date.
        /// </summary>
        [SerializeField]
        private TMPro.TMP_Text _dateTimeText;

        /// <summary>
        /// Modify the date/time text
        /// </summary>
        /// <param name="text"></param>
        public void SetDateTimeText(string text)
        {
            _dateTimeText.SetText(text);
        }

        /// <summary>
        /// Set the location text
        /// </summary>
        /// <param name="text"></param>
        public void SetLocationText(string text)
        {
            _locationText.SetText(text);
        }
    }
}
