using Ink.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Calypso
{
    /// <summary>
    /// The conversation manager holds references to all the conversation 
    /// text assets associated with a character
    /// </summary>
    public class ConversationManager : MonoBehaviour
    {
        [SerializeField]
        private List<TextAsset> _conversationScripts;

        private List<Conversation> _converstations;

        void Start()
        {
            ProcessConversationScripts();
        }

        /// <summary>
        /// Convert the list of text assets into Conversation objects
        /// </summary>
        private void ProcessConversationScripts()
        {
            _converstations = new List<Conversation>();
            foreach(var script in _conversationScripts)
            {
                _converstations.Add(Conversation.FromInkScript(script));
            }
        }
    }

    public class Conversation {

        /// <summary>
        /// Ink Runtime story object
        /// </summary>
        private Story _story;

        /// <summary>
        /// The relative frequence of this conversation being chosen compared to other
        /// conversations the owning character has access to.
        /// </summary>
        private int _weight;

        /// <summary>
        /// A string identifier associated with this convertation
        /// </summary>
        private string _storyId;

        private Conversation(Story story, int weight, string storyId)
        {
            _story = story;
            _weight = weight;
            _storyId = storyId;
        }

        
        private static int ExtractWeight(Story story)
        {
            foreach (string line in story.globalTags)
            {
                if (line.StartsWith("weight"))
                {
                    return int.Parse(line.Split(':')[1].Trim());
                }
            }
            throw new System.Exception("Missing 'weight: ...' in conversation global tags.");
        }

        private static string ExtractID(Story story)
        {
            foreach (string line in story.globalTags)
            {
                if (line.StartsWith("id"))
                {
                    return line.Split(':')[1].Trim();
                }
            }
            throw new System.Exception("Missing 'id: ...' in conversation global tags.");
        }


        public static Conversation FromInkScript(TextAsset conversationScript)
        {
            Story story = new Story(conversationScript.text);

            story.onError += (msg, type) => {
                if (type == Ink.ErrorType.Warning)
                    Debug.LogWarning(msg);
                else
                    Debug.LogError(msg);
            };

            int weight = Conversation.ExtractWeight(story);
            string storyId = Conversation.ExtractID(story);

            return new Conversation(story, weight, storyId);
        }
    
    }
}

