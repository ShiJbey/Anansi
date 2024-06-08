using System.Collections.Generic;
using System.Xml;

namespace Anansi.Scheduling
{
    /// <summary>
    /// Manages a collection of entries that determine what should happen at a given time.
    /// </summary>
    public class Schedule
    {
        #region Protected Fields

        /// <summary>
        /// All the entries that belong to a schedule.
        /// </summary>
        protected List<ScheduleEntry> _entries;

        /// <summary>
        /// The preconditions to check if this schedule applies
        /// </summary>
        protected List<IPrecondition> _preconditions;

        #endregion

        #region Constructors

        public Schedule(IEnumerable<IPrecondition> preconditions, IEnumerable<ScheduleEntry> entries)
        {
            _preconditions = new List<IPrecondition>( preconditions );
            _entries = new List<ScheduleEntry>( entries );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Add an entry to the schedule
        /// </summary>
        /// <param name="entry"></param>
        public void AddEntry(ScheduleEntry entry)
        {
            _entries.Add( entry );
        }

        /// <summary>
        /// Get schedule entries for the given date/time
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public ScheduleEntry[] GetEntries(SimDateTime dateTime)
        {
            List<ScheduleEntry> entriesForTime = new List<ScheduleEntry>();

            foreach ( ScheduleEntry entry in _entries )
            {
                if ( entry.TimeOfDay == dateTime.TimeOfDay )
                {
                    entriesForTime.Add( entry );
                }
            }

            return entriesForTime.ToArray();
        }

        /// <summary>
        /// Remove an entry from the from the schedule.
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public bool RemoveEntry(ScheduleEntry entry)
        {
            return _entries.Remove( entry );
        }

        /// <summary>
        /// Check all the preconditions for this
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public bool CheckPreconditions(SimDateTime dateTime)
        {
            foreach ( var precondition in _preconditions )
            {
                if ( precondition.CheckPrecondition( dateTime ) == false ) return false;
            }
            return true;
        }


        public static Schedule ParseXml(XmlNode scheduleNode)
        {
            XmlElement scheduleElement = (XmlElement)scheduleNode;

            List<IPrecondition> preconditions = new List<IPrecondition>();
            XmlElement preconditionsElem = scheduleElement["Preconditions"];
            for ( int i = 0; i < preconditionsElem.ChildNodes.Count; i++ )
            {
                XmlNode preconditionNode = preconditionsElem.ChildNodes[i];

                if ( preconditionNode.NodeType != XmlNodeType.Element ) continue;

                string preconditionType = ((XmlElement)preconditionNode).GetAttribute( "type" );

                try
                {
                    IPreconditionFactory factory = PreconditionLibrary.factories[preconditionType];

                    IPrecondition precondition = factory.CreatePrecondition( preconditionNode );

                    preconditions.Add( precondition );
                }
                catch ( KeyNotFoundException )
                {
                    throw new KeyNotFoundException(
                        $"Precondition '{preconditionType}' does not exist."
                        + " Are you missing a factory?"
                    );
                }


            }


            List<ScheduleEntry> entries = new List<ScheduleEntry>();
            XmlElement entriesElem = scheduleElement["ScheduleEntries"];
            for ( int i = 0; i < entriesElem.ChildNodes.Count; i++ )
            {
                XmlNode entryNode = entriesElem.ChildNodes[i];

                if ( entryNode.NodeType != XmlNodeType.Element ) continue;

                ScheduleEntry entry = ScheduleEntry.ParseXml( entryNode );

                entries.Add( entry );
            }

            return new Schedule( preconditions, entries );
        }

        #endregion
    }
}
