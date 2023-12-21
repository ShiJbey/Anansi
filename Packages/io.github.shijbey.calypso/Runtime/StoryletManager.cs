using System;
using System.Collections.Generic;
using System.Linq;
using Calypso.RePraxis;

namespace Calypso
{
    /// <summary>
    /// Manages a collection of Storylets.
    ///
    /// This class is modified from sample code by WildWinter.
    /// https://github.com/wildwinter/Ink-Storylet-Framework/
    /// </summary>
    public class StoryletManager
    {
        #region Private Attributes

        private enum State
        {
            NEEDS_REFRESH,
            REFRESHING,
            REFRESH_COMPLETE
        }
        private State _state = State.NEEDS_REFRESH;
        private List<Storylet> _refreshList = new();

        #endregion

        #region Protected Attributes

        /// <summary>
        /// All stories loaded into the manager.
        /// </summary>
        protected List<Ink.Runtime.Story> _stories;

        /// <summary>
        /// All storylets loaded from the loaded stories.
        /// </summary>
        protected List<Storylet> _storylets;

        #endregion

        #region Public Actions

        /// <summary>
        /// Action invoked when the manager has completed refreshing all the storylets
        /// </summary>
        public Action OnRefreshComplete;

        /// <summary>
        /// Event invoked when a new story is loaded into the manager
        /// </summary>
        public Action<Ink.Runtime.Story> OnStoryAdded;

        #endregion

        #region Public Properties

        public int StoryletsToProcessPerFrame { get; set; } = 5;
        public bool IsReady => _state == State.REFRESH_COMPLETE;
        public bool IsRefreshing => _state == State.REFRESHING;
        public bool NeedsRefresh => _state == State.NEEDS_REFRESH;

        #endregion

        #region Constructors

        public StoryletManager()
        {
            _stories = new List<Ink.Runtime.Story>();
            _storylets = new List<Storylet>();
        }

        #endregion

        public IEnumerable<Storylet> GetStorylets()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Storylet> GetAllStorylets()
        {
            throw new NotImplementedException();
        }

        public void DecrementCooldowns()
        {
            throw new NotImplementedException();
        }

        public void AddStorylets(Ink.Runtime.Story story, string storyletIDPrefix)
        {
            if (!_stories.Contains(story))
            {
                _stories.Add(story);
                if (OnStoryAdded != null)
                {
                    OnStoryAdded.Invoke(story);
                }
            }

            List<string> knotIDs = GetAllKnotIDs(story);

            foreach (string knotID in knotIDs)
            {
                if (knotID.StartsWith(storyletIDPrefix))
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
                            storylet.Cooldown = int.Parse(parts[1]);
                        }

                        if (parts[0] == "repeatable")
                        {
                            storylet.IsRepeatable = bool.Parse(parts[1]);
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

            _state = State.NEEDS_REFRESH;

        }

        // Call with a prefix e.g. "story_" will scan all the
        // knot IDs in the ink file that start with story_ and treat
        // them as a story.
        //
        // Remember each storylet must also have a function called
        // the same but with an underscore in front e.g.
        // a story called story_troll_attack needs a function called
        // _story_troll_attack()
        // The function must either return true/false ("is this available?")
        // or instead can return an integer weighting - the higher the integer,
        // the more changes that card gets of being picked randomly. (i.e. the more
        // copies of that card ends up in the current hand of cards!)
        //
        // If a knot has the tag #once
        // then it will be discarded after
        // use, otherwise each storylet will
        // be shuffled back in.
        //
        // IMPORTANT: Once you have called all the AddStorylets you need to,
        // make sure you call Refresh()!
        public void AddStorylets(string prefix)
        {

        }

        // Start a refresh process. You won't be
        // able to call GetPlayableStorylets() or
        // do anything else until
        // OnRefreshComplete is called, or
        // IsReady is true.
        // You will always need to call this at
        // the start of the game once you have called
        // AddStorylets()
        public void Refresh()
        {
            _refreshList = new List<Storylet>(_storylets);
            _state = State.REFRESHING;
        }

        // IMPORTANT Must be called every frame to make
        // sure refreshing of cards actually works.
        // Calling it in an Update() somewhere is usually good.
        public void Tick()
        {
            if (_state != State.REFRESHING)
                return;

            if (_refreshList.Count > 0)
            {
                int refreshCount = Math.Min(StoryletsToProcessPerFrame, _refreshList.Count);
                for (int i = 0; i < refreshCount; i++)
                {
                    Storylet storylet = _refreshList[0];
                    _refreshList.RemoveAt(0);

                    int weighting = GetWeighting(storylet);
                    if (weighting == 0)
                        continue;

                    // _hand.Add(storylet.knotID);

                    // for (int j = 0; j < weighting; j++)
                    //     _handWeighted.Add(storylet.knotID);
                }
            }

            if (_refreshList.Count == 0)
            {
                _state = State.REFRESH_COMPLETE;
                if (OnRefreshComplete != null)
                {
                    OnRefreshComplete.Invoke();
                }
            }
        }

        // Returns a list of knotIDs that are currently available,
        // assuming all the functions have been tested.
        // If weighted is true, returns multiple copies of anything which
        // has a weighting>1
        //
        // Once you or the player has picked a storylet from list list,
        // make sure you called MarkPlayed(knotID) on the storylet!
        public List<string> GetPlayableStorylets(bool weighted = false)
        {
            if (_state != State.REFRESH_COMPLETE)
            {
                // Debug.LogError("Don't call GetPlayableStorylets until refresh is complete!");
                return null;
            }

            return new List<string>();

            // if (!weighted)
            //     return _hand;

            // return _handWeighted;
        }

        // Call this if you use a storylet from the playable list
        // returned by GetPlayableStorylets
        public void MarkPlayed(string knotID)
        {
            // _deck[knotID].played = true;
        }

        // Gives you a random storylet from the currently
        // playable selection (hand of cards).
        // Automatically marks it as played.
        public string PickPlayableStorylet()
        {
            if (_state != State.REFRESH_COMPLETE)
            {
                // Debug.LogError("Don't call PickPlayableStorylet until refresh is complete!");
                return null;
            }

            // if (_handWeighted.Count == 0)
            //     return null;

            // int i = UnityEngine.Random.Range(0, _handWeighted.Count);
            // string knotID = _handWeighted[i];
            // MarkPlayed(knotID);
            // return knotID;

            return "";
        }

        // Throw out any played data and start from scratch.
        // Bear in mind you might have to reset your ink story
        // as well to reset any ink variables!
        public void Reset()
        {
            foreach (Storylet storylet in _storylets)
            {
                storylet.ResetCooldown();
            }

            _refreshList.Clear();

            _state = State.NEEDS_REFRESH;
        }

        private int GetWeighting(Storylet storylet)
        {
            // if (storylet.played && storylet.once)
            //     return 0;

            // object retVal = _story.EvaluateFunction("_" + storylet.knotID);
            // if (retVal is bool playable)
            //     return playable ? 1 : 0;

            // if (retVal is int i)
            //     return i;

            // throw new
            // Debug.LogError($"Wrong value returned from storylet function _{storylet.knotID} - should be bool or int!");
            return 0;
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
