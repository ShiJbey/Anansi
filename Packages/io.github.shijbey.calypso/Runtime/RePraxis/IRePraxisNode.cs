using System.Collections.Generic;

namespace Calypso.RePraxis
{
    /// <summary>
    /// An interface implemented by all nodes within a RePraxis Database
    /// </summary>
    public interface IRePraxisNode
    {
        /// <summary>
        /// The symbol associated with this node.
        /// </summary>
        public string Symbol { get; }

        /// <summary>
        /// A value associated with this node.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// The number of children this character is allowed to have
        /// </summary>
        public NodeCardinality Cardinality { get; set; }

        /// <summary>
        /// A reference to the node's parent node.
        /// </summary>
        public IRePraxisNode Parent { get; set; }

        /// <summary>
        /// Get the children of this node.
        /// </summary>
        public IEnumerable<IRePraxisNode> Children { get; }

        /// <summary>
        /// Add a new child node to this node
        /// </summary>
        /// <param name="node"></param>
        public void AddChild(IRePraxisNode node);

        /// <summary>
        /// Removes a child node from the DBNode
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns>True if successful</returns>
        public bool RemoveChild(string symbol);

        /// <summary>
        /// Get a child node
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns>The node with the given symbol</returns>
        public IRePraxisNode GetChild(string symbol);

        /// <summary>
        /// Check if the node has a child
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns>
        /// True if a child is present with the given symbol.
        /// False otherwise.
        /// </returns>
        public bool HasChild(string symbol);

        /// <summary>
        /// Remove all children and from this node
        /// </summary>
        public void ClearChildren();

        /// <summary>
        /// Get the database sentence this node represents
        /// </summary>
        public string GetPath();
    }
}
