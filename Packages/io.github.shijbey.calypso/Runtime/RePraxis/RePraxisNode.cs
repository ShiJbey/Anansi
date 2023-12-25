using System.Collections.Generic;

namespace Calypso.RePraxis
{
    /// <summary>
    /// A node within a RePraxis database.
    /// </summary>
    public class RePraxisNode : IRePraxisNode
    {
        #region Protected Fields

        /// <summary>
        /// A map of the node's children's symbols mapped to their instances
        /// </summary>
        private Dictionary<string, IRePraxisNode> _children;

        #endregion

        #region Public Properties

        public string Symbol { get; }
        public object Value { get; set; }
        public NodeCardinality Cardinality { get; set; }
        public IRePraxisNode Parent { get; set; }
        public IEnumerable<IRePraxisNode> Children => _children.Values;

        #endregion

        #region Constructors

        public RePraxisNode(string symbol, NodeCardinality cardinality, object value)
        {
            Symbol = symbol;
            Cardinality = cardinality;
            Value = value;
            _children = new Dictionary<string, IRePraxisNode>();
            Parent = null;
        }

        public RePraxisNode(string symbol, NodeCardinality cardinality)
        {
            Symbol = symbol;
            Cardinality = cardinality;
            Value = true;
            _children = new Dictionary<string, IRePraxisNode>();
            Parent = null;
        }

        #endregion

        #region Methods

        public void AddChild(IRePraxisNode node)
        {
            if (node.Cardinality == NodeCardinality.ONE && _children.Count >= 1)
            {
                throw new System.Exception(
                    "Cannot add additional child to node with cardinality ONE"
                );
            }

            _children.Add(node.Symbol, node);
            node.Parent = this;
        }

        public bool RemoveChild(string symbol)
        {
            if (_children.ContainsKey(symbol))
            {
                var child = _children[symbol];
                child.Parent = null;
                _children.Remove(symbol);
                return true;
            }
            return false;
        }

        public IRePraxisNode GetChild(string symbol)
        {
            return _children[symbol];
        }

        public bool HasChild(string symbol)
        {
            return _children.ContainsKey(symbol);
        }

        public void ClearChildren()
        {
            _children.Clear();
        }

        public string GetPath()
        {
            if (Parent == null || Parent.Symbol == "root")
            {
                return Symbol;
            }
            else
            {
                string parentCardinalityOp = Parent.Cardinality == NodeCardinality.ONE ? "!" : ".";
                return Parent.GetPath() + parentCardinalityOp + Symbol;
            }
        }

        #endregion
    }
}
