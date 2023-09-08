using UnityEngine;

namespace Calypso.Scheduling
{
    /// <summary>
    /// A ScriptableObject representation for a action to be executed 
    /// when activating a ScheduleEntry. 
    /// </summary>
    public abstract class ActionScriptableObject : ScriptableObject, IAction
    {
        public string ID => name;

        // Force subclasses to implement this interface
        public abstract void Execute();
    }
}


