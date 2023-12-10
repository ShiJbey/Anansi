using Calypso.Scheduling;
using UnityEngine;

namespace Calypso.Actions
{
    [CreateAssetMenu(fileName = "NewAction", menuName = "Calypso/Actions/PrintMessage")]
    public class PrintMessageAction : ActionScriptableObject
    {
        [SerializeField]
        private string _message;

        public override void Execute()
        {
            Debug.Log(_message);
        }
    }
}

