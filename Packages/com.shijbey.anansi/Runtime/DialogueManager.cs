using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Anansi
{
	/// <summary>
	/// The dialogue manager is responsible for coordinating communication between
	/// the AnansiStory instance and the front-end dialogue experience. It
	/// pre-processes the content coming from the story and manages control flow to
	/// allow us to change the presented speaker, change the background,
	/// and request user input.
	/// </summary>
	public class DialogueManager : MonoBehaviour
	{
		#region Fields

		/// <summary>
		/// A reference to the simulation controller to get info on characters and locations.
		/// </summary>
		[SerializeField]
		private SimulationController m_simulationController;

		/// <summary>
		/// A reference to the JSON file containing the compiled Ink story.
		/// </summary>
		[SerializeField]
		private TextAsset m_storyJson;

		/// <summary>
		/// A reference to the story constructed from the JSON data.
		/// </summary>
		private Story m_story;

		#endregion

		#region Properties

		/// <summary>
		/// The story presented by the manager.
		/// </summary>
		public Story Story => m_story;

		/// <summary>
		/// Is there currently dialogue being displayed on the screen.
		/// </summary>
		public bool IsDisplayingDialogue { get; private set; }

		/// <summary>
		/// What is the story controller currently doing.
		/// </summary>
		public bool IsWaitingForInput { get; private set; }

		/// <summary>
		///	The current input request the controller is waiting to complete.
		/// </summary>
		public InputRequest InputRequest { get; private set; }

		/// <summary>
		/// Information about the character currently speaking.
		/// </summary>
		/// <value></value>
		public SpeakerInfo CurrentSpeaker { get; private set; }

		/// <summary>
		/// Information about the background currently presented.
		/// </summary>
		/// <value></value>
		public BackgroundInfo CurrentBackground { get; private set; }

		/// <summary>
		/// The current dialogue line.
		/// </summary>
		/// <value></value>
		public string CurrentLine { get; private set; }

		#endregion

		#region Actions and Events

		/// <summary>
		/// Action invoked when starting a new dialogue;
		/// </summary>
		public Action OnDialogueStart;

		/// <summary>
		/// Action invoked when ending a dialogue;
		/// </summary>
		public Action OnDialogueEnd;

		/// <summary>
		/// Action invoked when processing the tags of a line of dialogue.
		/// </summary>
		public Action<IList<string>> OnProcessLineTags;

		/// <summary>
		/// Action invoked when the current speaker changes
		/// </summary>
		public Action<SpeakerInfo> OnSpeakerChange;

		/// <summary>
		/// Action invoked when the current background changes.
		/// </summary>
		public Action<BackgroundInfo> OnBackgroundChange;

		/// <summary>
		/// Action invoked when attempting to get user input.
		/// </summary>
		public Action<InputRequest> OnGetInput;

		/// <summary>
		/// Invoked when we have the next line of story dialogue.
		/// </summary>
		public Action<string> OnNextDialogueLine;

		/// <summary>
		/// Action invoked when loading external functions to register with the story.
		/// </summary>
		public Action<Ink.Runtime.Story> OnRegisterExternalFunctions;

		#endregion

		#region Unity Messages

		private void Awake()
		{
			IsDisplayingDialogue = false;
			IsWaitingForInput = false;
			CurrentSpeaker = null;
			CurrentBackground = null;

			if ( m_storyJson == null )
			{
				throw new NullReferenceException( "GameManager is missing storyJSON" );
			}

			m_story = new Story( m_storyJson.text );
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Initialize the dialogue manager before starting the story.
		/// </summary>
		public void Initialize()
		{
			RegisterExternalInkFunctions();
			SetSpeaker( null );
			SetBackground( null );
		}

		/// <summary>
		/// Start the dialogue and signal to the UI to open the dialogue box.
		/// </summary>
		public void StartDialogue()
		{
			IsDisplayingDialogue = true;
			OnDialogueStart?.Invoke();
		}

		/// <summary>
		/// End the current dialogue and let the player select another action or location.
		/// </summary>
		public void EndDialogue()
		{
			IsDisplayingDialogue = false;
			IsWaitingForInput = false;
			SetSpeaker( null );
			OnDialogueEnd?.Invoke();
		}

		/// <summary>
		/// Provide input if the system is waiting for input.
		/// </summary>
		public void SetInput(string variableName, object input)
		{
			IsWaitingForInput = false;
			InputRequest = null;
			Story.InkStory.state.variablesState[variableName] = input;
			AdvanceDialogue();
		}

		/// <summary>
		/// Check if the dialogue can continue further.
		/// </summary>
		/// <returns></returns>
		public bool CanContinue()
		{
			// Cannot continue if waiting for input
			if ( IsWaitingForInput ) return false;

			return Story.CanContinue();
		}

		/// <summary>
		/// Show the next line of dialogue or close if at the end
		/// </summary>
		public void AdvanceDialogue()
		{
			if ( CanContinue() )
			{
				string text = Story.Continue().Trim();
				text = PreProcessDialogueLine( text );
				CurrentLine = text;
				ProcessLineTags();
				OnNextDialogueLine( text );

				// Sometimes on navigation, we don't show any text. If this is the case,
				// do not even show the dialogue panel and try to get another line
				if ( text == "" )
				{
					// Hide();
					AdvanceDialogue();
					return;
				}
			}
			else if ( IsWaitingForInput )
			{
				return;
			}
			else
			{
				EndDialogue();
			}
		}

		/// <summary>
		/// Set information about the current speaker. Passing null clears the current speaker.
		/// </summary>
		/// <param name="info"></param>
		public void SetSpeaker(SpeakerInfo info)
		{
			CurrentSpeaker = null;
			OnSpeakerChange?.Invoke( info );
		}

		/// <summary>
		/// Set information about the current background. Passing null clears the background.
		/// </summary>
		/// <param name="info"></param>
		public void SetBackground(BackgroundInfo info)
		{
			CurrentBackground = info;
			OnBackgroundChange?.Invoke( info );
		}

		/// <summary>
		/// Jump the story to an instance of the given storylet and start the dialogue.
		/// </summary>
		/// <param name="storylet"></param>
		public void RunStorylet(Storylet storylet)
		{
			m_story.GoToStorylet( storylet );
			StartDialogue();
		}

		/// <summary>
		/// Jump the story to the given storylet instance and start the dialogue.
		/// </summary>
		/// <param name="instance"></param>
		public void RunStoryletInstance(StoryletInstance instance)
		{
			m_story.GoToStoryletInstance( instance );
			StartDialogue();
		}


		#endregion

		#region Private Methods

		/// <summary>
		/// Process the Ink tags associated with a line.
		/// </summary>
		private void ProcessLineTags()
		{
			foreach ( string line in m_story.CurrentTags )
			{
				List<string> tagParts = line.Split( " " ).Select( s => s.Trim() ).ToList();
				if ( tagParts[0] == "SetBackground:" )
				{
					SetBackground(
						new BackgroundInfo(
							backgroundId: tagParts[1],
							tags: tagParts.GetRange( 2, tagParts.Count - 2 ).ToArray()
						)
					);
				}
			}
		}

		/// <summary>
		/// PreProcess the dialogue line
		/// </summary>
		/// <param name="line"></param>
		private string PreProcessDialogueLine(string line)
		{
			Match match = Regex.Match( line, @"^(\w+[\.\w+]*):(.*)$" );

			if ( match.Value == "" )
			{
				SetSpeaker( null );
				return line;
			}
			else
			{
				List<string> speakerSpec = match.Groups[1].Value
					.Split( "." ).Select( s => s.Trim() ).ToList();

				string speakerId = speakerSpec[0];
				speakerSpec.RemoveAt( 0 );
				string[] speakerTags = speakerSpec.ToArray();

				SetSpeaker(
					new SpeakerInfo(
						speakerId,
						m_simulationController.GetCharacter( speakerId ).DisplayName,
						speakerTags
					)
				);

				string dialogueText = match.Groups[2].Value.Trim();

				return dialogueText;
			}
		}

		private void RegisterExternalInkFunctions()
		{
			Story.InkStory.BindExternalFunction(
				"GetInput",
				(string dataTypeName, string prompt, string varName) =>
				{
					IsWaitingForInput = true;
					InputDataType dataType = InputDataType.String;

					switch ( dataTypeName )
					{
						case "int":
							dataType = InputDataType.Int;
							break;
						case "float":
							dataType = InputDataType.Float;
							break;
						case "number":
							dataType = InputDataType.Float;
							break;
						case "text":
							dataType = InputDataType.String;
							break;
						default:
							dataType = InputDataType.String;
							break;
					}

					InputRequest = new InputRequest(
						prompt, varName, dataType
					);

					OnGetInput?.Invoke(
						InputRequest
					);
				}
			);

			// Load functions from external classes.
			OnRegisterExternalFunctions?.Invoke( Story.InkStory );
		}

		#endregion
	}




}
