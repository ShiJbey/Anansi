using System;
using UnityEngine;

namespace Calypso
{
    /// <summary>
    /// Tracks information about a date and time of day in the game
    /// </summary>
    [Serializable]
    public class SimDateTime
    {
        #region Fields

        [SerializeField]
        private int m_day;

        [SerializeField]
        private TimeOfDay m_timeOfDay;

        #endregion

        #region Properties

        /// <summary>
        /// The day
        /// </summary>
        public int Day => m_day;

        /// <summary>
        /// The time of the day
        /// </summary>
        public TimeOfDay TimeOfDay => m_timeOfDay;

        #endregion

        #region Constructors

        public SimDateTime()
        {
            m_day = 1;
            m_timeOfDay = TimeOfDay.Morning;
        }

        public SimDateTime(int day, TimeOfDay timeOfDay)
        {
            if (day < 1)
            {
                throw new ArgumentException("Argument 'day' for SimDateTime must be 1 or greater");
            }

            m_day = day;
            m_timeOfDay = timeOfDay;
        }

        public SimDateTime(SimDateTime other)
        {
            m_day = other.Day;
            m_timeOfDay = other.TimeOfDay;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Advance to the next time of day.
        ///
        /// <para>
        /// This will advance to the next day if we are currently at the last time of the day
        /// </para>
        /// </summary>
        public void AdvanceTime()
        {
            if (TimeOfDay == TimeOfDay.Night)
            {
                m_day += 1;
            }
            m_timeOfDay = (TimeOfDay)(((int)m_timeOfDay + 1) % 4);
        }

        public override string ToString()
        {
            return $"{TimeOfDay}, Day {Day}";
        }

        #endregion

        #region OperatorOverloads

        public static bool operator ==(SimDateTime a, SimDateTime b)
        {
            return a.Day == b.Day && a.TimeOfDay == b.TimeOfDay;
        }

        public static bool operator !=(SimDateTime a, SimDateTime b)
        {
            return a.Day != b.Day || a.TimeOfDay != b.TimeOfDay;
        }

        public static bool operator <(SimDateTime a, SimDateTime b)
        {
            if (a.Day < b.Day) return true;

            if (a.Day == b.Day && a.TimeOfDay < b.TimeOfDay) return true;

            return false;
        }

        public static bool operator >(SimDateTime a, SimDateTime b)
        {
            if (a.Day > b.Day) return true;

            if (a.Day == b.Day && a.TimeOfDay > b.TimeOfDay) return true;

            return false;
        }

        public static bool operator <=(SimDateTime a, SimDateTime b)
        {
            if (a.Day < b.Day) return true;

            if (a.Day == b.Day && a.TimeOfDay <= b.TimeOfDay) return true;

            return false;
        }

        public static bool operator >=(SimDateTime a, SimDateTime b)
        {
            if (a.Day > b.Day) return true;

            if (a.Day == b.Day && a.TimeOfDay >= b.TimeOfDay) return true;

            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is SimDateTime)
            {
                var otherDate = (SimDateTime)obj;
                return this.Day == otherDate.Day && this.TimeOfDay == otherDate.TimeOfDay;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }

    /// <summary>
    /// The various phases within a single day
    /// </summary>
    public enum TimeOfDay
    {
        Morning = 0,
        Afternoon = 1,
        Evening = 2,
        Night = 3,
    }
}
