using System;
using System.Collections.Generic;

namespace Calypso.RePraxis
{
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
        #region Properties

        /// <summary>
        /// A reference to the root node of the database
        /// </summary>
        public IRePraxisNode Root { get; private set; }

        #endregion

        #region Constructors

        public RePraxisDatabase()
        {
            Root = new RePraxisNode("root", NodeCardinality.MANY);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Add a value to the database under the given sentence.
        /// </summary>
        /// <param name="sentence"></param>
        /// <exception cref="ArgumentException">
        /// Thrown when the sentence contains variables.
        /// </exception>
        public void Insert(string sentence)
        {
            Insert(sentence, true);
        }

        /// <summary>
        /// Add a value to the database under the given sentence.
        /// </summary>
        /// <param name="sentence"></param>
        /// <param name="value"></param>
        /// <exception cref="ArgumentException">
        /// Thrown when the sentence contains variables.
        /// </exception>
        public void Insert(string sentence, object value)
        {
            SentenceToken[] tokens = ParseSentence(sentence);

            IRePraxisNode subtree = Root;

            for (int i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i];

                if (token.type == RePraxisTokenType.VARIABLE)
                {
                    throw new ArgumentException(@$"
                        Found variable {token.symbol} in sentence.
                        Sentence cannot contain variables when inserting a value."
                    );
                }

                IRePraxisNode currentNode;

                if (!subtree.HasChild(token.symbol))
                {
                    // We need to create a new node and continue.
                    currentNode = new RePraxisNode(token.symbol, token.cardinality);

                    subtree.AddChild(currentNode);
                }
                else
                {
                    // We need to get the existing node, check cardinalities, and establish new
                    // nodes
                    currentNode = subtree.GetChild(token.symbol);

                    if (currentNode.Cardinality == NodeCardinality.ONE && token.cardinality == NodeCardinality.MANY)
                    {
                        currentNode.Cardinality = NodeCardinality.MANY;

                    }
                    else if (currentNode.Cardinality == NodeCardinality.MANY && token.cardinality == NodeCardinality.ONE)
                    {
                        currentNode.Cardinality = NodeCardinality.ONE;
                        subtree.ClearChildren();
                    }
                }

                if (i == tokens.Length - 1)
                {
                    subtree.GetChild(token.symbol).Value = value;
                }

                subtree = currentNode;
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

            var currentNode = Root;

            foreach (var token in tokens)
            {
                if (token.type == RePraxisTokenType.VARIABLE)
                {
                    throw new ArgumentException(@$"
                        Found variable {token.symbol} in sentence.
                        Sentence cannot contain variables when retrieving a value."
                    );
                }

                if (!currentNode.HasChild(token.symbol))
                {
                    return false;
                }

                if (currentNode.Cardinality != token.cardinality)
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
            set { Insert(sentence, value); }
        }

        /// <summary>
        /// Remove all values and sub-trees under the given sentence
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns>
        /// True if something was removed. False otherwise.
        /// </returns>
        public bool Delete(string sentence)
        {

            SentenceToken[] tokens = ParseSentence(sentence);

            IRePraxisNode currentNode = Root;

            // Loop until we get to the second to last node
            for (int i = 0; i < tokens.Length - 1; ++i)
            {
                var token = tokens[i];
                currentNode = currentNode.GetChild(token.symbol);
            }

            // Get a reference to the final node in the sentence
            var lastToken = tokens[tokens.Length - 1];

            // Remove the child
            return currentNode.RemoveChild(lastToken.symbol);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Breakup a database sentence into component parts for use when looking though
        /// a database.
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns>The tokens of a database sentence</returns>
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
                            ch == '!' ? NodeCardinality.ONE : NodeCardinality.MANY,
                            currentSymbol[0] == '?'
                                ? RePraxisTokenType.VARIABLE : RePraxisTokenType.SYMBOL
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
                    NodeCardinality.MANY,
                    currentSymbol[0] == '?'
                        ? RePraxisTokenType.VARIABLE : RePraxisTokenType.SYMBOL
                )
            );

            return tokens.ToArray();
        }

        #endregion

        #region Helper Types

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
            public readonly NodeCardinality cardinality;
            public readonly RePraxisTokenType type;

            public SentenceToken(string symbol, NodeCardinality cardinality, RePraxisTokenType type)
            {
                this.symbol = symbol;
                this.cardinality = cardinality;
                this.type = type;
            }
        }

        #endregion
    }
}
