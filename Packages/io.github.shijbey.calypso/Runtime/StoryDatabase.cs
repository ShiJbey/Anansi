using System;
using System.Collections.Generic;
using Calypso.RePraxis;
using UnityEngine;


namespace Calypso
{
    /// <summary>
    /// Wraps a RePraxis database to make it available to Unity Scripts
    /// </summary>
    public class StoryDatabase : MonoBehaviour, ISerializationCallbackReceiver
    {
        #region Fields
        /// <summary>
        /// Unity serializable node representation of the database
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private List<SerializedDatabaseNode> _serializedNodes;
        /// <summary>
        /// Encapsulates all nodes under a single struct
        /// </summary>
        [SerializeField]
        private SerializedDatabase _serializedDatabase;
        /// <summary>
        /// Reference to the fully instantiated database
        /// </summary>
        public RePraxisDatabase db = new RePraxisDatabase();
        #endregion

        private void OnEnable()
        {
            RePraxisDatabase.OnEntryAdded += ValueAddedForceRepaint;
            RePraxisDatabase.OnEntryRemoved += ValueRemovedForceRepaint;
        }

        private void OnDisable()
        {
            RePraxisDatabase.OnEntryAdded -= ValueAddedForceRepaint;
            RePraxisDatabase.OnEntryRemoved -= ValueRemovedForceRepaint;
        }

        private void ValueAddedForceRepaint(string sentence, object value)
        {
            Debug.Log("Repainting on add");
            SerializeDatabase();
        }

        private void ValueRemovedForceRepaint(string sentence)
        {
            SerializeDatabase();
        }

        #region Serialization Callbacks & Helpers
        /// <summary>
        /// Serializes a database node
        /// </summary>
        /// <param name="id">An ID to assign to the node</param>
        /// <param name="node">The node to serialize</param>
        /// <returns></returns>
        private static SerializedDatabaseNode SerializeNode(int id, DBNode node)
        {
            var serializedNode = new SerializedDatabaseNode()
            {
                id = id,
                symbol = node.Symbol,
                isExclusive = node.IsExclusive
            };

            if (node.Value is bool)
            {
                serializedNode.valueType = RePraxisEntryType.Store_True;
            }
            else if (node.Value is int)
            {
                serializedNode.valueType = RePraxisEntryType.Int;
                serializedNode.intValue = (int)node.Value;
            }
            else if (node.Value is float)
            {
                serializedNode.valueType = RePraxisEntryType.Float;
                serializedNode.floatValue = (float)node.Value;
            }
            else if (node.Value is string)
            {
                serializedNode.valueType = RePraxisEntryType.String;
                serializedNode.stringValue = (string)node.Value;
            }

            return serializedNode;
        }

        private DBNode DeserializeDatabaseNode(SerializedDatabaseNode serializedNode)
        {
            return new DBNode(serializedNode.symbol, serializedNode.Value, serializedNode.isExclusive);
        }

        /// <summary>
        /// Create a list of serialized nodes for a subtree of a database
        /// </summary>
        /// <param name="node">The root node of the subtree</param>
        /// <param name="serializedNodes">The nodes that have already been serialized</param>
        /// <returns>The ID of the serialized root node</returns>
        private int SerializeSubTree(DBNode node)
        {
            var childNodes = new List<int>();

            foreach (var child in node.Children)
            {
                // Recursively call this method on the child nodes
                var childID = SerializeSubTree(child);
                childNodes.Add(childID);
            }

            var serializedNode = SerializeNode(_serializedNodes.Count, node);
            serializedNode.children = childNodes;
            _serializedNodes.Add(serializedNode);
            return serializedNode.id;
        }

        /// <summary>
        /// Add the subtree to the database
        /// </summary>
        /// <param name="index"></param>
        /// <param name="node"></param>
        private void DeserializeSubTree(int index, out DBNode node)
        {
            var serializedNode = _serializedNodes[index];

            var newNode = DeserializeDatabaseNode(serializedNode);

            foreach (var childIndex in serializedNode.children)
            {
                DBNode childNode;
                DeserializeSubTree(childIndex, out childNode);
                newNode.AddChild(childNode);
            }

            node = newNode;
        }

        void SerializeDatabase()
        {
            // Serialize the most up-to-date data before Unity reads the
            // _serializedNodes field

            // (0) Ensure neither of the fields are null
            if (_serializedNodes == null)
                _serializedNodes = new List<SerializedDatabaseNode>();
            if (db == null)
                db = new RePraxisDatabase();

            // (1) Clear the previous serialized data
            _serializedNodes.Clear();

            // (2) Perform a depth-first traversal over the database and 
            // populate a list of all the nodes 
            SerializeSubTree(db.Root);
            _serializedDatabase = new SerializedDatabase()
            {
                nodes = _serializedNodes,
            };
        }

        /// <summary>
        /// Reconstruct the story database from the serialized entries
        /// </summary>
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            // Take data that was written to the serialized nodes field and
            // use it to populate the runtime data

            db = new RePraxisDatabase();

            // Do nothing if we have no serialized nodes
            if (_serializedNodes.Count == 0) return;

            // The root node will be the last in the list
            var serializedRoot = _serializedNodes[_serializedNodes.Count - 1];

            // Rehydrate the tree starting with the children of the root
            foreach (var childIndex in serializedRoot.children)
            {
                DBNode childNode;
                DeserializeSubTree(childIndex, out childNode);
                db.Root.AddChild(childNode);
            }
        }

        /// <summary>
        /// Serialize the database to a list of Unity serializable structs
        /// </summary>
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            SerializeDatabase();
        }
        #endregion
    }

    [Serializable]
    public struct SerializedDatabaseNode
    {
        public int id;
        public string symbol;
        public bool isExclusive;
        public List<int> children;
        public RePraxisEntryType valueType;
        public int intValue;
        public float floatValue;
        public string stringValue;
        public object Value
        {
            get
            {
                switch (valueType)
                {
                    case RePraxisEntryType.Store_True:
                        return true;
                    case RePraxisEntryType.Int:
                        return intValue;
                    case RePraxisEntryType.Float:
                        return floatValue;
                    case RePraxisEntryType.String:
                        return stringValue;
                }
                throw new Exception($"Unknown value type: {valueType}");
            }
        }
    }

    [Serializable]
    public struct SerializedDatabase
    {
        public List<SerializedDatabaseNode> nodes;
    }

    [Serializable]
    public class RePraxisEntry
    {
        public string sentence;
        public RePraxisEntryType valueType;
        public int intValue;
        public float floatValue;
        public string stringValue;
        public bool loadedIntoDatabase;

        public bool IsValid
        {
            get
            {
                return !sentence.Equals("");
            }
        }

        public object Value
        {
            get
            {
                switch (valueType)
                {
                    case RePraxisEntryType.Store_True:
                        return true;
                    case RePraxisEntryType.Int:
                        return intValue;
                    case RePraxisEntryType.Float:
                        return floatValue;
                    case RePraxisEntryType.String:
                        return stringValue;
                }
                throw new Exception($"Unknown value type: {valueType}");
            }
        }
    }

    public enum RePraxisEntryType
    {
        Store_True,
        Int,
        Float,
        String,
    }
}
