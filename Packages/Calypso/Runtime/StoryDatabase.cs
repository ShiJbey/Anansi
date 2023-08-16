using System;
using System.Collections.Generic;
using System.Linq;

namespace Calypso
{
    /// <summary>
    /// Each token within a database sentence is assigned to a node within the database.
    /// </summary>
    public class DBNode
    {
        private object _value;
        private string _symbol;
        private bool _isExclusive;
        private Dictionary<string, DBNode> _children;

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

        public DBNode(string symbol, object value, bool isExclusive)
        {
            _value = value;
            _symbol = symbol;
            _isExclusive = isExclusive;
            _children = new Dictionary<string, DBNode>();
        }

        /// <summary>
        /// Add a new child node to the current node
        /// </summary>
        /// <param name="term"></param>
        public void AddChild(DBNode term)
        {
            _children[term.Symbol] = term;
        }

        public bool RemoveChild(string symbol)
        {
            return _children.Remove(symbol);
        }

        public DBNode GetChild(string symbol)
        {
            return _children[symbol];
        }

        public bool Contains(string symbol)
        {
            return _children.ContainsKey(symbol);
        }

    }

    public enum TokenType
    {
        Variable = 0,
        Symbol = 1,
    }

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

    public class StoryDatabase
    {
        private DBNode _db;

        public DBNode Root { get { return _db; } }

        public StoryDatabase()
        {
            _db = new DBNode("root", true, false);
        }

        public void Add(string sentence, object value)
        {
            var tokens = ParseSentence(sentence);

            var currentNode = _db;

            foreach (var token in tokens)
            {
                if (token.type == TokenType.Variable)
                {
                    throw new Exception("Cannot add value when sentence contains variables.");
                }

                if (!currentNode.Contains(token.symbol) || token.isExclusive)
                {
                    var node = new DBNode(token.symbol, true, token.isExclusive);
                    currentNode.AddChild(node);
                }

                currentNode = currentNode.GetChild(token.symbol);
            }

            currentNode.Value = value;
        }

        public object Get(string sentence)
        {
            var tokens = ParseSentence(sentence);

            var currentNode = _db;

            foreach (var token in tokens)
            {
                if (token.type == TokenType.Variable)
                {
                    throw new Exception("Sentence cannot contain variables when retreiving a value.");
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

        public object this[string sentence]
        {
            get { return Get(sentence); }
            set { Add(sentence, value); }
        }

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

            return currentNode.RemoveChild(last.symbol);
        }

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
    }
}

