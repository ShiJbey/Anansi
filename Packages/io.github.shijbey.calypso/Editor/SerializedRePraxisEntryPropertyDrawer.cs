using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

#if false
namespace Calypso.Editor
{
    [CustomPropertyDrawer(typeof(SerializedRePraxisEntry))]
    public class SerializedRePraxisEntryPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();

            // Define new property fields
            var sentenceField = new PropertyField(property.FindPropertyRelative("sentence"));
            var valueTypeField = new PropertyField(property.FindPropertyRelative("valueType"));
            var intValueField = new IntegerField("Value");
            var floatValueField = new FloatField("Value");
            var stringValueField = new TextField("Value");

            // Add the two required fields to the inspector
            container.Add(sentenceField);
            container.Add(valueTypeField);

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
            intValueField.BindProperty(property.FindPropertyRelative("intValue"));
            floatValueField.BindProperty(property.FindPropertyRelative("floatValue"));
            stringValueField.BindProperty(property.FindPropertyRelative("stringValue"));

            // Disable users from modifying this data at runtime
            container.SetEnabled(false);

            return container;
        }
    }
}
#endif
