using UnityEngine;
using UnityEngine.Events;

namespace Calypso.Unity
{
    /// <summary>
    /// Manages the current time in the simulation.
    /// 
    /// This code is adapted from this tutorial by Dan Pos:
    /// https://youtu.be/wTH5iwO4n0s
    /// </summary>
    public class TimeManager : MonoBehaviour
    {
        #region Fields
        [Header("Date & Time Settings")]
        [Range(1, 28)]
        [SerializeField]
        private int _dayInMonth;

        [Range(1, 12)]
        [SerializeField]
        private int _month;

        [Range(1, 100)]
        [SerializeField]
        private int _year;

        [Range(0, 23)]
        [SerializeField]
        private int _hour;

        [Range(0, 59)]
        [SerializeField]
        private int _minutes;

        [SerializeField]
        private DeltaTime _timeInterval;

        private SimDateTime _date;
        #endregion

        #region Properties
        public SimDateTime Date { get { return new SimDateTime(_date); } }
        #endregion

        #region Actions and Events
        public static UnityAction<SimDateTime> OnTimeChanged;
        #endregion

        private void Awake()
        {
            _date = new SimDateTime(_dayInMonth, _month, _year, _hour, _minutes);
        }

        // Start is called before the first frame update
        void Start()
        {
            OnTimeChanged?.Invoke(_date);
        }
        
        /// <summary>
        /// Update the current time by the given delta
        /// </summary>
        /// <param name="deltaTime"></param>
        public void AdvanceTime()
        {
            _date.AdvanceTime(_timeInterval);
            OnTimeChanged?.Invoke(_date);
        }
    }
}
