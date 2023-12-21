using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Calypso.RePraxis
{
    /// <summary>
    /// Each token within a database sentence is assigned to a node
    /// within the database.
    /// </summary>
    public class DBNode
    {
        #region Fields
        private object _value;
        private string _symbol;
        private bool _isExclusive;
        private DBNode _parent;
        private Dictionary<string, DBNode> _children;
        #endregion

        #region Properties
        /// <summary>
        /// The string symbol associated with this node
        /// </summary>
        public string Symbol { get { return _symbol; } }

        /// <summary>
        /// An untyped value associated with this node
        /// </summary>
        public object Value { get { return _value; } set { _value = value; } }

        /// <summary>
        /// Returns true if this node may have only a single child
        /// </summary>
        public bool IsExclusive { get { return _isExclusive; } }

        /// <summary>
        /// All the child nodes that belong to this node
        /// </summary>
        public List<DBNode> Children
        {
            get
            {
                return _children.Values.ToList();
            }
        }

        public string Path
        {
            get
            {
                string path = $"{Symbol}";

                var parentNode = _parent;

                while (parentNode != null)
                {
                    // The current node is the root node and should be skipped.
                    if (parentNode._parent == null)
                    {
                        break;
                    }
                    path = $"{parentNode.Symbol}{(parentNode.IsExclusive ? "!" : ".")}" + path;
                    parentNode = parentNode._parent;
                }

                return path;
            }
        }
        #endregion

        #region Constructors
        public DBNode(string symbol, object value, bool isExclusive)
        {
            _value = value;
            _symbol = symbol;
            _isExclusive = isExclusive;
            _children = new Dictionary<string, DBNode>();
            _parent = null;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Add a child node to the DBNode
        /// </summary>
        /// <param name="term"></param>
        public void AddChild(DBNode term)
        {
            _children[term.Symbol] = term;
            term._parent = this;
        }

        /// <summary>
        /// Removes a child node from the DBNode
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns>True if successful</returns>
        public bool RemoveChild(string symbol)
        {
            if (_children.ContainsKey(symbol))
            {
                var child = _children[symbol];
                child._parent = null;
                _children.Remove(symbol);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get a child node
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns>The node with the given symbol</returns>
        public DBNode GetChild(string symbol)
        {
            return _children[symbol];
        }

        /// <summary>
        /// Check if the node has a child
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns>
        /// True if a child is present with the given symbol.
        /// False otherwise.
        /// </returns>
        public bool Contains(string symbol)
        {
            return _children.ContainsKey(symbol);
        }
        #endregion
    }

    /// <summary>
    /// Tokens in a database sentence can be symbols or variables
    /// if they start with a question mark.
    /// </summary>
    public enum TokenType
    {
        Variable = 0,
        Symbol = 1,
    }

    /// <summary>
    /// A single token from a database sentence.
    /// <para>
    /// These are used to traverse the database when adding
    /// and removing data
    /// </para>
    /// </summary>
    public readonly struct SentenceToken
    {
        public readonly string symbol;
        public readonly bool isExclusive;
        public readonly TokenType type;

        public SentenceToken(string symbol, bool isExclusive, TokenType type)
        {
            this.symbol = symbol;
            this.isExclusive = isExclusive;
            this.type = type;
        }
    }

    /// <summary>
    /// Delegate definition for sentence observation
    /// </summary>
    /// <param name="sentence"></param>
    /// <param name="newValue"></param>
    public delegate void SentenceObserver(string sentence, object newValue);

    /// <summary>
    /// A database that operates like a Dictionary and enables
    /// users to query for information using logical queries.
    ///
    /// <para>
    /// This class is inspired by the Praxis logic language created by
    /// Emily Short and Richard Evans for the Versu interactive fiction
    /// engine. This particular implementation is adapted from a
    /// rational reconstruction of Praxis called Wyclef by James Dameris.
    /// </para>
    /// <list type="bullet">
    /// <item><see href="https://versu.com/"/></item>
    /// <item><see href="https://github.com/JamesDameris/Wyclef"/></item>
    /// </list>
    /// </summary>
    public class RePraxisDatabase
    {
        #region Fields
        private DBNode _db;
        private static Dictionary<string, SentenceObserver> _sentenceObservers
            = new Dictionary<string, SentenceObserver>();
        #endregion

        #region Events and Actions
        public static Action<string, object> OnEntryAdded;
        public static Action<string> OnEntryRemoved;
        #endregion

        #region Properties
        public DBNode Root { get { return _db; } }
        #endregion

        #region Constructors
        public RePraxisDatabase()
        {
            _db = new DBNode("root", true, false);
        }
        #endregion

        #region Accessor Methods
        /// <summary>
        /// Add a value to the database under the given sentence.
        /// </summary>
        /// <param name="sentence"></param>
        /// <param name="value"></param>
        /// <exception cref="ArgumentException">
        /// Thrown when the sentence contains variables.
        /// </exception>
        public void Add(string sentence, object value)
        {
            var tokens = ParseSentence(sentence);

            var currentNode = _db;

            foreach (var token in tokens)
            {
                if (token.type == TokenType.Variable)
                {
                    throw new ArgumentException("Cannot add value when sentence contains variables.");
                }

                if (!currentNode.Contains(token.symbol) || token.isExclusive)
                {
                    var node = new DBNode(token.symbol, true, token.isExclusive);
                    currentNode.AddChild(node);
                }

                currentNode = currentNode.GetChild(token.symbol);
            }

            currentNode.Value = value;

            OnEntryAdded?.Invoke(sentence, value);

            if (_sentenceObservers.ContainsKey(sentence))
            {
                _sentenceObservers[sentence]?.Invoke(sentence, value);
            }
        }

        /// <summary>
        /// Retrieve a value from the database.
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns>
        /// The object at the given sentence or False if none found.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when a sentence contains variables.
        /// </exception>
        public object Get(string sentence)
        {
            var tokens = ParseSentence(sentence);

            var currentNode = _db;

            foreach (var token in tokens)
            {
                if (token.type == TokenType.Variable)
                {
                    throw new ArgumentException("Sentence cannot contain variables when retrieving a value.");
                }

                if (!currentNode.Contains(token.symbol))
                {
                    return false;
                }

                if (currentNode.IsExclusive != token.isExclusive)
                {
                    return false;
                }

                currentNode = currentNode.GetChild(token.symbol);
            }

            return currentNode.Value;
        }

        /// <summary>
        /// Overrides the index operator to provide a Dictionary-like
        /// interface.
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns>
        /// The object at the given sentence or False if none found.
        /// </returns>
        public object this[string sentence]
        {
            get { return Get(sentence); }
            set { Add(sentence, value); }
        }

        /// <summary>
        /// Remove all values and sub-trees under the given sentence
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns>
        /// True if something was removed. False otherwise.
        /// </returns>
        public bool Remove(string sentence)
        {
            var tokens = ParseSentence(sentence);

            var currentNode = _db;

            for (int i = 0; i < tokens.Length - 1; ++i)
            {
                var token = tokens[i];
                currentNode = currentNode.GetChild(token.symbol);
            }

            var last = tokens[tokens.Length - 1];

            OnEntryRemoved?.Invoke(sentence);

            return currentNode.RemoveChild(last.symbol);
        }
        #endregion

        #region Observer Methods
        /// <summary>
        /// When the given sentence changes its value in the database, all the
        /// observers are notified of the change.
        /// <para>
        /// This method was adapted from from the Ink C# runtime and how it manages
        /// variable observation.
        /// </para>
        /// <para>
        /// NOTE:: Observers will not be notified if a sentence has been removed from the
        /// database.
        /// </para>
        /// </summary>
        /// <param name="sentence"></param>
        /// <param name="observer"></param>
        public static void ObserveSentence(string sentence, SentenceObserver observer)
        {
            if (_sentenceObservers.ContainsKey(sentence))
            {
                _sentenceObservers[sentence] += observer;
            }
            else
            {
                _sentenceObservers[sentence] = observer;
            }
        }

        /// <summary>
        /// Removes an observer from getting sentence value updates or removes
        /// all observers from a given sentence.
        /// </summary>
        /// <param name="observer"></param>
        /// <param name="sentence"></param>
        public static void RemoveSentenceObserver(
            SentenceObserver observer = null,
            string sentence = null)
        {
            // Remove the observer for a specific sentence or remove all
            // listeners for a given sentence
            if (sentence != null)
            {
                if (_sentenceObservers.ContainsKey(sentence))
                {
                    if (observer != null)
                    {
                        _sentenceObservers[sentence] -= observer;
                        if (_sentenceObservers[sentence] == null)
                        {
                            _sentenceObservers.Remove(sentence);
                        }
                    }
                    else
                    {
                        _sentenceObservers.Remove(sentence);
                    }
                }
            }

            // Remove this observer for all sentences
            else if (observer != null)
            {
                var keys = new List<string>(_sentenceObservers.Keys);
                foreach (var entry in keys)
                {
                    _sentenceObservers[entry] -= observer;

                }
            }
        }
        #endregion

        #region Helper Methods
        public static SentenceToken[] ParseSentence(string sentence)
        {
            string currentSymbol = "";
            var tokens = new List<SentenceToken>();

            foreach (char ch in sentence)
            {
                if (ch == '!' || ch == '.')
                {
                    tokens.Add(
                        new SentenceToken(
                            currentSymbol,
                            ch == '!',
                            Char.ToLower(currentSymbol[0]) == '?' ? TokenType.Variable : TokenType.Symbol
                        )
                    );
                    currentSymbol = "";
                }
                else
                {
                    currentSymbol += ch;
                }
            }

            tokens.Add(
                new SentenceToken(
                    currentSymbol,
                    false,
                    Char.ToLower(currentSymbol[0]) == '?' ? TokenType.Variable : TokenType.Symbol
                )
            );

            return tokens.ToArray();
        }
        #endregion
    }
}
