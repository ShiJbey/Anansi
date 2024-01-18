using UnityEngine;

namespace Calypso
{
    /// <summary>
    /// Controller behaviour for the status bar at the top of the screen
    /// </summary>
    public class StatusBarController : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// A reference to the text displaying the name of th. displayed location.
        /// </summary>
        [SerializeField]
        private TMPro.TMP_Text m_locationText;

        /// <summary>
        /// A reference to the text displaying the current time and date.
        /// </summary>
        [SerializeField]
        private TMPro.TMP_Text m_dateTimeText;

        #endregion

        #region Public Methods

        /// <summary>
        /// Modify the date/time text
        /// </summary>
        /// <param name="text"></param>
        public void SetDateTimeText(string text)
        {
            m_dateTimeText.SetText(text);
        }

        /// <summary>
        /// Set the location text
        /// </summary>
        /// <param name="text"></param>
        public void SetLocationText(string text)
        {
            m_locationText.SetText(text);
        }

        #endregion
    }
}
