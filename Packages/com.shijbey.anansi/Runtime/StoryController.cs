using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RePraxis;
using UnityEngine.Events;
using UnityEditor;

namespace Anansi
{
	/// <summary>
	/// This class is responsible for managing the progression of the story and the sequencing of
	/// dynamic content.
	/// </summary>
	public class StoryController : MonoBehaviour
	{
		#region Constants

		/// <summary>
		/// The prefix value that the manager looks for when loading storylets from a Story.
		/// </summary>
		private const string STORYLET_ID_PREFIX = "storylet_";

		/// <summary>
		/// The ID prefix for Ink knots that correspond to actions.
		/// </summary>
		private const string ACTION_ID_PREFIX = "action_";

		/// <summary>
		/// The ID prefix for Ink knots that correspond to locations.
		/// </summary>
		private const string LOCATION_ID_PREFIX = "location_";

		#endregion

		#region Fields

		/// <summary>
		/// A reference to the JSON file containing the compiled Ink story.
		/// </summary>
		[SerializeField]
		private TextAsset m_storyJson;

		/// <summary>
		/// A reference to the story constructed from the JSON data.
		/// </summary>
		private Ink.Runtime.Story m_story;

		/// <summary>
		/// A reference to the starting storylet.
		/// </summary>
		private Storylet m_startingStorylet;

		/// <summary>
		/// All storylets related to moving the story forward.
		/// </summary>
		private Dictionary<string, Storylet> m_basicStorylets;

		/// <summary>
		/// All storylets related to locations on the map.
		/// </summary>
		private Dictionary<string, Storylet> m_locationStorylets;

		/// <summary>
		/// All storylets related to actions the player can take at locations.
		/// </summary>
		private Dictionary<string, Storylet> m_actionStorylets;

		/// <summary>
		/// The storylet instance being presented to the player.
		/// </summary>
		private StoryletInstance m_currentStorylet = null;

		/// <summary>
		/// The next storylet to play after the current storylet concludes.
		/// </summary>
		private StoryletInstance m_storyletOnDeck = null;

		/// <summary>
		/// An internal cache of dynamic choices to present the player. This list is added to
		/// when we want to present storylet diverts as if they are native Ink choices.
		/// </summary>
		private List<StoryletInstance> m_dynamicChoices;

		/// <summary>
		/// The IDs of all the choices currently in the m_dynamic choices list.
		/// </summary>
		private HashSet<string> m_dynamicChoiceIds;

		/// <summary>
		/// This variable caches a reference to a storylet instance and is used by the
		/// GetBindingVar() external function when evaluating storylet instance weights.
		/// </summary>
		private StoryletInstance m_boundStoryletInstance = null;

		#endregion

		#region Properties

		/// <summary>
		/// All tags belonging to the current line of dialogue.
		/// </summary>
		public List<string> LineTags { get; private set; }

		/// <summary>
		/// The database used for storylet queries.
		/// </summary>
		public RePraxisDatabase DB { get; set; }

		/// <summary>
		/// What is the story controller currently doing.
		/// </summary>
		public bool IsWaitingForInput { get; private set; }

		/// <summary>
		/// Is there currently dialogue being displayed on the screen.
		/// </summary>
		public bool IsDisplayingDialogue { get; private set; }

		/// <summary>
		///	The current input request the controller is waiting to complete.
		/// </summary>
		public InputRequest InputRequest { get; private set; }

		#endregion

		#region Events and Actions

		/// <summary>
		/// Action invoked when loading external functions to register with the story.
		/// </summary>
		public UnityAction<Ink.Runtime.Story> OnRegisterExternalFunctions;

		/// <summary>
		/// Action invoked when starting a new dialogue;
		/// </summary>
		public UnityAction OnDialogueStart;

		/// <summary>
		/// Action invoked when ending a dialogue;
		/// </summary>
		public UnityAction OnDialogueEnd;

		/// <summary>
		/// Action invoked when processing the tags of a line of dialogue.
		/// </summary>
		public UnityAction<IList<string>> OnProcessLineTags;

		/// <summary>
		/// Action invoked when the current speaker changes
		/// </summary>
		public UnityAction<SpeakerInfo> OnSpeakerChange;

		/// <summary>
		/// Action invoked when attempting to get user input.
		/// </summary>
		public UnityAction<InputRequest> OnGetInput;

		/// <summary>
		/// Invoked when we have the next line of story dialogue.
		/// </summary>
		public UnityAction<string> OnNextDialogueLine;

		/// <summary>
		/// Allow external listeners to contribute the the weight of a storylet instance.
		/// </summary>
		public UnityAction<StoryletInstance> OnScoreStoryletInstance;

		#endregion

		#region Unity Messages

		private void Awake()
		{
			if ( m_storyJson == null )
			{
				throw new NullReferenceException( "StoryController is missing storyJSON" );
			}

			DB = new RePraxisDatabase();
			m_basicStorylets = new Dictionary<string, Storylet>();
			m_story = new Ink.Runtime.Story( m_storyJson.text );
			IsDisplayingDialogue = false;
			IsWaitingForInput = false;
			m_locationStorylets = new Dictionary<string, Storylet>();
			m_actionStorylets = new Dictionary<string, Storylet>();

			LoadStorylets();
		}

		public void OnEnable()
		{
			if ( m_story )
			{
				m_story.onError += HandleStoryErrors;
			}
		}

		public void OnDisable()
		{
			if ( m_story )
			{
				m_story.onError -= HandleStoryErrors;
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Run all setup-tasks before starting the story.
		/// </summary>
		public void Initialize()
		{
			RegisterExternalFunctions();
		}

		/// <summary>
		/// Show the next line of dialogue or close if at the end
		/// </summary>
		public void AdvanceDialogue()
		{
			if ( CanContinue() )
			{
				string text = GetNextLine().Trim();

				// Sometimes on navigation, we don't show any text. If this is the case,
				// do not even show the dialogue panel and try to get another line
				if ( text == "" )
				{
					// Hide();
					AdvanceDialogue();
					return;
				}

				OnNextDialogueLine?.Invoke( text );
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
		/// Get the next line of dialogue
		/// </summary>
		/// <returns></returns>
		private string GetNextLine()
		{
			// Follow the current storylet until it is exhausted
			if ( m_currentStorylet.Story.canContinue )
			{
				// Get the next line of text from Ink
				string line = m_currentStorylet.Story.Continue().Trim();

				line = PreProcessDialogueLine( line );

				// Process tags
				ProcessTags( m_currentStorylet.Story.currentTags );

				IsDisplayingDialogue = true;

				return line;
			}

			if ( m_storyletOnDeck != null )
			{
				RunStoryletInstance( m_storyletOnDeck );
				m_storyletOnDeck = null;

				// Get the next line of text from Ink
				string line = m_currentStorylet.Story.Continue();

				line = PreProcessDialogueLine( line );

				// Process tags
				ProcessTags( m_currentStorylet.Story.currentTags );

				IsDisplayingDialogue = true;

				return line;
			}

			throw new Exception( "Cannot get next line of dialogue that ended." );
		}

		/// <summary>
		/// Get the current choices for this dialogue
		/// </summary>
		/// <returns></returns>
		public string[] GetChoices()
		{
			return m_currentStorylet.Story.currentChoices.Select( choice => choice.text ).ToArray();
		}

		/// <summary>
		/// Make a choice
		/// </summary>
		/// <param name="choiceIndex"></param>
		public void MakeChoice(int choiceIndex)
		{
			m_currentStorylet.Story.ChooseChoiceIndex( choiceIndex );
		}

		/// <summary>
		/// Check if the dialogue manager is waiting for the player to make a choice
		/// </summary>
		/// <returns></returns>
		public bool HasChoices()
		{
			return m_currentStorylet.Story.currentChoices.Count > 0;
		}

		/// <summary>
		/// Check if the dialogue can continue further.
		/// </summary>
		/// <returns></returns>
		public bool CanContinue()
		{
			// Cannot continue if waiting for input
			if ( IsWaitingForInput ) return false;

			// The story can continue if the current storylet can continue
			if ( m_currentStorylet.Story.canContinue ) return true;

			// We cannot continue if there are choices.
			if ( HasChoices() ) return false;

			// If the current story can continue and it does not have any choices,
			// check if there is a storylet on deck to transition to
			if ( !HasChoices() && m_storyletOnDeck != null ) return true;

			return false;
		}

		/// <summary>
		/// Progress the story from the given storylet instance.
		/// </summary>
		/// <param name="instance"></param>
		public void RunStoryletInstance(StoryletInstance instance)
		{
			m_currentStorylet = instance;

			m_boundStoryletInstance = instance;

			// Decrement the cooldowns of all storylets
			foreach ( Storylet storylet in m_basicStorylets.Values )
			{
				storylet.DecrementCooldown();
			}

			m_currentStorylet.Storylet.IncrementTimesPlayed();

			m_currentStorylet.Storylet.ResetCooldown();

			m_currentStorylet.BindInstanceVariables();

			m_currentStorylet.Story.ChoosePathString( m_currentStorylet.KnotID );

			if ( !IsDisplayingDialogue )
			{
				IsDisplayingDialogue = true;

				OnDialogueStart?.Invoke();
			}
		}

		/// <summary>
		/// End the current dialogue and let the player select another action or location.
		/// </summary>
		public void EndDialogue()
		{
			IsDisplayingDialogue = false;
			IsWaitingForInput = false;

			OnDialogueEnd?.Invoke();
		}

		/// <summary>
		/// Provide input if the system is waiting for input.
		/// </summary>
		public void SetInput(string variableName, object input)
		{
			IsWaitingForInput = false;
			InputRequest = null;
			this.m_story.state.variablesState[variableName] = input;
			AdvanceDialogue();
		}

		/// <summary>
		/// Start the story.
		/// </summary>
		public void StartStory()
		{
			// Generally, the starting storylet should not contain a query, but if it does, then
			// there is the possibility that we will fail to create any storylet instances.
			// This is a bug that designers should figure out and is not a fault of Anansi.
			List<StoryletInstance> startInstances = CreateStoryletInstances(
				m_startingStorylet,
				DB,
				new Dictionary<string, object>()
			);

			if ( startInstances.Count == 0 )
			{
				throw new Exception( "Failed to create instance of starting storylet." );
			}

			// These should all be weighted the same, but this code will remain incase future
			// implementations allow for instances of the same storylet to have varying weights
			StoryletInstance selectedInstance = startInstances
				.RandomElementByWeight( s => s.Weight );

			RunStoryletInstance( selectedInstance );
		}

		/// <summary>
		/// Get a storylet by it's ID
		/// </summary>
		/// <param name="storyletId"></param>
		/// <returns></returns>
		public Storylet GetStorylet(string storyletId)
		{
			return this.m_basicStorylets[storyletId];
		}

		/// <summary>
		/// Returns all storylets satisfying the given tags.
		/// </summary>
		/// <param name="tags"></param>
		/// <returns></returns>
		public List<Storylet> GetStoryletsWithTags(IEnumerable<string> tags)
		{
			List<(Storylet, int)> matches = new List<(Storylet, int)>();
			int maxMatchScore = 0;

			HashSet<string> mandatoryTags = new HashSet<string>( tags.Where( t => t[0] != '~' ) );
			HashSet<string> optionalTags = new HashSet<string>( tags.Where( t => t[0] == '~' ) );

			foreach ( var storylet in m_basicStorylets.Values )
			{
				var unsatisfiedMandatoryTags = mandatoryTags.Except( storylet.Tags );
				var hasAllMandatoryTags = unsatisfiedMandatoryTags.Count() == 0;

				if ( !hasAllMandatoryTags )
				{
					continue;
				}

				var satisfiedOptionalTags = optionalTags.Intersect( storylet.Tags );
				var optionalTagsCount = satisfiedOptionalTags.Count();

				matches.Add( (storylet, optionalTagsCount) );

				maxMatchScore = Math.Max( optionalTagsCount, maxMatchScore );
			}

			if ( matches.Count > 0 )
			{
				matches.Sort( (a, b) =>
				{
					if ( a.Item2 > b.Item2 )
					{
						return 1;
					}
					else if ( a.Item2 == b.Item2 )
					{
						return 0;
					}
					else
					{
						return -1;
					}
				} );

				List<Storylet> bestMatches = matches
					.Where( entry => entry.Item2 == maxMatchScore )
					.Select( entry => entry.Item1 )
					.ToList();

				return bestMatches;
			}

			return new List<Storylet>();
		}

		/// <summary>
		/// Create storylet instances for a given storylet.
		/// </summary>
		/// <param name="storylet"></param>
		/// <param name="db"></param>
		/// <param name="bindings"></param>
		/// <returns></returns>
		public List<StoryletInstance> CreateStoryletInstances(
			Storylet storylet,
			RePraxisDatabase db,
			Dictionary<string, object> bindings
		)
		{
			Dictionary<string, object> inputBindings = new Dictionary<string, object>( bindings );

			foreach ( var (inkVar, queryVar) in storylet.InputBindings )
			{
				inputBindings[queryVar.ToString()] = m_story.variablesState[inkVar].ToString();
			}

			List<StoryletInstance> instances = new List<StoryletInstance>();

			if ( storylet.Precondition != null )
			{
				var results = storylet.Precondition.Run( db, inputBindings );

				if ( results.Success )
				{
					foreach ( var bindingDict in results.Bindings )
					{
						var instance = new StoryletInstance(
							storylet,
							bindingDict,
							storylet.Weight
						);

						if ( storylet.WeightFunctionName != null )
						{
							m_boundStoryletInstance = instance;

							instance.Weight = (int)m_story.EvaluateFunction(
								storylet.WeightFunctionName
							);
						}

						this.OnScoreStoryletInstance?.Invoke( instance );

						instances.Add( instance );
					}
				}
			}
			else
			{
				var instance = new StoryletInstance(
					storylet,
					inputBindings,
					storylet.Weight
				);

				if ( storylet.WeightFunctionName != null )
				{
					m_boundStoryletInstance = instance;

					instance.Weight = (int)m_story.EvaluateFunction(
						storylet.WeightFunctionName
					);
				}

				this.OnScoreStoryletInstance?.Invoke( instance );

				instances.Add( instance );
			}

			return instances;
		}

		public void AddDynamicChoice(StoryletInstance instance)
		{
			if ( !m_dynamicChoiceIds.Contains( instance.KnotID ) )
			{
				m_dynamicChoices.Add( instance );
				m_dynamicChoiceIds.Add( instance.KnotID );
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Extract all the storylets from the controller's story instance.
		/// </summary>
		private void LoadStorylets()
		{
			List<string> knotIDs = GetAllKnotIDs( m_story );
			bool hasStartStorylet = false;

			// Loop through all the knots, create storylets, and separate
			// them based on basic storylets, actions, and locations.
			foreach ( string knotID in knotIDs )
			{
				if ( knotID == "start" )
				{
					// This is our entry storylet
					hasStartStorylet = true;
					var storylet = CreateStorylet( m_story, knotID, StoryletType.Basic );
					m_basicStorylets[knotID] = storylet;
					m_startingStorylet = storylet;
				}
				else if ( knotID.StartsWith( STORYLET_ID_PREFIX ) )
				{
					// This is a basic storylet
					var storylet = CreateStorylet( m_story, knotID, StoryletType.Basic );
					m_basicStorylets[knotID] = storylet;
				}
				else if ( knotID.StartsWith( LOCATION_ID_PREFIX ) )
				{
					// This storylet corresponds to a location
					var storylet = CreateStorylet( m_story, knotID, StoryletType.Location );
					m_locationStorylets[knotID] = storylet;
					storylet.LocationID = knotID.Substring( LOCATION_ID_PREFIX.Length );
				}
				else if ( knotID.StartsWith( ACTION_ID_PREFIX ) )
				{
					// This storylet corresponds to an action
					var storylet = CreateStorylet( m_story, knotID, StoryletType.Action );
					m_actionStorylets[knotID] = storylet;
					storylet.ActionID = knotID.Substring( ACTION_ID_PREFIX.Length );
				}
			}

			// Raise an error if the starting storylet is never found.
			// If it is missing, we cant start the story.
			if ( !hasStartStorylet )
			{
				Debug.LogError( "Could not find entry knot with name 'start'." );
			}
		}

		/// <summary>
		/// Create a storylet for a knot
		/// </summary>
		/// <param name="story"></param>
		/// <param name="knotID"></param>
		/// <returns></returns>
		private static Storylet CreateStorylet(
			Ink.Runtime.Story story,
			string knotID,
			StoryletType storyletType
		)
		{
			Storylet storylet = new Storylet( knotID, story, storyletType );

			// storylet.VariableSubstitutions["speaker"] = "?speaker";
			// storylet.VariableSubstitutions["player"] = "?player";
			// storylet.VariableSubstitutions["location"] = "?location";

			List<string> knotTags = story.TagsForContentAtPath( knotID );

			if ( knotTags == null ) return storylet;

			bool foundOpeningTag = false;
			int lineIndex = 0;

			// Loop through the queue until it is empty
			while ( lineIndex < knotTags.Count )
			{
				// Get the next tag line
				string line = knotTags[lineIndex].Trim();

				// Check if this line is the opening tag line
				if ( line == "---" )
				{
					if ( foundOpeningTag )
					{
						throw new System.ArgumentException(
							$"Found duplicate opening tag '---' in {knotID}."
						);
					}
					foundOpeningTag = true;
					lineIndex++;
					continue;
				}

				// Skip any tag lines that come before the opening tag
				if ( !foundOpeningTag )
				{
					lineIndex++;
					continue;
				}

				// Check if this line is the closing tag and break the loop if it is
				if ( line == "===" )
				{
					break;
				}

				// This line specifies tags for storylet filtering
				if ( line.StartsWith( "tags:" ) )
				{
					List<string> storyletTags = line.Substring( "tags:".Length )
						.Split( "," ).Select( s => s.Trim() ).ToList();

					foreach ( string t in storyletTags )
					{
						storylet.Tags.Add( t );
					}

					lineIndex++;
					continue;
				}

				// This line specifies IDs of connected locations
				if ( storyletType == StoryletType.Location && line.StartsWith( "connectedLocations:" ) )
				{
					List<string> arguments = line.Substring( "connectedLocations:".Length )
						.Split( "," ).Select( s => s.Trim() ).ToList();

					foreach ( string locationId in arguments )
					{
						storylet.ConnectedLocations.Add( locationId );
					}

					lineIndex++;
					continue;
				}

				// This line specifies IDs of locations where an action may be selected
				if ( storyletType == StoryletType.Action && line.StartsWith( "eligibleLocs:" ) )
				{
					List<string> arguments = line.Substring( "eligibleLocs:".Length )
						.Split( "," ).Select( s => s.Trim() ).ToList();

					foreach ( string locationId in arguments )
					{
						storylet.EligibleLocations.Add( locationId );
					}

					lineIndex++;
					continue;
				}

				// This line specifies IDs of connected locations
				if ( line.StartsWith( "choiceLabel:" ) )
				{
					string label = line.Substring( "choiceLabel:".Length );

					storylet.ChoiceLabel = label;

					lineIndex++;
					continue;
				}

				// This line specifies an ink variable to set from a query variable
				if ( line.StartsWith( "@using" ) )
				{
					List<string> arguments = line.Substring( "@using".Length )
						.Trim().Split( " " ).Select( s => s.Trim() ).ToList();

					if ( arguments.Count != 3 )
					{
						throw new ArgumentException(
							$"Invalid Using-expression '{line}' in knot '{knotID}'. "
							+ "Using expression must be of the form `@using X as ?Y"
						);
					}

					storylet.InputBindings[arguments[0]] = arguments[2];

					lineIndex++;
					continue;
				}

				// This line specifies an ink variable to set from a query variable
				if ( line.StartsWith( "@set" ) )
				{
					List<string> arguments = line.Substring( "@set".Length )
						.Trim().Split( " " ).Select( s => s.Trim() ).ToList();

					if ( arguments.Count != 3 )
					{
						throw new ArgumentException(
							$"Invalid Set-expression '{line}' in knot '{knotID}'. "
							+ "Set expression must be of the form `@set X to Y"
						);
					}

					storylet.VariableSubstitutions[arguments[0]] = arguments[2];

					lineIndex++;
					continue;
				}

				// This line specifies the relative weight of this story let during selection
				if ( line.StartsWith( "weight:" ) )
				{
					List<string> arguments = line.Substring( "weight:".Length )
						.Split( "," ).Select( s => s.Trim() ).ToList();

					if ( int.TryParse( arguments[0], out var weight ) )
					{
						if ( weight < 0 )
						{
							throw new ArgumentException(
								$"Invalid value for weight in '{line}' of knot '{knotID}'. "
								+ "Acceptable values are integers greater than or equal to zero."
							);
						}

						storylet.Weight = weight;
					}
					else
					{
						storylet.WeightFunctionName = arguments[0];
					}

					lineIndex++;
					continue;
				}

				// This line if this storylet has a cooldown time
				if ( line.StartsWith( "cooldown:" ) )
				{
					List<string> arguments = line.Substring( "cooldown:".Length )
						.Split( "," ).Select( s => s.Trim() ).ToList();

					if ( !int.TryParse( arguments[0], out var cooldown ) )
					{
						throw new ArgumentException(
							$"Invalid value for cooldown in '{line}' of knot '{knotID}'. "
							+ "Acceptable values are integers greater than or equal to zero."
						);
					}

					storylet.Cooldown = cooldown;

					lineIndex++;
					continue;
				}

				// This line specifies specifies if the story let may be repeated more than once
				if ( line.StartsWith( "repeatable:" ) )
				{
					List<string> arguments = line.Substring( "repeatable:".Length )
						.Split( "," ).Select( s => s.Trim() ).ToList();

					if ( !bool.TryParse( arguments[0], out var isRepeatable ) )
					{
						throw new ArgumentException(
							$"Invalid value for isRepeatable in '{line}' of knot '{knotID}'. "
							+ "Acceptable values are 'true' or 'false'"
						);
					}

					storylet.IsRepeatable = isRepeatable;

					lineIndex++;
					continue;
				}

				// This line specifies tags for storylet filtering
				if ( line.StartsWith( "mandatory:" ) )
				{
					List<string> arguments = line.Substring( "weight:".Length )
						.Split( "," ).Select( s => s.Trim() ).ToList();

					if ( !bool.TryParse( arguments[0], out var isMandatory ) )
					{
						throw new ArgumentException(
							$"Invalid value for isMandatory in '{line}' of knot '{knotID}'. "
							+ "Acceptable values are 'true' or 'false'"
						);
					}

					storylet.IsMandatory = isMandatory;

					lineIndex++;
					continue;
				}

				// This is the first line declaring a query
				if ( line.StartsWith( "@query" ) )
				{
					storylet.Precondition = PreconditionQueryFromTags(
						knotID, knotTags, lineIndex, out lineIndex );
				}

				lineIndex++;
				continue;
			}

			return storylet;
		}

		/// <summary>
		/// Construct a RePraxis query from tags in a storylet header
		/// </summary>
		/// <param name="knotID"></param>
		/// <param name="tags"></param>
		/// <param name="startingIndex"></param>
		/// <param name="lineIndex"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		private static DBQuery PreconditionQueryFromTags(
			string knotID, List<string> tags, int startingIndex, out int lineIndex)
		{
			List<string> queryLines = new List<string>();
			bool endReached = false;

			int i = startingIndex + 1;

			while ( i < tags.Count )
			{
				string line = tags[i].Trim();

				if ( line.StartsWith( "@end" ) )
				{
					endReached = true;
					break;
				}

				queryLines.Add( line );
				i++;
			}

			if ( !endReached )
			{
				throw new ArgumentException(
					$"Missing 'end >>' statement in precondition query for knot '{knotID}'"
				);
			}

			lineIndex = i;
			return new DBQuery( queryLines );
		}

		/// <summary>
		/// Retrieve all the knot IDs from an Ink Story instance
		/// <remark>
		/// This function depends on Ink version 1.1.7
		/// </remark>
		/// </summary>
		/// <param name="story"></param>
		/// <returns></returns>
		private static List<string> GetAllKnotIDs(Ink.Runtime.Story story)
		{
			List<string> knotList = new List<string>();

			Ink.Runtime.Container mainContentContainer = story.mainContentContainer;
			if ( mainContentContainer == null )
				return knotList;

			foreach ( string name in mainContentContainer.namedOnlyContent.Keys )
			{
				// Don't want this as it's Ink internal
				if ( name == "global decl" )
					continue;

				knotList.Add( name );
			}

			return knotList;
		}

		/// <summary>
		/// Log story errors
		/// </summary>
		/// <param name="message"></param>
		/// <param name="errorType"></param>
		private void HandleStoryErrors(string message, Ink.ErrorType errorType)
		{
			if ( errorType == Ink.ErrorType.Warning )
			{
				Debug.LogWarning( message );
			}
			else
			{
				Debug.LogError( message );
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
				OnSpeakerChange?.Invoke( null );
				return line;
			}
			else
			{
				List<string> speakerSpec = match.Groups[1].Value
					.Split( "." ).Select( s => s.Trim() ).ToList();

				string speakerId = speakerSpec[0];
				speakerSpec.RemoveAt( 0 );
				string[] speakerTags = speakerSpec.ToArray();

				OnSpeakerChange?.Invoke( new SpeakerInfo( speakerId, speakerTags ) );

				string dialogueText = match.Groups[2].Value.Trim();

				return dialogueText;
			}
		}

		/// <summary>
		/// Process a list of Ink tags
		/// </summary>
		/// <param name="tags"></param>
		private void ProcessTags(IList<string> tags)
		{
			bool foundOpeningTag = false;
			bool foundClosingTag = false;

			// Sometimes the line may be the first in the knot. When this happens, the
			// lines tags are mixed with the knots tags. We have to ignore all tags
			// that come between the opening and closing markers.
			List<string> filteredTags = new List<string>();
			if ( tags != null )
			{
				foreach ( string line in tags )
				{
					if ( foundOpeningTag && !foundClosingTag )
					{
						continue;
					}

					if ( line.StartsWith( "---" ) )
					{
						foundOpeningTag = true;
						continue;
					}

					if ( line.StartsWith( "===" ) )
					{
						foundClosingTag = true;
						continue;
					}

					filteredTags.Add( line );
				}
			}

			LineTags = filteredTags;
			OnProcessLineTags?.Invoke( filteredTags );
		}

		/// <summary>
		/// Register external functions with the Ink story.
		/// </summary>
		private void RegisterExternalFunctions()
		{
			m_story.BindExternalFunction(
				"DbInsert",
				(string statement) =>
				{
					DB.Insert( statement );
				}
			);

			m_story.BindExternalFunction(
				"DbDelete",
				(string statement) =>
				{
					return DB.Delete( statement );
				}
			);

			m_story.BindExternalFunction(
				"DbAssert",
				(string statement) =>
				{
					return DB.Assert( statement );
				}
			);

			m_story.BindExternalFunction(
				"QueueStorylet",
				(string storyletId) =>
				{
					Storylet storylet = m_basicStorylets[storyletId];

					List<StoryletInstance> instances = CreateStoryletInstances(
						storylet,
						DB,
						new Dictionary<string, object>()
					);

					if ( instances.Count == 0 ) return;

					StoryletInstance selectedInstance = instances
						.RandomElementByWeight( s => s.Weight );

					m_storyletOnDeck = selectedInstance;
				}
			);

			m_story.BindExternalFunction(
				"QueueStoryletWithTags",
				(string tags, string fallback) =>
				{
					IEnumerable<string> tagList = tags.Split( "," ).Select( t => t.Trim() );

					IEnumerable<Storylet> storyletsWithTags = GetStoryletsWithTags( tagList )
						.Where( storylet => storylet.IsEligible );

					List<StoryletInstance> storyletInstances = new List<StoryletInstance>();
					List<StoryletInstance> mandatoryStorylets = new List<StoryletInstance>();

					foreach ( Storylet storylet in storyletsWithTags )
					{
						List<StoryletInstance> instances = CreateStoryletInstances(
							storylet,
							DB,
							new Dictionary<string, object>()
						);

						foreach ( var entry in instances )
						{
							storyletInstances.Add( entry );

							if ( storylet.IsMandatory )
							{
								mandatoryStorylets.Add( entry );
							}
						}
					}

					if ( mandatoryStorylets.Count > 0 )
					{
						StoryletInstance selectedInstance = mandatoryStorylets
							.RandomElementByWeight( s => s.Weight );

						m_storyletOnDeck = selectedInstance;
					}
					else if ( storyletInstances.Count > 0 )
					{
						StoryletInstance selectedInstance = storyletInstances
							.RandomElementByWeight( s => s.Weight );

						m_storyletOnDeck = selectedInstance;
					}
					else
					{
						Storylet fallbackStorylet = m_basicStorylets[fallback];

						List<StoryletInstance> instances = CreateStoryletInstances(
							fallbackStorylet,
							DB,
							new Dictionary<string, object>()
						);

						if ( instances.Count == 0 )
						{
							throw new Exception( "Could not create instance of fallback storylet" );
						}

						StoryletInstance selectedInstance = instances
							.RandomElementByWeight( s => s.Weight );

						m_storyletOnDeck = selectedInstance;
					}
				}
			);

			m_story.BindExternalFunction(
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

			m_story.BindExternalFunction(
				"GetBindingVar",
				(string variableName) =>
				{
					return m_boundStoryletInstance.PreconditionBindings[variableName];
				}
			);

			m_story.BindExternalFunction(
				"GoToChoice",
				(string storyletId) =>
				{
					Storylet storylet = m_basicStorylets[storyletId];

					List<StoryletInstance> instances = CreateStoryletInstances(
						storylet,
						DB,
						new Dictionary<string, object>()
					);

					if ( instances.Count == 0 ) return;

					StoryletInstance selectedInstance = instances
						.RandomElementByWeight( s => s.Weight );

					m_dynamicChoices.Add( selectedInstance );
					m_dynamicChoiceIds.Add( selectedInstance.KnotID );
				}
			);

			m_story.BindExternalFunction(
				"ChoiceWithTags",
				(string tags) =>
				{
					IEnumerable<string> tagList = tags.Split( "," ).Select( t => t.Trim() );

					IEnumerable<Storylet> storyletsWithTags = GetStoryletsWithTags( tagList )
						.Where( storylet => storylet.IsEligible );

					List<StoryletInstance> storyletInstances = new List<StoryletInstance>();
					List<StoryletInstance> mandatoryStorylets = new List<StoryletInstance>();

					foreach ( Storylet storylet in storyletsWithTags )
					{
						if ( m_dynamicChoiceIds.Contains( storylet.KnotID ) ) continue;

						List<StoryletInstance> instances = CreateStoryletInstances(
							storylet,
							DB,
							new Dictionary<string, object>()
						);

						foreach ( var entry in instances )
						{
							storyletInstances.Add( entry );

							if ( storylet.IsMandatory )
							{
								mandatoryStorylets.Add( entry );
							}
						}
					}

					if ( mandatoryStorylets.Count > 0 )
					{
						StoryletInstance selectedInstance = mandatoryStorylets
							.RandomElementByWeight( s => s.Weight );

						m_dynamicChoices.Add( selectedInstance );
						m_dynamicChoiceIds.Add( selectedInstance.KnotID );
					}
					else if ( storyletInstances.Count > 0 )
					{
						StoryletInstance selectedInstance = storyletInstances
							.RandomElementByWeight( s => s.Weight );

						m_dynamicChoices.Add( selectedInstance );
						m_dynamicChoiceIds.Add( selectedInstance.KnotID );
					}
				}
			);

			// Load functions from external classes.
			OnRegisterExternalFunctions?.Invoke( m_story );
		}

		#endregion
	}

	public enum InputDataType
	{
		String,
		Int,
		Float
	}

	public class InputRequest
	{
		public string Prompt { get; }
		public string VariableName { get; }
		public InputDataType DataType { get; }

		public InputRequest(string prompt, string variableName, InputDataType dataType)
		{
			Prompt = prompt;
			VariableName = variableName;
			DataType = dataType;
		}
	}

	public class SpeakerInfo
	{
		public string SpeakerId { get; }
		public string[] Tags { get; }

		public SpeakerInfo(string speakerId, string[] tags)
		{
			this.SpeakerId = speakerId;
			this.Tags = tags;
		}
	}
}
