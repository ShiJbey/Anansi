using Calypso.Scheduling;
using UnityEngine;

namespace Calypso.Actions
{
    /// <summary>
    /// A schedule action that moves a character to a new location
    /// </summary>
    public class GoToLocationAction : ActionScriptableObject
    {
        [SerializeField]
        private GameObject Location;

        public override void Execute()
        {

        }
    }
}

