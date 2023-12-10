using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Calypso.Editor
{
    [CustomPropertyDrawer(typeof(RePraxisEntry))]
    public class RePraxisEntryPropertyDrawer : PropertyDrawer
    {
        private SerializedProperty sentence;
        private SerializedProperty valueType;
        private SerializedProperty intValue;
        private SerializedProperty floatValue;
        private SerializedProperty stringValue;


        private void FindProperties(SerializedProperty property)
        {
            sentence = property.FindPropertyRelative("sentence");
            valueType = property.FindPropertyRelative("valueType");
            intValue = property.FindPropertyRelative("intValue");
            floatValue = property.FindPropertyRelative("floatValue");
            stringValue = property.FindPropertyRelative("stringValue");
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            FindProperties(property);

            var container = new VisualElement();

            // Define new property fields
            var sentenceField = new PropertyField(sentence);
            var valueTypeField = new PropertyField(valueType);
            var intValueField = new IntegerField("Value");
            var floatValueField = new FloatField("Value");
            var stringValueField = new TextField("Value");

            // Add the two required fields to the inspector
            container.Add(sentenceField);
            container.Add(valueTypeField);

            // Set the initial fields based on the enum
            var vt = (RePraxisEntryType)valueType.enumValueIndex;
            switch (vt)
            {
                case RePraxisEntryType.Store_True:
                    if (intValueField.parent == container) container.Remove(intValueField);
                    if (floatValueField.parent == container) container.Remove(floatValueField);
                    if (stringValueField.parent == container) container.Remove(stringValueField);
                    break;
                case RePraxisEntryType.Int:
                    if (intValueField.parent != container) container.Add(intValueField);
                    if (floatValueField.parent == container) container.Remove(floatValueField);
                    if (stringValueField.parent == container) container.Remove(stringValueField);
                    break;
                case RePraxisEntryType.Float:
                    if (intValueField.parent == container) container.Remove(intValueField);
                    if (floatValueField.parent != container) container.Add(floatValueField);
                    if (stringValueField.parent == container) container.Remove(stringValueField);
                    break;
                case RePraxisEntryType.String:
                    if (intValueField.parent == container) container.Remove(intValueField);
                    if (floatValueField.parent == container) container.Remove(floatValueField);
                    if (stringValueField.parent != container) container.Add(stringValueField);
                    break;
                default:
                    break;
            }

            // Changes the presented value field based on the valueTypeField
            valueTypeField.RegisterValueChangeCallback((evt) =>
            {
                var valueType = (RePraxisEntryType)evt.changedProperty.enumValueIndex;
                switch (valueType)
                {
                    case RePraxisEntryType.Store_True:
                        if (intValueField.parent == container) container.Remove(intValueField);
                        if (floatValueField.parent == container) container.Remove(floatValueField);
                        if (stringValueField.parent == container) container.Remove(stringValueField);
                        break;
                    case RePraxisEntryType.Int:
                        if (intValueField.parent != container) container.Add(intValueField);
                        if (floatValueField.parent == container) container.Remove(floatValueField);
                        if (stringValueField.parent == container) container.Remove(stringValueField);
                        break;
                    case RePraxisEntryType.Float:
                        if (intValueField.parent == container) container.Remove(intValueField);
                        if (floatValueField.parent != container) container.Add(floatValueField);
                        if (stringValueField.parent == container) container.Remove(stringValueField);
                        break;
                    case RePraxisEntryType.String:
                        if (intValueField.parent == container) container.Remove(intValueField);
                        if (floatValueField.parent == container) container.Remove(floatValueField);
                        if (stringValueField.parent != container) container.Add(stringValueField);
                        break;
                    default:
                        break;
                }
            });

            // Bind Properties
            intValueField.BindProperty(intValue);
            floatValueField.BindProperty(floatValue);
            stringValueField.BindProperty(stringValue);

            return container;
        }
    }
}

