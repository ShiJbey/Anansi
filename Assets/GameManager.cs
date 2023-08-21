using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Calypso.Unity;


namespace Calypso
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField]
        private ActorGroup _actors;

        [SerializeField]
        private DialogueManager _dialogueManager;

        private StoryDatabase _storyDatabase = new StoryDatabase();

        // Start is called before the first frame update
        void Start()
        {
            // TESTING CODE
            // Select a character at random and initiate a conversation

            int selectedIndex = Random.Range(0, _actors.Actors.Count);
            var selectedActor = _actors.Actors[selectedIndex];

            var conversation = selectedActor.GetComponent<ConversationManager>().SelectConversation(_storyDatabase);

            _dialogueManager.StartConversation(conversation);
        }
    }
}

