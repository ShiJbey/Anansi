using Ink.Runtime;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace Calypso
{
    /// <summary>
    /// The conversation manager holds references to all the conversation 
    /// text assets associated with a character
    /// </summary>
    public class ConversationManager : MonoBehaviour
    {
        /// <summary>
        /// Conversation assets set within the Unity inspector
        /// </summary>
        [SerializeField]
        private List<TextAsset> _conversationScripts;

        /// <summary>
        /// Conversation objects constructed when the game starts
        /// </summary>
        private List<Conversation> _converstations = new List<Conversation>();


        void Awake()
        {
            ProcessConversationScripts();
        }

        /// <summary>
        /// Convert the list of text assets into Conversation objects
        /// </summary>
        private void ProcessConversationScripts()
        {
            var speaker = GetComponent<Unity.Actor>();

            foreach (var script in _conversationScripts)
            {
                var conversation = Conversation.FromInkScript(speaker, script);
                _converstations.Add(conversation);
                Debug.Log($"Created conversation '{conversation.ID}' for {speaker.DisplayName}.");
            }
        }

        /// <summary>
        /// Randomly choose a conversation and return it
        /// </summary>
        /// <returns></returns>
        public Conversation SelectConversation(StoryDatabase db)
        {
            // Select a random conversation
            var selectedConversation  = _converstations.Select((c) =>
                {
                    var queryResult = c.PreconditionQuery.Run(db);
                    return Tuple.Create(c, queryResult);
                })
                .Where((pair) =>
                {
                    return pair.Item2.Success;
                })
                .RandomElementByWeight((pair) => pair.Item1.Weight);



            return selectedConversation.Item1;
        }
    }

    public class Conversation {

        
        private Story _story;
        private int _weight;
        private string _id;
        private DBQuery _preconditionQuery;

        /// <summary>
        /// The relative frequency of this conversation being chosen compared to other
        /// conversations the owning character has access to.
        /// </summary>
        public int Weight { get { return _weight; } }

        /// <summary>
        /// A string identifier associated with this convertation
        /// </summary>
        public string ID { get { return _id;  } }

        /// <summary>
        /// Ink Runtime story object
        /// </summary>
        public Story Story {  get { return _story; } }

        /// <summary>
        /// Reference to the query for preconditions that need to be passed to
        /// select this conversation
        /// </summary>
        public DBQuery PreconditionQuery { get { return _preconditionQuery; } }

        public Calypso.Unity.Actor Speaker { get; }

        private Conversation(Calypso.Unity.Actor speaker, Story story, int weight, string id, DBQuery query)
        {
            Speaker = speaker;
            _story = story;
            _weight = weight;
            _id = id;
            _preconditionQuery = query;
        }

        public static Conversation FromInkScript(Calypso.Unity.Actor speaker, TextAsset conversationScript)
        {
            Story story = new Story(conversationScript.text);

            story.onError += (msg, type) => {
                if (type == Ink.ErrorType.Warning)
                    Debug.LogWarning(msg);
                else
                    Debug.LogError(msg);
            };

            var globalTagData = ProcessGlobalTags(story.globalTags);
            return new Conversation(speaker, story, globalTagData.weight, globalTagData.id, globalTagData.query);
        }

        private static GlobalTagData ProcessGlobalTags(List<string> tags)
        {
            var tagData = new GlobalTagData();

            foreach(string line in tags)
            {
                var parts = line.Split(':', 2);
                var key = parts[0].Trim().ToLower();
                var value = parts[1].Trim();

                switch (key) {
                    case "id":
                        tagData.id = value;
                        break;
                    case "weight":
                        tagData.weight = int.Parse(value);
                        break;
                    case "where":
                        tagData.query = tagData.query.Where(value);
                        break;
                    case "set":
                        tagData.setCalls.Add(value);
                        break;
                    default:
                        Debug.LogWarning($"Unkown global tag key, '{key}'");
                        break;
                }

            }

            return tagData;
        }
    }

    class GlobalTagData
    {
        public string id;
        public int weight;
        public DBQuery query;
        public List<string> setCalls;

        public GlobalTagData()
        {
            id = "";
            weight = 1;
            query = new DBQuery();
            setCalls = new List<string>();
        }
    }
}

