using UnityEngine;

namespace Calypso.Unity
{
    public class StatusBarController : MonoBehaviour
    {
        [SerializeField]
        private TMPro.TMP_Text _locationText;

        [SerializeField]
        private TMPro.TMP_Text _dateTimeText;

        public void SetDatTimeText(string text)
        {
            _dateTimeText.SetText(text);
        }

        public void SetLocationText(string text)
        {
            _locationText.SetText(text);
        }
    }
}
