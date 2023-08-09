using Ink.Runtime;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InkManager : MonoBehaviour
{
    [SerializeField]
    private TextAsset _inkJsonAsset;

    private Story _story;

    [SerializeField]
    private TMP_Text _textField;

    [SerializeField]
    private TMP_Text _speakerName;

    [SerializeField]
    private RectTransform _choiceButtonContainer;

    [SerializeField]
    private Button _choiceButtonPrefab;

    [SerializeField]
    private GameObject _choicePanel;

    [SerializeField]
    private Color _normalTextColor;

    [SerializeField]
    private Color _thoughtTextColor;


    // Start is called before the first frame update
    void Start()
    {
        _story = new Story(_inkJsonAsset.text);

        _story.onError += (msg, type) => {
            if (type == Ink.ErrorType.Warning)
                Debug.LogWarning(msg);
            else
                Debug.LogError(msg);
        };

        AdvanceDialog();
    }

    public void AdvanceDialog()
    {
        if (_story.canContinue)
        {
            string text = _story.Continue(); // gets next line

            text = text?.Trim(); // removes white space from text

            ApplyStyling();
            UpdateSpeakerName();

            _textField.text = text; // displays new text

            if (_story.currentChoices.Count > 0)
            {
                DisplayChoices();
            }
        }
        else if (_story.currentChoices.Count > 0)
        {
            DisplayChoices();
        }
    }

    private void DisplayChoices()
    {
        // checks if choices are already being displayed
        if (_choiceButtonContainer.GetComponentsInChildren<Button>().Length > 0)
        {
            Debug.Log("Choices already displayed.");
            return;
        }

        _choicePanel.SetActive(true);

        for (int i = 0; i < _story.currentChoices.Count; i++) // iterates through all choices
        {
            var choice = _story.currentChoices[i];
            Debug.Log(choice.text);
            var button = CreateChoiceButton(choice.text); // creates a choice button

            button.onClick.AddListener(() => OnClickChoiceButton(choice));
        }
    }

    Button CreateChoiceButton(string text)
    {
        // creates the button from a prefab
        var choiceButton = Instantiate(_choiceButtonPrefab);
        choiceButton.transform.SetParent(_choiceButtonContainer.transform, false);

        // sets text on the button
        var buttonText = choiceButton.GetComponentInChildren<TMP_Text>();
        buttonText.text = text;

        return choiceButton;
    }

    void OnClickChoiceButton(Choice choice)
    {
        _story.ChooseChoiceIndex(choice.index); // tells ink which choice was selected
        RefreshChoiceView(); // removes choices from the screen
        _choicePanel.SetActive(false);
        AdvanceDialog();
    }

    void RefreshChoiceView()
    {
        if (_choiceButtonContainer != null)
        {
            foreach (var button in _choiceButtonContainer.GetComponentsInChildren<Button>())
            {
                Destroy(button.gameObject);
            }
        }
    }

    private void ApplyStyling()
    {
        if (_story.currentTags.Contains("thought"))
        {
            _textField.color = _thoughtTextColor;
            _textField.fontStyle = FontStyles.Italic;
        }
        else
        {
            _textField.color = _normalTextColor;
            _textField.fontStyle = FontStyles.Normal;
        }
    }

    private void UpdateSpeakerName()
    {
        foreach (string line in _story.currentTags)
        {
            if (line.Contains("speaker"))
            {
                string speakerName = line.Split(':')[1].Trim();
                _speakerName.text = speakerName;
                return;
            }
        }
    }
}
