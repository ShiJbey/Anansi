using System.Collections.Generic;
using UnityEngine;

namespace Calypso
{
    /// <summary>
    /// Provides Unity MonoBehavior Interface to each character's/location's StoryletManager.
    /// </summary>
    public class StoryletController : MonoBehaviour
    {
        /// <summary>
        /// Serializable class used for configuring ink script data in the Unity Inspector.
        /// </summary>
        [System.Serializable]
        public class InkScript
        {
            /// <summary>
            /// A reference to a TextAsset containing JSON data for an Ink Script.
            /// </summary>
            public TextAsset storyFile;

            /// <summary>
            /// The prefixes of Knot IDs of Knots to load as storylets from the story file.
            /// </summary>
            public List<string> storyletIDPrefixes;
        }

        /// <summary>
        /// Ink JSON files containing storylet information.
        /// </summary>
        [SerializeField]
        protected List<InkScript> _storyFiles;

        /// <summary>
        /// Manages all the storylets associated with this character/location.
        /// </summary>
        protected StoryletManager _storyletManager;

        void Awake()
        {
            // Create the storylet manager script
            _storyletManager = new StoryletManager();

            // Add error message handling to loaded stories
            _storyletManager.OnStoryAdded += (story) =>
            {
                story.onError += (msg, type) =>
                            {
                                if (type == Ink.ErrorType.Warning)
                                    Debug.LogWarning(msg);
                                else
                                    Debug.LogError(msg);
                            };
            };

            // Load all the story assets from the JSON files
            foreach (InkScript entry in _storyFiles)
            {
                if (entry.storyFile == null)
                {
                    Debug.LogWarning($"Missing story file asset reference for {gameObject.name}.");
                    continue;
                }

                Ink.Runtime.Story story = new Ink.Runtime.Story(entry.storyFile.text);

                foreach (string idPrefix in entry.storyletIDPrefixes)
                {
                    LoadStory(story, idPrefix);
                }
            }
        }

        /// <summary>
        /// Load a stories knots as storylets.
        /// </summary>
        /// <param name="story"></param>
        /// <param name="storyletIDPrefix"></param>
        public void LoadStory(Ink.Runtime.Story story, string storyletIDPrefix)
        {
            _storyletManager.AddStorylets(story, storyletIDPrefix);
        }
    }
}
