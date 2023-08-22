using System.Collections.Generic;
using UnityEngine;


namespace Calypso.Unity
{
    /// <summary>
    /// Tracks all the characters in the story
    /// </summary>
    public class ActorGroup : MonoBehaviour
    {
        private List<Actor> _actors;

        public List<Actor> Actors { get { return _actors; } }

        // Start is called before the first frame update
        void Awake()
        {
            _actors = new List<Actor>();

            foreach(var actor in gameObject.GetComponentsInChildren<Actor>())
            {
                _actors.Add(actor);
                Debug.Log($"Added {actor.DisplayName} to the CharacterGroup.");
            }
        }
    }
}

