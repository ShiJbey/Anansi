using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Calypso.RePraxis
{
    /// <summary>
    /// Returned by DBQueries indicating if they passed/failed
    /// </summary>
    public class QueryResult
    {
        #region Fields
        private bool _pass;
        private Dictionary<string, string>[] _bindings;
        #endregion

        #region Properties
        /// <summary>
        /// Did all statements in the query pass or evaluate to true
        /// </summary>
        public bool Success { get { return _pass; } set { _pass = value; } }

        /// <summary>
        /// Bindings for any variables present in the query
        /// </summary>
        public Dictionary<string, string>[] Bindings
        {
            get { return _bindings; }
        }
        #endregion

        #region Constructors
        public QueryResult(bool pass, IEnumerable<Dictionary<string, string>> bindings)
        {
            _pass = pass;
            _bindings = bindings.ToArray();
        }

        public QueryResult() : this(true, new Dictionary<string, string>[0]) { }
        #endregion

        #region Methods
        /// <summary>
        /// Set the new bindings value
        /// </summary>
        /// <param name="bindings"></param>
        public void UpdateBindings([NotNull] Dictionary<string, string>[] bindings)
        {
            _bindings = bindings;
        }

        /// <summary>
        /// Reset the bindings to an empty array
        /// </summary>
        public void ClearBindings()
        {
            _bindings = new Dictionary<string, string>[0];
        }
        #endregion
    }

    /// <summary>
    /// Used to construct queries against a story database
    ///
    /// <para>
    /// This class is immutable. So, additional calls to the Where method produce
    /// new DBQuery instances.
    /// </para>
    /// </summary>
    public class DBQuery
    {
        #region Fields
        /// <summary>
        /// 'Where' expressions contained within this query
        /// </summary>
        private string[] _expressions;
        #endregion

        #region Constructors
        public DBQuery(IEnumerable<string> expressions)
        {
            _expressions = expressions.ToArray();
        }

        public DBQuery()
        {
            _expressions = new string[0];
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds a new expression to the query
        /// </summary>
        /// <param name="expression"></param>
        /// <returns>
        /// A new DBQuery instance containing the provided expression and
        /// all expressions from the calling query.
        /// </returns>
        public DBQuery Where(string expression)
        {
            return new DBQuery(_expressions.ToList().Append(expression));
        }

        /// <summary>
        /// Run the query on the provided database
        /// </summary>
        /// <param name="db"></param>
        /// <returns>
        /// A <c>QueryResult</c> object with the final result of the query. If
        /// result.Success is true, then everything passes. Also, if there were
        /// any variables within the query, it returns all valid bindings for those
        /// variables as an array of dictionaries.
        /// </returns>
        public QueryResult Run(RePraxisDatabase db)
        {
            var combined_expressions = string.Join("\n", _expressions);
            var input = new AntlrInputStream(combined_expressions);
            var lexer = new RePraxisLexer(input);
            var tokens = new CommonTokenStream(lexer);
            var parser = new RePraxisParser(tokens);
            var parseTree = parser.prog();
            var queryParser = new QueryParser(db);
            queryParser.Visit(parseTree);
            return queryParser.Result;
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Creates a new sentence by binding variables to entries within the bindings.
        /// </summary>
        /// <param name="sentence"></param>
        /// <param name="bindings"></param>
        /// <returns></returns>
        public static string BindSentence(string sentence, Dictionary<string, string> bindings)
        {
            var tokens = RePraxisDatabase.ParseSentence(sentence);
            var finalSentence = "";

            for (int i = 0; i < tokens.Length; ++i)
            {
                var token = tokens[i];

                if (token.type == RePraxisTokenType.VARIABLE)
                {
                    finalSentence += bindings[token.symbol];
                }
                else
                {
                    finalSentence += token.symbol;
                }

                if (i < tokens.Length - 1)
                {
                    finalSentence += token.cardinality == NodeCardinality.ONE ? "!" : ".";
                }
            }

            return finalSentence;
        }
        #endregion
    }

    /// <summary>
    /// Used internally to track bindings for a single sentence and the database.
    /// </summary>
    struct BindingContext
    {
        #region Fields
        private Dictionary<string, string> _binding;
        private IRePraxisNode _subtree;
        #endregion

        #region Properties
        public Dictionary<string, string> Binding { get { return _binding; } }
        public IRePraxisNode SubTree { get { return _subtree; } }
        #endregion

        #region Constructors
        public BindingContext(Dictionary<string, string> binding, IRePraxisNode subtree)
        {
            _binding = binding;
            _subtree = subtree;
        }

        public BindingContext(IRePraxisNode subtree) : this(new Dictionary<string, string>(), subtree) { }
        #endregion
    }

    /// <summary>
    /// Evaluates a parse tree of a query and produces the final query result
    /// </summary>
    class QueryParser : RePraxisBaseVisitor<QueryResult>
    {
        #region Fields
        private RePraxisDatabase _db;
        private QueryResult _result;
        #endregion

        #region Properties
        public QueryResult Result { get { return _result; } }
        #endregion

        #region Constructors
        public QueryParser(RePraxisDatabase db)
        {
            _db = db;
            _result = new QueryResult();
        }
        #endregion

        #region Visitor Method Overrides
        public override QueryResult VisitAssertionExpr([NotNull] RePraxisParser.AssertionExprContext context)
        {
            var sentence = context.sentence().GetText();

            // Exit early if we already know the query has failed
            if (_result.Success == false) return _result;

            // Generate new bindings for any variables in the sentence
            var bindings = UnifyAll(new string[] { sentence });

            // Store if the sentence has variables
            var sentenceHasVariables = HasVariables(sentence);

            // If the sentence has variables and no bindings were returned
            // then this query has failed
            if (bindings.Count() == 0 && sentenceHasVariables == true)
            {
                _result.Success = false;
                _result.ClearBindings();
                return _result;
            }

            // If the sentence does not have variables then just check
            // if the value in the database is Truthy
            if (bindings.Count() == 0 && sentenceHasVariables == false)
            {
                _result.Success = Convert.ToBoolean(_db[sentence]);
                if (_result.Success == false)
                {
                    _result.ClearBindings();
                }
                return _result;
            }

            // If any bindings were returned filter them for ones whose
            // bound sentences are mapped to Truthy values
            if (bindings.Count() > 0)
            {
                var filteredBindings = bindings
                    .Where((binding) =>
                    {
                        return Convert.ToBoolean(_db[DBQuery.BindSentence(sentence, binding)]);
                    });

                if (filteredBindings.Count() == 0)
                {
                    _result.Success = false;
                    _result.ClearBindings();
                }
                else
                {
                    _result.UpdateBindings(filteredBindings.ToArray());
                }

                return _result;
            }

            return _result;
        }

        public override QueryResult VisitRelationalExpr([NotNull] RePraxisParser.RelationalExprContext context)
        {
            var sentence = context.sentence(0).GetText();
            var op = context.RELATIONAL_SYMBOL().GetText();

            if (_result.Success == false) return _result;


            if (context.constant() != null)
            {
                // We are comparing against a constant

                var value = ParseConstant(context.constant());

                var bindings = UnifyAll(new string[] { sentence });

                if (bindings.Count() == 0 && HasVariables(sentence))
                {
                    _result.Success = false;
                    _result.ClearBindings();
                }
                else if (bindings.Count() == 0 && HasVariables(sentence) == false)
                {
                    _result.Success = EvaluateRelation(_db[sentence], op, value);
                    if (_result.Success == false)
                    {
                        _result.ClearBindings();
                    }
                }
                else if (bindings.Count() > 0)
                {
                    var filteredBindings = bindings
                        .Where((binding) =>
                        {
                            return EvaluateRelation(_db[DBQuery.BindSentence(sentence, binding)], op, value);
                        });

                    if (filteredBindings.Count() == 0)
                    {
                        _result.Success = false;
                        _result.ClearBindings();
                    }
                    else
                    {
                        _result.UpdateBindings(filteredBindings.ToArray());
                    }
                }
                else
                {
                    _result.Success = false;
                    _result.ClearBindings();
                }
            }
            else
            {
                // We are comparing against the value at another sentence
                var other_sentence = context.sentence(1).GetText();

                var bindings = UnifyAll(new string[] { sentence, other_sentence });

                if (bindings.Count() == 0 && (HasVariables(sentence) || HasVariables(other_sentence)))
                {
                    _result.Success = false;
                    _result.ClearBindings();
                }
                else if (bindings.Count() == 0 && HasVariables(sentence) == false && HasVariables(other_sentence) == false)
                {
                    _result.Success = EvaluateRelation(_db[sentence], op, _db[other_sentence]);
                    if (_result.Success == false)
                    {
                        _result.ClearBindings();
                    }
                }
                else if (bindings.Count() > 0)
                {
                    var filteredBindings = bindings
                        .Where((binding) =>
                        {
                            return EvaluateRelation(
                                _db[DBQuery.BindSentence(sentence, binding)],
                                op,
                                _db[DBQuery.BindSentence(other_sentence, binding)]);
                        });

                    if (filteredBindings.Count() == 0)
                    {
                        _result.Success = false;
                        _result.ClearBindings();
                    }
                    else
                    {
                        _result.UpdateBindings(filteredBindings.ToArray());
                    }
                }
                else
                {
                    _result.Success = false;
                    _result.ClearBindings();
                }
            }

            return _result;
        }
        #endregion

        #region Helper Methods

        public object ParseConstant([NotNull] RePraxisParser.ConstantContext context)
        {
            if (context.NULL() != null)
            {
                return null;
            }
            else if (context.BOOL() != null)
            {
                return bool.Parse(context.BOOL().GetText());
            }
            else if (context.STRING() != null)
            {
                return context.STRING().GetText();
            }
            else if (context.FLOAT() != null)
            {
                return float.Parse(context.FLOAT().GetText());
            }
            else if (context.INT() != null)
            {
                return int.Parse(context.INT().GetText());
            }
            else
            {
                throw new Exception($"Unknown datatype in expression {context.GetText()}");
            }
        }

        private static bool EvaluateRelation(object lhValue, string op, object rhValue)
        {
            if (rhValue == null)
            {
                switch (op)
                {
                    case "==":
                        return lhValue == null;
                    case "!=":
                        return lhValue != null;
                    default:
                        throw new Exception("Only == and != operators may be used with null value.");
                }
            }
            else if (rhValue is bool)
            {
                switch (op)
                {
                    case "==":
                        return Convert.ToBoolean(lhValue) == Convert.ToBoolean(rhValue);
                    case "!=":
                        return Convert.ToBoolean(lhValue) != Convert.ToBoolean(rhValue);
                    default:
                        throw new Exception("Only == and != operators may be used with bool value");
                }
            }
            else if (rhValue is string)
            {
                switch (op)
                {
                    case "==":
                        return Convert.ToString(lhValue) == Convert.ToString(rhValue);
                    case "!=":
                        return Convert.ToString(lhValue) != Convert.ToString(rhValue);
                    default:
                        throw new Exception($"Unknown relational operator {op} for string value");
                }
            }
            else if (rhValue is float || rhValue is double)
            {
                switch (op)
                {
                    case "<":
                        return Convert.ToDouble(lhValue) < Convert.ToDouble(rhValue);
                    case ">":
                        return Convert.ToDouble(lhValue) > Convert.ToDouble(rhValue);
                    case "<=":
                        return Convert.ToDouble(lhValue) <= Convert.ToDouble(rhValue);
                    case ">=":
                        return Convert.ToDouble(lhValue) >= Convert.ToDouble(rhValue);
                    case "==":
                        return Convert.ToDouble(lhValue) == Convert.ToDouble(rhValue);
                    case "!=":
                        return Convert.ToDouble(lhValue) != Convert.ToDouble(rhValue);
                    default:
                        throw new Exception($"Unknown relational operator: {op}");
                }
            }
            else if (rhValue is int)
            {
                switch (op)
                {
                    case "<":
                        return Convert.ToInt64(lhValue) < Convert.ToInt64(rhValue);
                    case ">":
                        return Convert.ToInt64(lhValue) > Convert.ToInt64(rhValue);
                    case "<=":
                        return Convert.ToInt64(lhValue) <= Convert.ToInt64(rhValue);
                    case ">=":
                        return Convert.ToInt64(lhValue) >= Convert.ToInt64(rhValue);
                    case "==":
                        return Convert.ToInt64(lhValue) == Convert.ToInt64(rhValue);
                    case "!=":
                        return Convert.ToInt64(lhValue) != Convert.ToInt64(rhValue);
                    default:
                        throw new Exception($"Unknown relational operator: {op}");
                }
            }
            else
            {
                throw new Exception($"Unknown datatype in expression {rhValue}");
            }
        }

        private static bool HasVariables(string sentence)
        {
            return RePraxisDatabase.ParseSentence(sentence)
                .Where(t => t.type == RePraxisTokenType.VARIABLE).Count() > 0;
        }
        #endregion

        #region Unification Methods
        /// <summary>
        /// Generates potential bindings from the database for a single sentence
        ///
        /// This method does not take the current bindings into consideration. It
        /// should only be called by the UnifyAll method
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        private List<Dictionary<string, string>> Unify(string sentence)
        {
            List<BindingContext> unified = new List<BindingContext>
            {
                new BindingContext(_db.Root)
            };

            var tokens = RePraxisDatabase.ParseSentence(sentence);

            foreach (var token in tokens)
            {
                List<BindingContext> nextUnified = new List<BindingContext>();

                foreach (var entry in unified)
                {
                    foreach (var child in entry.SubTree.Children)
                    {
                        if (token.type == RePraxisTokenType.VARIABLE)
                        {
                            var unification =
                                new BindingContext(
                                    new Dictionary<string, string>(entry.Binding), child);

                            unification.Binding[token.symbol] = child.Symbol;

                            nextUnified.Add(unification);
                        }
                        else
                        {
                            if (token.symbol == child.Symbol)
                            {
                                nextUnified.Add(new BindingContext(entry.Binding, child));
                            }
                        }
                    }
                }

                unified = nextUnified;
            }

            return unified
                .Select(unification => unification.Binding)
                .Where(binding => binding.Count() > 0)
                .ToList();
        }

        /// <summary>
        /// Generates potential bindings from the database unifying across all sentences.
        ///
        /// This method takes into consideration the bindings from the current results.
        /// </summary>
        /// <param name="sentences"></param>
        /// <returns></returns>
        private List<Dictionary<string, string>> UnifyAll(string[] sentences)
        {
            var possibleBindings = _result.Bindings.ToList();

            foreach (var key in sentences)
            {
                var iterativeBindings = new List<Dictionary<string, string>>();
                var newBindings = Unify(key);

                if (possibleBindings.Count == 0)
                {
                    foreach (var binding in newBindings)
                    {
                        var nextUnification = new Dictionary<string, string>();

                        foreach (var k in binding.Keys)
                        {
                            nextUnification[k] = binding[k];
                        }

                        iterativeBindings.Add(nextUnification);
                    }
                }
                else
                {
                    foreach (var oldBinding in possibleBindings)
                    {
                        foreach (var binding in newBindings)
                        {
                            var newKeys = binding.Keys.Where(k => !oldBinding.ContainsKey(k));
                            var oldKeys = binding.Keys.Where(k => oldBinding.ContainsKey(k));
                            bool existsIncompatibleKey = oldKeys.Any(k => oldBinding[k] != binding[k]);

                            if (existsIncompatibleKey)
                            {
                                continue;
                            }
                            else
                            {
                                var nextUnification = new Dictionary<string, string>(oldBinding);

                                foreach (var k in newKeys)
                                {
                                    nextUnification[k] = binding[k];
                                }

                                iterativeBindings.Add(nextUnification);
                            }
                        }
                    }
                }

                possibleBindings = iterativeBindings;
            }

            return possibleBindings.Where(bindings => bindings.Count() > 0).ToList();
        }

        #endregion
    }
}
