using System;

namespace Calypso
{
    /// <summary>
    /// Tracks information about the current date and time of day
    /// </summary>
    [Serializable]
    public class SimDateTime
    {
        #region Public Properties

        /// <summary>
        /// The current day
        /// </summary>
        public int Day { get; private set; }

        /// <summary>
        /// The current time of the day
        /// </summary>
        public TimeOfDay TimeOfDay { get; private set; }

        #endregion

        #region Constructors

        public SimDateTime()
        {
            Day = 1;
            TimeOfDay = TimeOfDay.Morning;
        }

        public SimDateTime(int day, TimeOfDay timeOfDay)
        {
            if (day < 1)
            {
                throw new ArgumentException("Argument 'day' for SimDateTime must be 1 or greater");
            }

            Day = day;
            TimeOfDay = timeOfDay;
        }

        public SimDateTime(SimDateTime other)
        {
            Day = other.Day;
            TimeOfDay = other.TimeOfDay;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Advance to the next time of day. This will advance to the next day if current time of
        /// day is night.
        /// </summary>
        public void AdvanceTime()
        {
            if (TimeOfDay == TimeOfDay.Night)
            {
                Day += 1;
            }
            this.TimeOfDay = (TimeOfDay)(((int)this.TimeOfDay + 1) % 4);
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

    public enum TimeOfDay
    {
        Morning = 0,
        Afternoon = 1,
        Evening = 2,
        Night = 3,
    }
}
