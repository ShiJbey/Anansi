using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using System;

namespace Anansi
{
	/// <summary>
	/// Controls the text box that displays dialogue text and choices to the player
	/// </summary>
	public class DialoguePanelController : MonoBehaviour
	{
		#region Fields

		/// <summary>
		/// A reference to the game's dialogue manager
		/// </summary>
		private StoryController m_storyController;

		/// <summary>
		/// A reference to the game manager
		/// </summary>
		private GameManager m_gameManager;

		/// <summary>
		/// A reference to the button that advances to the next set of dialogue
		/// </summary>
		[SerializeField]
		private Button m_advanceDialogueButton;

		/// <summary>
		/// A reference to the component displaying the dialogue text.
		/// </summary>
		[SerializeField]
		private TMP_Text m_dialogueText;

		/// <summary>
		/// A reference to the component displaying the speaker's name.
		/// </summary>
		[SerializeField]
		private TMP_Text m_speakerName;

		/// <summary>
		/// The on-screen position of the dialogue panel.
		/// </summary>
		private Vector3 m_onScreenPosition;

		/// <summary>
		/// The off-screen position of the dialogue panel.
		/// </summary>
		private Vector3 m_offScreenPosition;

		/// <summary>
		/// A reference to GameObject that manages choice buttons.
		/// </summary>
		[SerializeField]
		private ChoiceDialogController m_choiceDialog;

		[SerializeField]
		private InputPanelController m_inputPanel;

		/// <summary>
		/// A reference to the coroutine that handles sliding the dialogue panel on and off screen
		/// </summary>
		private Coroutine m_panelSlideCoroutine = null;

		/// <summary>
		/// A reference to the coroutine that handles displaying dialogue using the typewriter
		/// effect.
		/// </summary>
		private Coroutine m_typingCoroutine = null;

		/// <summary>
		/// Should the typing coroutine skip to the end of the dialogue line.
		/// </summary>
		private bool m_skipTypewriterEffect = false;

		/// <summary>
		/// Tracks if the panel is hidden or moving into the hidden state.
		/// </summary>
		private bool m_isPanelHidden = false;

		/// <summary>
		/// The fade-out duration time in seconds.
		/// </summary>
		[SerializeField]
		protected float m_slideOutSeconds;

		/// <summary>
		/// The fade-in duration time in seconds.
		/// </summary>
		[SerializeField]
		protected float m_slideInSeconds;

		/// <summary>
		/// The time delay between displaying the next character during the typewriter effect.
		/// </summary>
		[SerializeField]
		protected float m_typingSpeed = 0.04f;

		/// <summary>
		/// A reference to the panel's transform
		/// </summary>
		private RectTransform m_rectTransform;

		/// <summary>
		/// The current choice to select in the dialogue (-1 halts dialogue progression)
		/// </summary>
		private int m_userChoiceIndex = -1;

		#endregion

		#region Properties

		/// <summary>
		/// Is the panel currently hidden or being hidden
		/// </summary>
		public bool IsHidden => m_isPanelHidden;

		/// <summary>
		/// Is the controller currently typing text to the dialogue box.
		/// </summary>
		public bool IsTyping { get; private set; }

		/// <summary>
		/// A reference to the controller script for the choice button dialog box
		/// </summary>
		public ChoiceDialogController ChoiceDialog
		{
			get
			{
				if ( m_choiceDialog != null ) return m_choiceDialog;
				throw new NullReferenceException( "ChoiceDialog for DialoguePanel is null." );
			}
		}

		#endregion

		#region Events and Actions

		/// <summary>
		/// Event invoked when the panel's Show() method is called.
		/// </summary>
		public UnityEvent OnShow;

		/// <summary>
		/// Event invoked when the panel's Hide() method is called.
		/// </summary>
		public UnityEvent OnHide;

		#endregion

		#region Unity Messages

		private void Awake()
		{
			m_rectTransform = gameObject.transform as RectTransform;
			m_storyController = FindObjectOfType<StoryController>();
			m_gameManager = FindObjectOfType<GameManager>();
		}

		private void Start()
		{
			Vector3 startingPos = m_rectTransform.position;
			m_onScreenPosition = new Vector3( startingPos.x, startingPos.y, startingPos.z );

			m_offScreenPosition = new Vector3(
				startingPos.x,
				startingPos.y - (m_rectTransform.rect.height + 200),
				startingPos.z
			);

			m_advanceDialogueButton.onClick.AddListener( HandleAdvanceDialogueButtonClick );
			m_choiceDialog.OnChoiceSelected.AddListener( HandleChoiceSelection );
			m_storyController.OnDialogueStart += HandleOnDialogueStart;
			m_storyController.OnDialogueEnd += HandleOnDialogueEnd;
			m_storyController.OnSpeakerChange += HandleSpeakerChange;
			m_storyController.OnNextDialogueLine += HandleDialogueLine;

			Hide();
		}

		private void OnDisable()
		{
			m_advanceDialogueButton.onClick.RemoveListener( HandleAdvanceDialogueButtonClick );
			m_choiceDialog.OnChoiceSelected.RemoveListener( HandleChoiceSelection );
			m_storyController.OnDialogueStart -= HandleOnDialogueStart;
			m_storyController.OnDialogueEnd -= HandleOnDialogueEnd;
			m_storyController.OnSpeakerChange -= HandleSpeakerChange;
			m_storyController.OnNextDialogueLine -= HandleDialogueLine;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Set the name in the dialog box of the character speaking
		/// </summary>
		/// <param name="name"></param>
		public void SetSpeakerName(string name)
		{
			m_speakerName.text = name;
		}

		/// <summary>
		/// Bypass the typewriter effect and display the full line of text.
		/// </summary>
		public void JumpToEndOfText()
		{
			if ( IsTyping ) m_skipTypewriterEffect = true;
		}

		/// <summary>
		/// Show the dialogue panel.
		/// </summary>
		public void Show()
		{
			m_isPanelHidden = false;

			if ( m_panelSlideCoroutine != null )
			{
				StopCoroutine( m_panelSlideCoroutine );
			}

			m_panelSlideCoroutine = StartCoroutine( SlidePanelIn() );

			if ( OnShow != null ) OnShow.Invoke();
		}

		/// <summary>
		/// Hide the dialogue and choice panels.
		/// </summary>
		public void Hide()
		{
			m_isPanelHidden = true;

			m_choiceDialog.gameObject.SetActive( false );

			if ( m_panelSlideCoroutine != null )
			{
				StopCoroutine( m_panelSlideCoroutine );
			}

			m_panelSlideCoroutine = StartCoroutine( SlidePanelOut() );

			if ( m_typingCoroutine != null ) StopCoroutine( m_typingCoroutine );
			IsTyping = false;
			m_advanceDialogueButton.interactable = false;

			if ( OnHide != null ) OnHide.Invoke();
		}

		/// <summary>
		/// Show the next line of dialogue or close if at the end
		/// </summary>
		public void AdvanceDialogue()
		{
			m_storyController.AdvanceDialogue();
			// if (
			// 	m_storyController.CanContinue()
			// 	&& !IsTyping
			// )
			// {
			// 	string text = m_storyController.GetNextLine().Trim();

			// 	// Sometimes on navigation, we don't show any text. If this is the case,
			// 	// do not even show the dialogue panel and try to get another line
			// 	if ( text == "" )
			// 	{
			// 		// Hide();
			// 		AdvanceDialogue();
			// 		return;
			// 	}

			// 	if ( m_isPanelHidden ) Show();

			// 	if ( m_typingCoroutine != null ) StopCoroutine( m_typingCoroutine );

			// 	IsTyping = true;
			// 	m_advanceDialogueButton.interactable = false;

			// 	m_typingCoroutine = StartCoroutine( DisplayTextCoroutine( text ) );
			// }
			// else if ( m_storyController.IsWaitingForInput )
			// {
			// 	m_advanceDialogueButton.interactable = false;
			// 	return;
			// }
			// else
			// {
			// 	m_storyController.EndDialogue();
			// }
		}

		#endregion

		#region Private Methods

		private void HandleDialogueLine(string text)
		{
			if ( m_isPanelHidden ) Show();

			if ( m_typingCoroutine != null ) StopCoroutine( m_typingCoroutine );

			IsTyping = true;
			m_advanceDialogueButton.interactable = false;

			m_typingCoroutine = StartCoroutine( DisplayTextCoroutine( text ) );
		}

		/// <summary>
		/// A callback executed when the advance dialogue button is clicked
		/// </summary>
		private void HandleAdvanceDialogueButtonClick()
		{
			AdvanceDialogue();
			m_advanceDialogueButton.interactable = false;
		}

		/// <summary>
		/// A callback executed when a choice button is clicked in the choice dialog box
		/// </summary>
		/// <param name="choiceIndex"></param>
		private void HandleChoiceSelection(int choiceIndex)
		{
			m_userChoiceIndex = choiceIndex;
		}

		private void HandleOnDialogueStart()
		{
			AdvanceDialogue();
		}

		private void HandleOnDialogueEnd()
		{
			Hide();
		}

		private void HandleSpeakerChange(SpeakerInfo info)
		{
			if ( info == null )
			{
				SetSpeakerName( "" );
			}
			else
			{
				SetSpeakerName( info.SpeakerId );
			}
		}

		/// <summary>
		/// Displays text using a typewriter effect where each character appears once at a time.
		/// </summary>
		/// <returns></returns>
		private IEnumerator DisplayTextCoroutine(string text)
		{
			m_advanceDialogueButton.interactable = false;
			m_dialogueText.text = "";

			foreach ( char letter in text.ToCharArray() )
			{
				if ( m_skipTypewriterEffect )
				{
					m_dialogueText.text = text;
					m_skipTypewriterEffect = false;
					break;
				}

				m_dialogueText.text += letter;
				yield return new WaitForSeconds( m_typingSpeed );
			}

			if ( m_storyController.IsWaitingForInput )
			{
				// m_inputPanel.SetPrompt( m_storyController.InputRequest.Prompt );
				// m_inputPanel.SetDataType( m_storyController.InputRequest.DataType );
				// m_inputPanel.SetVariableName( m_storyController.InputRequest.VariableName );

				m_inputPanel.HandleGetInput( m_storyController.InputRequest );

				yield return new WaitUntil( () => !m_storyController.IsWaitingForInput );

				m_inputPanel.Hide();

				AdvanceDialogue();
			}

			if ( m_storyController.HasChoices() )
			{
				var choices = m_storyController.GetChoices();
				m_choiceDialog.gameObject.SetActive( true );
				m_choiceDialog.SetChoices( choices );

				yield return new WaitUntil( () => m_userChoiceIndex != -1 );

				m_choiceDialog.gameObject.SetActive( false );

				m_storyController.MakeChoice( m_userChoiceIndex );

				m_userChoiceIndex = -1;

				IsTyping = false;

				AdvanceDialogue();
			}
			else
			{
				m_advanceDialogueButton.interactable = true;
				IsTyping = false;
			}
		}

		private IEnumerator SlidePanelOut()
		{
			Vector3 initialPosition = m_rectTransform.position;
			float elapsedTime = 0f;

			while ( elapsedTime < m_slideOutSeconds )
			{
				elapsedTime += Time.deltaTime;
				m_rectTransform.position =
					Vector3.Lerp( initialPosition, m_offScreenPosition, elapsedTime / m_slideOutSeconds );
				yield return null;
			}
		}

		private IEnumerator SlidePanelIn()
		{
			Vector3 initialPosition = m_rectTransform.position;
			float elapsedTime = 0f;

			while ( elapsedTime < m_slideInSeconds )
			{
				elapsedTime += Time.deltaTime;
				m_rectTransform.position =
					Vector3.Lerp( initialPosition, m_onScreenPosition, elapsedTime / m_slideInSeconds );
				yield return null;
			}
		}

		#endregion
	}

}
