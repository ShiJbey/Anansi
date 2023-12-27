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

        [SerializeField]
        private int _startingDay = 1;

        [SerializeField]
        private TimeOfDay _startingTimeOfDay = TimeOfDay.Morning;

        private SimDateTime _date;

        #endregion

        #region Properties

        public SimDateTime Date { get { return new SimDateTime(_date); } }

        #endregion

        #region Actions and Events

        /// <summary>
        /// Event invoked when the date is updated
        /// </summary>
        public static UnityAction<SimDateTime> OnTimeChanged;

        #endregion

        private void Awake()
        {
            _date = new SimDateTime(_startingDay, _startingTimeOfDay);
        }

        // Start is called before the first frame update
        void Start()
        {
            if (OnTimeChanged != null) OnTimeChanged.Invoke(_date);
        }

        /// <summary>
        /// Advance the time by one timestep
        /// </summary>
        /// <param name="deltaTime"></param>
        public void AdvanceTime()
        {
            _date.AdvanceTime();
            if (OnTimeChanged != null) OnTimeChanged.Invoke(_date);
        }
    }
}
