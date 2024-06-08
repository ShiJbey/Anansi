using System;
using System.Xml;
using Unity.IO.LowLevel.Unsafe;

namespace Anansi.Scheduling
{
    /// <summary>
    /// An abstract base class to simplify making classes that implement ISchedule entry.
    /// </summary>
    public class ScheduleEntry
    {
        #region Public Properties

        public TimeOfDay TimeOfDay { get; }
        public string Location { get; }

        public int Priority { get; }

        #endregion

        #region Constructor


        public ScheduleEntry(TimeOfDay timeOfDay, string location, int priority)
        {
            TimeOfDay = timeOfDay;
            Location = location;
            Priority = priority;
        }

        #endregion

        public static ScheduleEntry ParseXml(XmlNode node)
        {
            XmlElement entryElem = (XmlElement)node;

            TimeOfDay timeOfDay = Enum.Parse<TimeOfDay>( entryElem.GetAttribute( "timeOfDay" ) );
            string location = entryElem.GetAttribute( "location" );

            int priority = 0;
            if ( entryElem.HasAttribute( "priority" ) )
            {
                priority = int.Parse( entryElem.GetAttribute( "priority" ) );
            }

            return new ScheduleEntry( timeOfDay, location, priority );
        }
    }
}
