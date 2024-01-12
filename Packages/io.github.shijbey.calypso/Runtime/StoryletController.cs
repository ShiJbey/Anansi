using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RePraxis;

namespace Calypso
{
    /// <summary>
    /// Provides Unity MonoBehavior Interface to each character's/location's StoryletManager.
    /// </summary>
    public class StoryletController : MonoBehaviour
    {
        #region Constants

        /// <summary>
        /// The prefix value that the manager looks for when loading storylets from a Story
        /// </summary>
        private const string STORYLET_ID_PREFIX = "storylet_";

        #endregion

        #region Fields

        /// <summary>
        /// Ink JSON files containing storylet information.
        /// </summary>
        [SerializeField]
        protected List<TextAsset> m_storyFiles;

        /// <summary>
        /// All stories loaded into the manager.
        /// </summary>
        protected List<Ink.Runtime.Story> m_stories = new List<Ink.Runtime.Story>();

        /// <summary>
        /// All storylets loaded from the loaded stories.
        /// </summary>
        protected List<Storylet> m_storylets = new List<Storylet>();

        #endregion

        #region Properties

        /// <summary>
        /// All the storylets in this controller
        /// </summary>
        public List<Storylet> Storylets => m_storylets;

        #endregion

        #region Unity Messages

        private void Awake()
        {
            // Load all the story assets from the JSON files
            foreach (TextAsset entry in m_storyFiles)
            {
                if (entry == null) continue;

                Ink.Runtime.Story story = new Ink.Runtime.Story(entry.text);

                LoadStoryletsFromStory(story);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Create new storylets from various knots in the given Ink story
        /// </summary>
        /// <param name="story"></param>
        /// <exception cref="ArgumentException"></exception>
        private void LoadStoryletsFromStory(Ink.Runtime.Story story)
        {
            if (m_stories.Contains(story)) return;

            m_stories.Add(story);

            List<string> knotIDs = GetAllKnotIDs(story);

            foreach (string knotID in knotIDs)
            {
                if (knotID.StartsWith(STORYLET_ID_PREFIX))
                {
                    Storylet storylet = CreateStoryletFromKnot(story, knotID);
                    m_storylets.Add(storylet);
                }
            }
        }

        /// <summary>
        /// Create a storylet for a knot
        /// </summary>
        /// <param name="story"></param>
        /// <param name="knotID"></param>
        /// <returns></returns>
        private static Storylet CreateStoryletFromKnot(Ink.Runtime.Story story, string knotID)
        {
            // Now we have to get the metadata tags from the knot

            Queue<string> tagsQueue = new Queue<string>();

            bool isRepeatable = true;
            bool isMandatory = false;
            int weight = 0;
            int cooldown = 0;
            DBQuery precondition = null;
            List<string> tags = new List<string>();
            Dictionary<string, string> variableSubstitutions = new Dictionary<string, string>()
            {
                {"speaker", "?speaker"},
                {"player", "?player"},
                {"location", "?location"}
            };
            int firstLineTagOffset = 0;

            List<string> knotTags = story.TagsForContentAtPath(knotID);
            if (knotTags != null)
            {
                firstLineTagOffset = knotTags.Count;

                foreach (var tag in knotTags)
                {
                    tagsQueue.Enqueue(tag);
                }
            }



            // Loop through the queue until it is empty
            while (tagsQueue.Count > 0)
            {
                // Get the next line off the queue
                string line = tagsQueue.Dequeue().Trim();

                // Get the different parts of the line
                string[] parts = line.Split(">>").Select(s => s.Trim()).ToArray();

                if (parts.Length != 2)
                {
                    throw new ArgumentException(
                        $"Invalid expression '{line}' in knot '{knotID}'."
                    );
                }

                string command = parts[0];
                string[] arguments = parts[1].Split(" ")
                    .Select(s => s.Trim()).ToArray();

                // Perform a switch on the command type
                switch (command)
                {
                    case "set":
                        if (arguments.Length != 3)
                        {
                            throw new ArgumentException(
                                $"Invalid Set-expression '{line}' in knot '{knotID}'. "
                                + "Set expression must be of the form `set >> X to Y"
                            );
                        }

                        variableSubstitutions[arguments[0]] = arguments[2];

                        break;

                    case "weight":
                        if (!int.TryParse(arguments[0], out weight))
                        {
                            throw new ArgumentException(
                                $"Invalid value for weight in '{line}' of knot '{knotID}'. "
                                + "Acceptable values are integers greater than or equal to zero."
                            );
                        }

                        break;

                    case "query":
                        precondition = PreconditionQueryFromTags(knotID, tagsQueue);

                        break;

                    case "isRepeatable":
                        if (!bool.TryParse(arguments[0], out isRepeatable))
                        {
                            throw new ArgumentException(
                                $"Invalid value for isRepeatable in '{line}' of knot '{knotID}'. "
                                + "Acceptable values are 'true' or 'false'"
                            );
                        }

                        break;

                    case "isMandatory":
                        if (!bool.TryParse(arguments[0], out isMandatory))
                        {
                            throw new ArgumentException(
                                $"Invalid value for isMandatory in '{line}' of knot '{knotID}'. "
                                + "Acceptable values are 'true' or 'false'"
                            );
                        }

                        break;

                    case "cooldown":
                        if (!int.TryParse(arguments[0], out cooldown))
                        {
                            throw new ArgumentException(
                                $"Invalid value for cooldown in '{line}' of knot '{knotID}'. "
                                + "Acceptable values are integers greater than or equal to zero."
                            );
                        }

                        break;

                    case "tags":
                        foreach (string tag in arguments)
                        {
                            tags.Add(tag);
                        }

                        break;

                    default:
                        throw new Exception(
                            $"Unrecognized command '{command}' in knot '{knotID}'");
                }

            }

            Storylet storylet = new Storylet(
                knotID,
                story,
                cooldown,
                isRepeatable,
                isMandatory,
                weight,
                tags,
                precondition,
                variableSubstitutions,
                firstLineTagOffset
            );

            return storylet;
        }

        private static DBQuery PreconditionQueryFromTags(string knotID, Queue<string> tagQueue)
        {
            List<string> queryLines = new List<string>();
            bool endReached = false;

            while (tagQueue.Count > 0)
            {
                string line = tagQueue.Dequeue().Trim();

                if (line.StartsWith("end"))
                {
                    endReached = true;
                    break;
                }

                queryLines.Add(line);
            }

            if (!endReached)
            {
                throw new ArgumentException(
                    $"Missing 'end >>' statement in precondition query for knot '{knotID}'"
                );
            }

            return new DBQuery(queryLines);
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

        #endregion
    }
}
