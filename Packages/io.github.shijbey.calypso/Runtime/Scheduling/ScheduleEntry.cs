using System;
using System.Xml;

namespace Calypso.Scheduling
{
    /// <summary>
    /// An abstract base class to simplify making classes that implement ISchedule entry.
    /// </summary>
    public class ScheduleEntry
    {
        #region Public Properties

        public TimeOfDay TimeOfDay { get; }
        public string Location { get; }

        #endregion

        #region Constructor


        public ScheduleEntry(TimeOfDay timeOfDay, string location)
        {
            TimeOfDay = timeOfDay;
            Location = location;
        }

        #endregion

        public static ScheduleEntry ParseXml(XmlNode node)
        {
            XmlElement entryElem = (XmlElement)node;

            TimeOfDay timeOfDay = Enum.Parse<TimeOfDay>(entryElem.GetAttribute("timeOfDay"));
            string location = entryElem.GetAttribute("location");

            return new ScheduleEntry(timeOfDay, location);
        }
    }
}
