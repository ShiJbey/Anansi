using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Calypso.RePraxis;

namespace Calypso
{
    public class StoryDatabaseDebugger : EditorWindow
    {
        private ScrollView scrollView;
        private StoryDatabase storyDatabase;


        [MenuItem("Window/Calypso/Database Viewer")]
        public static void ShowWindow()
        {
            StoryDatabaseDebugger window = GetWindow<StoryDatabaseDebugger>();
            window.titleContent = new GUIContent("Calypso Database Viewer");
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            scrollView = new ScrollView(ScrollViewMode.Vertical);

            root.Add(scrollView);

            storyDatabase = FindObjectOfType<StoryDatabase>();

            if (storyDatabase is null)
            {
                Debug.LogError(
                    "Database Viewer cannot find GameObject with StoryDatabase component.");
            }
        }

        public void Update()
        {
            scrollView.Clear();

            var nodeStack = new Stack<IRePraxisNode>(storyDatabase.DB.Root.Children);

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
                    scrollView.Add(new Label(node.GetPath()));
                }
            }
        }
    }
}
