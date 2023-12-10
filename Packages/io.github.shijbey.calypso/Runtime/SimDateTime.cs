using System.Runtime.ConstrainedExecution;
using System;
using UnityEngine.XR;

namespace Calypso
{
    /// <summary>
    /// A custom date/time implementation that represents years as 12 months
    /// with 4, 7-day weeks per month. The smallest unit of time is one minute.
    /// </summary>
    [System.Serializable]
    public struct SimDateTime
    {
        #region Constants
        public static readonly int MINUTES_PER_HOUR = 60;
        public static readonly int HOURS_PER_DAY = 24;
        public static readonly int DAYS_PER_WEEK = 7;
        public static readonly int DAYS_PER_MONTH = 28;
        public static readonly int DAYS_PER_YEAR = 336;
        public static readonly int WEEKS_PER_MONTH = 4;
        public static readonly int MONTHS_PER_YEAR = 12;
        public static readonly int WEEKS_PER_YEAR = 48;
        #endregion

        #region Fields
        private int _date;
        private int _month;
        private int _year;
        private int _hour;
        private int _minutes;
        private int _totalNumDays;
        private int _totalNumWeeks;
        private DeltaTime _deltaTime;
        #endregion

        #region Properties
        public Days Day => (Days)(_date % DAYS_PER_WEEK);
        public int Date => _date + 1;
        public Months Month => (Months)_month;
        public int Year => _year + 1;
        public int Hour => _hour;
        public int Minutes => _minutes;
        public int TotalNumDays => _totalNumDays;
        public int TotalNumWeeks => _totalNumWeeks;
        public int TotalHours
        {
            get
            {
                return _hour
                    + (_date * HOURS_PER_DAY)
                    + (_month * HOURS_PER_DAY * DAYS_PER_MONTH)
                    + (_year * HOURS_PER_DAY * DAYS_PER_YEAR);
            }
        }
        public int TotalMinutes
        {
            get
            {
                return _minutes
                    + (_hour * MINUTES_PER_HOUR)
                    + (_date * MINUTES_PER_HOUR * HOURS_PER_DAY)
                    + (_month * MINUTES_PER_HOUR * HOURS_PER_DAY * DAYS_PER_MONTH)
                    + (_year * MINUTES_PER_HOUR * HOURS_PER_DAY * DAYS_PER_YEAR);
            }
        }
        public int Week
        {
            get
            {
                return
                    _totalNumWeeks % (WEEKS_PER_YEAR) == 0
                    ? (WEEKS_PER_YEAR)
                    : _totalNumWeeks % (WEEKS_PER_YEAR);
            }
        }
        public DeltaTime DeltaTime => _deltaTime;
        public TimeOfDay TimeOfDay
        {
            get
            {
                if (_hour >= 5 && _hour < 12)
                {
                    return TimeOfDay.Morning;
                }
                else if (_hour >= 12 && _hour < 17)
                {
                    return TimeOfDay.Afternoon;
                }
                else if (_hour >= 17 && _hour < 21)
                {
                    return TimeOfDay.Evening;
                }
                else
                {
                    return TimeOfDay.Night;
                }
            }
        }
        #endregion

        #region Constructors
        public SimDateTime(int date, int month, int year, int hour, int minute)
        {
            if (hour < 0 || hour >= HOURS_PER_DAY)
                throw new System.ArgumentException(
                    $"Hours must be between 0 and {HOURS_PER_DAY - 1}.");

            if (minute < 0 || minute >= MINUTES_PER_HOUR)
                throw new System.ArgumentException(
                    $"Minutes must be between 0 and {MINUTES_PER_HOUR - 1}.");

            if (date < 1 || date > DAYS_PER_MONTH)
                throw new System.ArgumentException(
                    $"Date must be between 1 and {DAYS_PER_MONTH}.");

            if (month < 1 || month > MONTHS_PER_YEAR)
                throw new System.ArgumentException(
                    $"Month must be between 1 and {DAYS_PER_MONTH}.");

            if (year < 1)
                throw new System.ArgumentException(
                    "Year must be greater than or equal to 1");

            // Set the time
            _hour = hour;
            _minutes = minute;

            // Set the date
            _date = date - 1;
            _month = month - 1;
            _year = year - 1;

            _totalNumDays = _date + (_month * DAYS_PER_MONTH) + ((year - 1) * DAYS_PER_YEAR);
            _totalNumWeeks = _totalNumDays / DAYS_PER_WEEK;

            // No delta time when created
            _deltaTime = new DeltaTime(0, 0, 0, 0, 0);
        }

        public SimDateTime(SimDateTime original)
            : this(original.Date, (int)original.Month + 1, original.Year, original.Hour, original.Minutes)
        {

        }

        public SimDateTime(int date, int month, int year)
            : this(date, month, year, 0, 0)
        { }
        #endregion

        #region Methods
        public void AdvanceTime(DeltaTime deltaTime)
        {
            _deltaTime = deltaTime;

            int totalMinutes = _minutes + deltaTime.Minutes;
            int carriedHours = totalMinutes / MINUTES_PER_HOUR;

            _minutes = totalMinutes % MINUTES_PER_HOUR;

            int totalHours = _hour + deltaTime.Hours + carriedHours;
            int carriedDays = totalHours / HOURS_PER_DAY;

            _hour = totalHours % HOURS_PER_DAY;

            int totalDays = _date + deltaTime.Days + carriedDays;
            int carriedMonths = totalDays / DAYS_PER_MONTH;

            _date = totalDays % DAYS_PER_MONTH;

            int totalMonths = _month + deltaTime.Months + carriedMonths;
            int carriedYears = totalMonths / MONTHS_PER_YEAR;

            _month = totalMonths % MONTHS_PER_YEAR;
            _year += deltaTime.Years + carriedYears;
        }

        public SimDateTime Copy()
        {
            return new SimDateTime(this);
        }
        #endregion

        #region ToStrings
        public override string ToString()
        {
            return string.Format(
                "{0}, {1} {2:D2}, {3:D4} @ {4:D2}:{5:D2}",
                Day, Month, Date, Year, Hour, Minutes
                );
        }
        #endregion

        #region OperatorOverloads
        public static DeltaTime operator -(SimDateTime a, SimDateTime b)
        {
            int diffMinutes = a.TotalMinutes - b.TotalMinutes;

            if (diffMinutes < 0)
                throw new System.ArgumentException("Cannot subtract later date from past date.");

            // Convert minutes back to the various date components
            int remainder = diffMinutes;
            int years = remainder / (MINUTES_PER_HOUR * HOURS_PER_DAY * DAYS_PER_YEAR);

            remainder = remainder % (MINUTES_PER_HOUR * HOURS_PER_DAY * DAYS_PER_YEAR);
            int months = remainder / (MINUTES_PER_HOUR * HOURS_PER_DAY * DAYS_PER_MONTH);

            remainder = remainder % (MINUTES_PER_HOUR * HOURS_PER_DAY * DAYS_PER_MONTH);
            int days = remainder / (MINUTES_PER_HOUR * HOURS_PER_DAY);

            remainder = remainder % (MINUTES_PER_HOUR * HOURS_PER_DAY);
            int hours = remainder / MINUTES_PER_HOUR;

            int minutes = remainder;

            return new DeltaTime(years, months, days, hours, minutes);
        }

        public static SimDateTime operator +(SimDateTime time, DeltaTime deltaTime)
        {
            time.AdvanceTime(deltaTime);
            return time;
        }

        public static bool operator ==(SimDateTime a, SimDateTime b)
        {
            return a.TotalMinutes == b.TotalMinutes;
        }

        public static bool operator !=(SimDateTime a, SimDateTime b)
        {
            return a.TotalMinutes != b.TotalMinutes;
        }

        public static bool operator <(SimDateTime a, SimDateTime b)
        {
            return a.TotalMinutes < b.TotalMinutes;
        }

        public static bool operator >(SimDateTime a, SimDateTime b)
        {
            return a.TotalMinutes > b.TotalMinutes;
        }

        public static bool operator <=(SimDateTime a, SimDateTime b)
        {
            return a.TotalMinutes <= b.TotalMinutes;
        }

        public static bool operator >=(SimDateTime a, SimDateTime b)
        {
            return a.TotalMinutes >= b.TotalMinutes;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is SimDateTime)
            {
                var otherDate = (SimDateTime)obj;
                return this.TotalMinutes == otherDate.TotalMinutes;
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

    [System.Serializable]
    public struct DeltaTime
    {
        #region Fields
        public int _years;
        public int _months;
        public int _days;
        public int _hours;
        public int _minutes;
        #endregion

        #region Properties
        public int Years => _years;
        public int Months => _months;
        public int Days => _days;
        public int Hours => _hours;
        public int Minutes => _minutes;
        public int TotalHours
        {
            get
            {
                return _hours
                    + (_days * SimDateTime.HOURS_PER_DAY)
                    + (_months * SimDateTime.HOURS_PER_DAY * SimDateTime.DAYS_PER_MONTH)
                    + (_years * SimDateTime.HOURS_PER_DAY * SimDateTime.DAYS_PER_YEAR);
            }
        }
        public int TotalDays
        {
            get
            {
                return _days
                    + (_months * SimDateTime.DAYS_PER_MONTH)
                    + (_years * SimDateTime.DAYS_PER_YEAR);
            }
        }
        #endregion

        #region Constructors
        public DeltaTime(int years, int months, int days, int hours, int minutes)
        {
            if (years < 0)
                throw new System.ArgumentException("Years must be non-negative.");

            if (months < 0)
                throw new System.ArgumentException("Months must be non-negative");

            if (days < 0)
                throw new System.ArgumentException("Days must be non-negative.");

            if (hours < 0)
                throw new System.ArgumentException("Hours must be non-negative.");

            if (minutes < 0)
                throw new System.ArgumentException("Minutes must be non-negative.");

            _years = years;
            _months = months;
            _days = days;
            _hours = hours;
            _minutes = minutes;
        }
        #endregion
    }

    public enum Days
    {
        Monday = 0,
        Tuesday = 1,
        Wednesday = 2,
        Thursday = 3,
        Friday = 4,
        Saturday = 5,
        Sunday = 6
    }

    public enum Months
    {
        January = 0,
        February = 1,
        March = 2,
        April = 3,
        May = 4,
        June = 5,
        July = 6,
        August = 7,
        September = 8,
        October = 9,
        November = 10,
        December = 11,
    }

    public enum TimeOfDay
    {
        Morning = 1,
        Afternoon = 2,
        Evening = 3,
        Night = 4,
    }
}
