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
        /// Get a collection instances of this storylet.
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        // public IEnumerable<StoryletInstance> GetInstances(StoryDatabase db)
        // {
        //     return new StoryletInstance[0];
        // }

        /// <summary>
        /// Get a collection instances of this storylet.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="bindings"></param>
        /// <returns></returns>
        // public IEnumerable<StoryletInstance> GetInstances(StoryDatabase db, Dictionary<string, string> bindings)
        // {
        //     return new StoryletInstance[0];
        // }

    }
}
