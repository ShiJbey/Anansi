using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Calypso.Editor
{
    [CustomPropertyDrawer(typeof(SerializedDatabase))]
    public class SerializedRePraxisEntryPropertyDrawer : PropertyDrawer
    {
        private SerializedProperty _serializedDatabase;
        private SerializedProperty _serializedNodes;
        private List<TreeViewItemData<NodeData>> _treeRoots;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            FindProperties(property);
            return ConstructGUI();
        }

        private void FindProperties(SerializedProperty rootProperty)
        {
            _serializedDatabase = rootProperty;
            _serializedNodes = rootProperty.FindPropertyRelative("nodes");
        }

        private VisualElement ConstructGUI()
        {
            var container = new VisualElement();

            var treeView = new TreeView()
            {
                makeItem = () => new Label()
            };

            treeView.bindItem = (VisualElement element, int index) =>
            {
                var itemData = treeView.GetItemDataForIndex<NodeData>(index);
                var labelText = itemData.symbol;

                if (itemData.isTerminal)
                {
                    switch (itemData.valueType)
                    {
                        case RePraxisEntryType.Store_True:
                            labelText += " = true";
                            break;
                        case RePraxisEntryType.Int:
                            labelText += $" = {itemData.intValue.intValue}";
                            break;
                        case RePraxisEntryType.Float:
                            labelText += $" = {itemData.floatValue.floatValue}";
                            break;
                        case RePraxisEntryType.String:
                            labelText += $" = \"{itemData.stringValue.stringValue}\"";
                            break;
                    }
                }

                (element as Label).text = labelText;
            };



            CreateTreeData();

            treeView.SetRootItems(_treeRoots);


            container.Add(treeView);

            // // Define new property fields
            // var sentenceField = new PropertyField(property.FindPropertyRelative("sentence"));
            // var valueTypeField = new PropertyField(property.FindPropertyRelative("valueType"));
            // var intValueField = new IntegerField("Value");
            // var floatValueField = new FloatField("Value");
            // var stringValueField = new TextField("Value");

            // // Add the two required fields to the inspector
            // container.Add(sentenceField);
            // container.Add(valueTypeField);

            // // Changes the presented value field based on the valueTypeField
            // valueTypeField.RegisterValueChangeCallback((evt) =>
            // {
            //     var valueType = (RePraxisEntryType)evt.changedProperty.enumValueIndex;
            //     switch (valueType)
            //     {
            //         case RePraxisEntryType.Store_True:
            //             if (intValueField.parent == container) container.Remove(intValueField);
            //             if (floatValueField.parent == container) container.Remove(floatValueField);
            //             if (stringValueField.parent == container) container.Remove(stringValueField);
            //             break;
            //         case RePraxisEntryType.Int:
            //             if (intValueField.parent != container) container.Add(intValueField);
            //             if (floatValueField.parent == container) container.Remove(floatValueField);
            //             if (stringValueField.parent == container) container.Remove(stringValueField);
            //             break;
            //         case RePraxisEntryType.Float:
            //             if (intValueField.parent == container) container.Remove(intValueField);
            //             if (floatValueField.parent != container) container.Add(floatValueField);
            //             if (stringValueField.parent == container) container.Remove(stringValueField);
            //             break;
            //         case RePraxisEntryType.String:
            //             if (intValueField.parent == container) container.Remove(intValueField);
            //             if (floatValueField.parent == container) container.Remove(floatValueField);
            //             if (stringValueField.parent != container) container.Add(stringValueField);
            //             break;
            //         default:
            //             break;
            //     }
            // });

            // // Bind Properties
            // intValueField.BindProperty(property.FindPropertyRelative("intValue"));
            // floatValueField.BindProperty(property.FindPropertyRelative("floatValue"));
            // stringValueField.BindProperty(property.FindPropertyRelative("stringValue"));

            // // Disable users from modifying this data at runtime
            // container.SetEnabled(false);

            return container;
        }

        private void CreateTreeData()
        {
            if (_treeRoots == null)
                _treeRoots = new List<TreeViewItemData<NodeData>>();

            _treeRoots.Clear();

            if (_serializedNodes.arraySize == 0) return;

            TreeViewItemData<NodeData> item;
            CreateSubTree(_serializedNodes.arraySize - 1, out item);
            _treeRoots.Add(item);

            // // The root will be the last entry in the array
            // var serializedRoot = _serializedNodes.GetArrayElementAtIndex(_serializedNodes.arraySize - 1);
            // var rootChildren = serializedRoot.FindPropertyRelative("children");

            // // Loop through each of the children
            // for (int i = 0; i < rootChildren.arraySize; ++i)
            // {
            //     var childIndex = rootChildren.GetArrayElementAtIndex(i).intValue;

            //     TreeViewItemData<NodeData> item;
            //     CreateSubTree(childIndex, out item);
            //     _treeRoots.Add(item);
            // }
        }

        private void CreateSubTree(int index, out TreeViewItemData<NodeData> item)
        {
            var serializedNode = _serializedNodes.GetArrayElementAtIndex(index);

            var childNodes = new List<TreeViewItemData<NodeData>>();

            var nodeChildren = serializedNode.FindPropertyRelative("children");

            for (int i = 0; i < nodeChildren.arraySize; ++i)
            {
                var childIndex = nodeChildren.GetArrayElementAtIndex(i).intValue;

                TreeViewItemData<NodeData> childNode;
                CreateSubTree(childIndex, out childNode);
                childNodes.Add(childNode);
            }

            var newItem = new TreeViewItemData<NodeData>(index, new NodeData()
            {
                symbol = serializedNode.FindPropertyRelative("symbol").stringValue,
                intValue = serializedNode.FindPropertyRelative("intValue"),
                floatValue = serializedNode.FindPropertyRelative("floatValue"),
                stringValue = serializedNode.FindPropertyRelative("stringValue"),
                valueType = (RePraxisEntryType)serializedNode.FindPropertyRelative("valueType").enumValueIndex,
                isTerminal = serializedNode.FindPropertyRelative("children").arraySize == 0,
            }, childNodes);
            item = newItem;
        }
    }

    public struct NodeData
    {
        public string symbol;
        public RePraxisEntryType valueType;
        public bool isTerminal;
        public SerializedProperty intValue;
        public SerializedProperty floatValue;
        public SerializedProperty stringValue;
    }
}
