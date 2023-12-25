using System.Collections.Generic;
using System.Linq;
using Calypso.RePraxis;
using UnityEngine;


namespace Calypso
{
    /// <summary>
    /// Wraps a RePraxis database to make it available to Unity Scripts
    /// </summary>
    public class StoryDatabase : MonoBehaviour, ISerializationCallbackReceiver
    {
        #region Protected Fields

        /// <summary>
        /// Unity serializable node representation of the database
        /// </summary>
        [SerializeField]
        private List<string> _serializedSentences = new List<string>();

        /// <summary>
        /// Reference to the fully instantiated database
        /// </summary>
        private RePraxisDatabase db = new RePraxisDatabase();

        #endregion

        public RePraxisDatabase DB => db;

        #region Serialization Callbacks

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            // Do Nothing
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            // Convert the tree representation of the database into a series of string sentences

            // Perform DFS, save the current path, when we hit a leaf, save the path to a string
            // in the collection
            _serializedSentences.Clear();

            var nodeStack = new Stack<IRePraxisNode>(DB.Root.Children);

            while (nodeStack.Count > 0)
            {
                IRePraxisNode node = nodeStack.Pop();

                IEnumerable<IRePraxisNode> children = node.Children;

                if (children.Count() > 0)
                {
                    // Add children to the stack
                    foreach (var child in children)
                    {
                        nodeStack.Push(child);
                    }
                }
                else
                {
                    // This is a leaf
                    _serializedSentences.Add(node.GetPath());
                }
            }
        }

        #endregion
    }
}
