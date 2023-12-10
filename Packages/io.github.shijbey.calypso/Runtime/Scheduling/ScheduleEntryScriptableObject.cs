using UnityEngine;

namespace Calypso.Scheduling
{
    /// <summary>
    /// ScriptableObject data for an entry within a character's schedule 
    /// or the game's SystemSchedule.
    /// </summary>
    public abstract class ScheduleEntryScriptableObject : ScriptableObject
    {
        #region Fields
        [SerializeField]
        protected int _startTime;

        [SerializeField]
        protected int _endTime;

        [SerializeField]
        protected bool _isRepeatable;
        #endregion

        public abstract IScheduleEntry CreateScheduleEntry();
    }
}

