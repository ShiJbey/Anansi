using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Calypso.RePraxis;

namespace Calypso
{
    /// <summary>
    /// Provides Unity MonoBehavior Interface to each character's/location's StoryletManager.
    /// </summary>
    public class StoryletController : MonoBehaviour
    {
        /// <summary>
        /// The prefix value that the manager looks for when loading storylets from a Story
        /// </summary>
        private const string STORYLET_ID_PREFIX = "storylet_";

        /// <summary>
        /// Ink JSON files containing storylet information.
        /// </summary>
        [SerializeField]
        protected List<TextAsset> _storyFiles;

        /// <summary>
        /// Manages all the storylets associated with this character/location.
        /// </summary>
        protected StoryletManager _storyletManager;

        /// <summary>
        /// All stories loaded into the manager.
        /// </summary>
        protected List<Ink.Runtime.Story> _stories = new List<Ink.Runtime.Story>();

        /// <summary>
        /// All storylets loaded from the loaded stories.
        /// </summary>
        protected List<Storylet> _storylets = new List<Storylet>();

        private void Awake()
        {
            // Create the storylet manager script
            _storyletManager = new StoryletManager();

            // Load all the story assets from the JSON files
            foreach (TextAsset entry in _storyFiles)
            {
                if (entry == null) continue;

                Ink.Runtime.Story story = new Ink.Runtime.Story(entry.text);

                LoadStoryletsFromStory(story);
            }
        }

        /// <summary>
        /// Get all storylets that are not on cooldown.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Storylet> GetStorylets()
        {
            return _storylets.Where(s => s.CooldownTimeRemaining <= 0);
        }

        /// <summary>
        /// Get all storylets managed by this controller.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Storylet> GetAllStorylets()
        {
            return _storylets;
        }

        /// <summary>
        /// Create new storylets from various knots in the given Ink story
        /// </summary>
        /// <param name="story"></param>
        /// <exception cref="ArgumentException"></exception>
        private void LoadStoryletsFromStory(Ink.Runtime.Story story)
        {
            if (!_stories.Contains(story)) _stories.Add(story);

            List<string> knotIDs = GetAllKnotIDs(story);

            foreach (string knotID in knotIDs)
            {
                if (knotID.StartsWith(STORYLET_ID_PREFIX))
                {
                    // Using a _ as a prefix for the function
                    string functionName = "_" + knotID;
                    if (!knotIDs.Contains(functionName))
                    {
                        // Debug.LogError();
                        throw new ArgumentException(
                            $"Can't find test function {functionName} for storylet {knotID}.");
                    }

                    // Create a new storylet
                    Storylet storylet = new Storylet(knotID, story);
                    _storylets.Add(storylet);

                    // Now we have to get the metadata tags from the knot
                    List<string> knotTags = story.TagsForContentAtPath(knotID);

                    if (knotTags == null)
                    {
                        continue;
                    }

                    for (int i = 0; i < knotTags.Count;)
                    {
                        string line = knotTags[i].Trim();

                        string[] parts = line.Split(">>").Select(s => s.Trim()).ToArray();

                        // This is a variable substitution
                        if (parts[0] == "set")
                        {
                            if (parts.Length < 2)
                            {
                                throw new ArgumentException("Missing expression after 'Set>> '.");
                            }

                            string setExpression = parts[1];

                            // Should be of the form X to Y
                            string[] exprParts = setExpression.Split(" ").Select(s => s.Trim()).ToArray();

                            if (exprParts.Length != 3)
                            {
                                throw new ArgumentException(
                                    "Set expression must be of the form `Set>> X to Y");
                            }

                            storylet.VariableSubstitutions[exprParts[0]] = exprParts[2];
                        }

                        // This is the start of a query read all following lines until we reach
                        // an 'End' statement
                        if (parts[0] == "query")
                        {
                            List<string> queryLines = new List<string>();
                            bool endReached = false;

                            i++;
                            while (!endReached && i < knotTags.Count)
                            {
                                string queryLine = knotTags[i].Trim();

                                if (queryLine.StartsWith("end"))
                                {
                                    endReached = true;
                                    break;
                                }

                                queryLines.Add(queryLine);
                                i++;
                            }

                            storylet.Query = new DBQuery(queryLines);
                        }

                        if (parts[0] == "cooldown")
                        {
                            // storylet.Cooldown = int.Parse(parts[1]);
                        }

                        if (parts[0] == "repeatable")
                        {
                            // storylet.IsRepeatable = bool.Parse(parts[1]);
                        }

                        if (parts[0] == "mandatory")
                        {
                            storylet.Mandatory = bool.Parse(parts[1]);
                        }

                        if (parts[0] == "tags")
                        {
                            string[] storyletTags = parts[1].Split(",").Select(s => s.Trim()).ToArray();
                            foreach (string t in storyletTags)
                            {
                                storylet.Tags.Add(t);
                            }
                        }
                        i++;
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves all the knot IDs from an Ink Story instance
        ///
        /// <remark>
        /// This function depends on Ink version 1.1.7
        /// </remark>
        /// </summary>
        /// <param name="story"></param>
        /// <returns></returns>
        private static List<string> GetAllKnotIDs(Ink.Runtime.Story story)
        {
            List<string> knotList = new List<string>();

            Ink.Runtime.Container mainContentContainer = story.mainContentContainer;
            if (mainContentContainer == null)
                return knotList;

            foreach (string name in mainContentContainer.namedOnlyContent.Keys)
            {
                // Don't want this as it's Ink internal
                if (name == "global decl")
                    continue;

                knotList.Add(name);
            }

            return knotList;
        }

    }
}
