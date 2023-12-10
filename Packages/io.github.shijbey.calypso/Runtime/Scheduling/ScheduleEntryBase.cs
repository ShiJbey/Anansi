using System;
using System.Collections.Generic;

namespace Calypso.Scheduling
{
    /// <summary>
    /// An abstract base class to simplify making classes that implement ISchedule entry.
    /// </summary>
    public abstract class ScheduleEntryBase : IScheduleEntry
    {
        #region Fields
        protected string _scheduleID;
        protected int _startTime;
        protected int _endTime;
        protected int _lastCompleted;
        protected bool _isRepeatable;
        #endregion

        #region Properties
        public string ID => _scheduleID;
        public int StartTime => _startTime;
        public int EndTime => _endTime;
        public int LastCompleted { get => _lastCompleted; set => _lastCompleted = value; }
        public bool IsRepeatable => _isRepeatable;
        #endregion

        #region Constructor
        public ScheduleEntryBase(string scheduleID, int startTime, int endTime, bool isRepeatable)
        {
            _scheduleID = scheduleID;
            _startTime = startTime;
            _endTime = endTime;
            _lastCompleted = 0;
            _isRepeatable = isRepeatable;
        }
        #endregion

        #region Methods
        public abstract void AddAction(IAction action);
        public abstract void Execute(SimDateTime currentTime);
        #endregion
    }

    /// <summary>
    /// A schedule entry that may only hold a single action to perform.
    /// </summary>
    public class SimpleScheduleEntry : ScheduleEntryBase
    {
        #region Fields
        /// <summary>
        /// The single action stored inside the entry.
        /// </summary>
        private IAction _action;
        #endregion

        #region Constructors
        public SimpleScheduleEntry(string scheduleID, int startTime, int endTime, bool isRepeatable)
            : base(scheduleID, startTime, endTime, isRepeatable)
        {
            _action = null;
        }
        #endregion

        #region Methods
        public override void AddAction(IAction action)
        {
            if (_action != null)
            {
                throw new Exception("Action already provided for simple schedule entry.");
            }
            _action = action;
        }

        public override void Execute(SimDateTime currentTime)
        {
            _lastCompleted = currentTime.TotalNumDays;
            _action.Execute();
        }
        #endregion
    }

    /// <summary>
    /// Performs weighted random selection over the provided actions
    /// </summary>
    public class WeightedRandomScheduleEntry : ScheduleEntryBase
    {
        private List<IAction> _actions;
        private List<int> _weights;
        private int _totalWeight;

        public WeightedRandomScheduleEntry(string scheduleID, int startTime, int endTime, bool isRepeatable)
            : base(scheduleID, startTime, endTime, isRepeatable)
        {
            _actions = new List<IAction>();
            _weights = new List<int>();
            _totalWeight = 0;
        }

        public override void AddAction(IAction action)
        {
            AddAction(action, 1);
        }

        public void AddAction(IAction action, int weight)
        {
            _actions.Add(action);
            _weights.Add(weight);
            _totalWeight += weight;
        }

        public override void Execute(SimDateTime currentTime)
        {
            _lastCompleted = currentTime.TotalNumDays;

            int totalActions = _actions.Count;
            float chosenWeightIndex = (float)new Random().NextDouble() * _totalWeight;
            float currentWeightIndex = 0;

            for (int i = 0; i < totalActions; i++)
            {
                var action = _actions[i];
                var weight = _weights[i];

                currentWeightIndex += weight;

                if (currentWeightIndex >= chosenWeightIndex)
                {
                    action.Execute();
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Performs provided actions in order of addition.
    /// </summary>
    public class SequentialScheduleEntry : ScheduleEntryBase
    {
        private List<IAction> _actions;

        public SequentialScheduleEntry(string scheduleID, int startTime, int endTime, bool isRepeatable)
            : base(scheduleID, startTime, endTime, isRepeatable)
        {
            _actions = new List<IAction>();
        }

        public override void AddAction(IAction action)
        {
            _actions.Add(action);
        }

        public override void Execute(SimDateTime currentTime)
        {
            _lastCompleted = currentTime.TotalNumDays;
            foreach (var a in _actions)
            {
                a.Execute();
            }
        }
    }
}