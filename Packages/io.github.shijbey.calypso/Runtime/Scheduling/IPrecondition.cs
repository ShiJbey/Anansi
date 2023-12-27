using UnityEngine;

namespace Calypso
{
    public interface IPrecondition
    {
        public bool CheckPrecondition(GameObject gameObject);
    }
}
