using System.Collections.Generic;

namespace Calypso
{
    public class DialogueVariableSync
    {
        private Dictionary<string, Ink.Runtime.Object> m_variables;

        public Dictionary<string, Ink.Runtime.Object> Variables => m_variables;

        // Start is called before the first frame update
        public DialogueVariableSync()
        {
            m_variables = new Dictionary<string, Ink.Runtime.Object>();
        }

        public void StartListening(Ink.Runtime.Story story)
        {
            SetVariablesInStory(story);
            story.variablesState.variableChangedEvent += HandleVariableChanged;
        }

        public void StopListening(Ink.Runtime.Story story)
        {
            story.variablesState.variableChangedEvent -= HandleVariableChanged;
        }

        private void HandleVariableChanged(string name, Ink.Runtime.Object value)
        {
            m_variables[name] = value;
        }

        private void SetVariablesInStory(Ink.Runtime.Story story)
        {
            foreach (var variable in m_variables)
            {
                story.variablesState.SetGlobal(variable.Key, variable.Value);
            }
        }
    }
}
