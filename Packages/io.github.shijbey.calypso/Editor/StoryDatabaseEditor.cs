using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Calypso.RePraxis;

namespace Calypso.Editor
{
    /// <summary>
    /// Provides users with a read-only custom inspector that visualizes
    /// the contents of the story database as a tree structure.
    /// </summary>
    [CustomEditor(typeof(StoryDatabase))]
    public class StoryDatabaseEditor : UnityEditor.Editor
    {
        private SerializedProperty _serializedDatabase;

        private void OnEnable()
        {
            _serializedDatabase = serializedObject.FindProperty("_serializedDatabase");
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
            Repaint();
        }

        private void ValueRemovedForceRepaint(string sentence)
        {
            Repaint();
        }

        public override VisualElement CreateInspectorGUI()
        {
            var container = new VisualElement();

            var databaseField = new PropertyField(_serializedDatabase);

            container.Add(databaseField);

            return container;
        }

    }
}

