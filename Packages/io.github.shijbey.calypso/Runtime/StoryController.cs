using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RePraxis;
using UnityEngine.Events;
using Calypso.Scheduling;
using TDRS;

namespace Calypso
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
		/// A reference to the player's character.
		/// </summary>
		[SerializeField]
		protected Character m_player;

		/// <summary>
		/// Manages a lookup table for all characters in the game.
		/// </summary>
		[SerializeField]
		private CharacterManager m_characterManager;

		/// <summary>
		/// Manages a look-up table for all the locations in the game.
		/// </summary>
		[SerializeField]
		private LocationManager m_locationManager;

		/// <summary>
		/// Manages the current time in the game.
		/// </summary>
		[SerializeField]
		private TimeManager m_timeManager;

		/// <summary>
		/// Manages character relationships and all information in the logical database.
		/// </summary>
		[SerializeField]
		private SocialEngine m_socialEngine;

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
		private List<Storylet> m_basicStorylets;

		/// <summary>
		/// All storylets related to locations on the map.
		/// </summary>
		private List<Storylet> m_locationStorylets;

		/// <summary>
		/// All storylets related to actions the player can take at locations.
		/// </summary>
		private List<Storylet> m_actionStorylets;

		/// <summary>
		/// The storylet instance being presented to the player.
		/// </summary>
		private StoryletInstance m_storyletInstance = null;

		#endregion

		#region Properties

		/// <summary>
		/// The database used for storylet queries.
		/// </summary>
		public RePraxisDatabase DB => m_socialEngine.DB;

		/// <summary>
		/// Is there currently dialogue being displayed on the screen.
		/// </summary>
		public bool IsDialogueActive { get; private set; }

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
		/// Action invoked when the location of the story changes
		/// </summary>
		public UnityAction<Location> OnStoryLocationChange;

		/// <summary>
		/// Action invoked when the current speaker changes
		/// </summary>
		public UnityAction<Character> OnSpeakerChange;

		#endregion

		#region Unity Messages

		private void Awake()
		{
			if ( m_storyJson == null )
			{
				throw new NullReferenceException( "StoryController is missing storyJSON" );
			}

			m_startingStorylet = null;
			m_basicStorylets = new List<Storylet>();
			m_locationStorylets = new List<Storylet>();
			m_actionStorylets = new List<Storylet>();
			m_story = new Ink.Runtime.Story( m_storyJson.text );

			LoadStorylets();
		}

		public void OnEnable()
		{
			m_story.onError += HandleStoryErrors;
		}

		public void OnDisable()
		{
			m_story.onError -= HandleStoryErrors;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Called by the GameManager when we are ready to start the game.
		/// </summary>
		public void Initialize()
		{
			RegisterExternalFunctions();

			m_characterManager.InitializeLookUpTable();
			m_locationManager.InitializeLookUpTable();

			// Insert initial character entry into database
			foreach ( var character in m_characterManager.Characters )
			{
				DB.Insert( $"{character.UniqueID}" );
			}

			// Insert initial location entry into the database
			foreach ( var location in m_locationManager.Locations )
			{
				DB.Insert( $"{location.UniqueID}" );
			}

			// Reposition character and background sprites
			m_locationManager.ResetBackgrounds();
			m_characterManager.ResetSprites();

			// Add the current date to the social engine's database
			SimDateTime currentDate = m_timeManager.DateTime;

			DB.Insert(
				$"date.time_of_day!{Enum.GetName( typeof( TimeOfDay ), currentDate.TimeOfDay )}" );
			DB.Insert(
				$"date.day!{currentDate.Day}" );
			DB.Insert(
				$"date.weekday!{Enum.GetName( typeof( WeekDay ), currentDate.WeekDay )}" );
			DB.Insert(
				$"date.week!{currentDate.Week}" );

			// Move NPCs to their starting positions
			TickCharacterSchedules();
		}

		public void StartStory()
		{
			// Generally, the starting storylet should not contain a query, but if it does, then
			// there is the possibility that we will fail to create any storylet instances.
			// This is a bug that designers should figure out and is not a fault of Calypso.
			List<StoryletInstance> startInstances = CreateStoryletInstances(
				m_startingStorylet,
				DB,
				new Dictionary<string, string>()
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
		/// Tick the simulation
		/// </summary>
		public void Tick()
		{
			TickTime();
			TickCharacterSchedules();
		}

		/// <summary>
		/// Update advance the current time by one step.
		/// </summary>
		public void TickTime()
		{
			m_timeManager.AdvanceTime();

			SimDateTime currentDate = m_timeManager.DateTime;

			DB.Insert(
				$"date.time_of_day!{Enum.GetName( typeof( TimeOfDay ), currentDate.TimeOfDay )}" );
			DB.Insert(
				$"date.day!{currentDate.Day}" );
			DB.Insert(
				$"date.weekday!{Enum.GetName( typeof( WeekDay ), currentDate.WeekDay )}" );
			DB.Insert(
				$"date.week!{currentDate.Week}" );
		}

		/// <summary>
		/// Update character states based on what their schedules dictate.
		/// </summary>
		public void TickCharacterSchedules()
		{
			SimDateTime currentDate = m_timeManager.DateTime;

			foreach ( Character character in m_characterManager.Characters )
			{
				if ( character == m_player ) continue;

				var scheduleManager = character.gameObject.GetComponent<ScheduleManager>();

				ScheduleEntry entry = scheduleManager.GetEntry( currentDate );

				if ( entry == null ) continue;

				SetCharacterLocation( character, m_locationManager.GetLocation( entry.Location ) );
			}
		}

		/// <summary>
		/// Sets the games background image to the given location
		/// </summary>
		/// <param name="location"></param>
		public void SetStoryLocation(string locationID, params string[] tags)
		{
			m_locationManager.SetBackground( locationID, tags );
		}

		/// <summary>
		/// Move an Actor to a new location.
		/// </summary>
		/// <param name="location"></param>
		public void SetCharacterLocation(Character character, Location location)
		{
			// Remove the character from their current location
			if ( character.Location != null )
			{
				character.Location.RemoveCharacter( character );
				DB.Delete(
					$"{character.Location.UniqueID}.characters.{character.UniqueID}" );
				DB.Delete(
					$"{character.UniqueID}.location.{character.Location.UniqueID}" );
				character.Location = null;
			}

			if ( location != null )
			{
				location.AddCharacter( character );
				character.Location = location;
				DB.Insert( $"{location.UniqueID}.characters.{character.UniqueID}" );
				DB.Insert( $"{character.UniqueID}.location.{location.UniqueID}" );
			}
		}

		/// <summary>
		/// Set the player's current location and change the background
		/// </summary>
		/// <param name="location"></param>
		public void SetPlayerLocation(Location location)
		{
			if ( m_player.Location != location )
			{
				SetCharacterLocation( m_player, location );
			}

			if ( location == null )
			{
				m_characterManager.HideSpeaker();
				return;
			}

			SetStoryLocation( location.UniqueID );
		}

		/// <summary>
		/// Set the speaker sprite shown on screen.
		/// </summary>
		/// <param name="characterID"></param>
		/// <param name="tags"></param>
		public void SetSpeaker(string characterID, params string[] tags)
		{
			var character = m_characterManager.GetCharacter( characterID );
			m_characterManager.SetSpeaker( characterID, tags );
		}

		/// <summary>
		/// Get the next line of dialogue
		/// </summary>
		/// <returns></returns>
		public string GetNextLine()
		{
			// Get the next line of text from Ink
			string line = m_storyletInstance.Story.Continue();

			line = PreProcessDialogueLine( line );

			// Process tags
			ProcessTags( m_storyletInstance.Story.currentTags );

			return line;
		}

		/// <summary>
		/// Get the current choices for this dialogue
		/// </summary>
		/// <returns></returns>
		public string[] GetChoices()
		{
			return m_storyletInstance.Story.currentChoices.Select( choice => choice.text ).ToArray();
		}

		/// <summary>
		/// Make a choice
		/// </summary>
		/// <param name="choiceIndex"></param>
		public void MakeChoice(int choiceIndex)
		{
			m_storyletInstance.Story.ChooseChoiceIndex( choiceIndex );
		}

		/// <summary>
		/// Check if the dialogue manager is waiting for the player to make a choice
		/// </summary>
		/// <returns></returns>
		public bool HasChoices()
		{
			return m_storyletInstance.Story.currentChoices.Count > 0;
		}

		/// <summary>
		/// Check if the dialogue can continue further
		/// </summary>
		/// <returns></returns>
		public bool CanContinue()
		{
			return m_storyletInstance.Story.canContinue;
		}

		/// <summary>
		/// Check if the dialogue has reached its end
		/// </summary>
		/// <returns></returns>
		public bool AtDialogueEnd()
		{
			return !CanContinue() && !HasChoices();
		}

		public void RunStoryletInstance(StoryletInstance instance)
		{
			m_storyletInstance = instance;

			m_storyletInstance.Storylet.IncrementTimesPlayed();

			m_storyletInstance.BindInstanceVariables();

			m_storyletInstance.Story.ChoosePathString( m_storyletInstance.KnotID );

			IsDialogueActive = true;

			OnDialogueStart?.Invoke();
		}

		public void EndDialogue()
		{
			IsDialogueActive = false;

			OnDialogueEnd?.Invoke();
		}

		public List<StoryletInstance> GetEligibleLocationStorylets()
		{
			List<StoryletInstance> instances = new List<StoryletInstance>();

			foreach ( var storylet in m_locationStorylets )
			{
				// Skip storylets still on cooldown
				if ( storylet.CooldownTimeRemaining > 0 ) continue;

				// Skip storylets that are not repeatable
				if ( !storylet.IsRepeatable && storylet.TimesPlayed > 0 ) continue;

				// Query the social engine database
				if ( storylet.Precondition != null )
				{
					var results = storylet.Precondition.Run( DB );

					if ( !results.Success ) continue;

					foreach ( var bindingDict in results.Bindings )
					{
						instances.Add( new StoryletInstance(
						storylet,
						bindingDict,
						storylet.Weight
					) );
					}
				}
				else
				{
					instances.Add( new StoryletInstance(
						storylet,
						new Dictionary<string, string>(),
						storylet.Weight
					) );
				}
			}

			return instances;
		}

		public List<StoryletInstance> GetEligibleActionStorylets()
		{
			List<StoryletInstance> instances = new List<StoryletInstance>();

			foreach ( var storylet in m_actionStorylets )
			{
				// Skip storylets still on cooldown
				if ( storylet.CooldownTimeRemaining > 0 ) continue;

				// Skip storylets that are not repeatable
				if ( !storylet.IsRepeatable && storylet.TimesPlayed > 0 ) continue;

				// Query the social engine database
				if ( storylet.Precondition != null )
				{
					var results = storylet.Precondition.Run( DB );

					if ( !results.Success ) continue;

					foreach ( var bindingDict in results.Bindings )
					{
						instances.Add( new StoryletInstance(
						storylet,
						bindingDict,
						storylet.Weight
					) );
					}
				}
				else
				{
					instances.Add( new StoryletInstance(
						storylet,
						new Dictionary<string, string>(),
						storylet.Weight
					) );
				}
			}

			return instances;
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
					m_basicStorylets.Add( storylet );
					m_startingStorylet = storylet;
				}
				else if ( knotID.StartsWith( STORYLET_ID_PREFIX ) )
				{
					// This is a basic storylet
					string idNoPrefix = knotID.Substring( STORYLET_ID_PREFIX.Length );
					var storylet = CreateStorylet( m_story, knotID, StoryletType.Basic );
					m_basicStorylets.Add( storylet );
				}
				else if ( knotID.StartsWith( LOCATION_ID_PREFIX ) )
				{
					// This storylet corresponds to a location
					string idNoPrefix = knotID.Substring( LOCATION_ID_PREFIX.Length );
					var storylet = CreateStorylet( m_story, knotID, StoryletType.Location );
					m_locationStorylets.Add( storylet );
				}
				else if ( knotID.StartsWith( ACTION_ID_PREFIX ) )
				{
					// This storylet corresponds to an action
					string idNoPrefix = knotID.Substring( ACTION_ID_PREFIX.Length );
					var storylet = CreateStorylet( m_story, knotID, StoryletType.Action );
					m_actionStorylets.Add( storylet );
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
				if ( line.StartsWith( "@set" ) )
				{
					List<string> arguments = line.Substring( "@set".Length )
						.Split( " " ).Select( s => s.Trim() ).ToList();

					if ( arguments.Count != 3 )
					{
						throw new ArgumentException(
							$"Invalid Set-expression '{line}' in knot '{knotID}'. "
							+ "Set expression must be of the form `set >> X to Y"
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

					if ( !int.TryParse( arguments[0], out var weight ) )
					{
						throw new ArgumentException(
							$"Invalid value for weight in '{line}' of knot '{knotID}'. "
							+ "Acceptable values are integers greater than or equal to zero."
						);
					}

					storylet.Weight = weight;

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
					DBQuery precondition = PreconditionQueryFromTags(
						knotID, knotTags, lineIndex, out lineIndex );
				}
			}

			return storylet;
		}

		private static DBQuery PreconditionQueryFromTags(
			string knotID, List<string> tags, int startingIndex, out int lineIndex)
		{
			List<string> queryLines = new List<string>();
			bool endReached = false;

			int i = startingIndex;

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
		/// Create storylet instances for a given storylet.
		/// </summary>
		/// <param name="storylet"></param>
		/// <param name="db"></param>
		/// <param name="bindings"></param>
		/// <returns></returns>
		private List<StoryletInstance> CreateStoryletInstances(
			Storylet storylet,
			RePraxisDatabase db,
			Dictionary<string, string> bindings
		)
		{

			List<StoryletInstance> instances = new List<StoryletInstance>();

			if ( storylet.Precondition != null )
			{
				var results = storylet.Precondition.Run( db, bindings );

				if ( results.Success )
				{
					foreach ( var bindingDict in results.Bindings )
					{
						instances.Add(
							new StoryletInstance(
								storylet,
								bindingDict,
								storylet.Weight
							)
						);
					}
				}
			}
			else
			{
				instances.Add(
					new StoryletInstance(
						storylet,
						bindings,
						storylet.Weight
					)
				);
			}

			return instances;
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
			return line;
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

			foreach ( string line in tags )
			{
				// Get the different parts of the line
				// string[] parts = line.Split( ">>" ).Select( s => s.Trim() ).ToArray();

				// if ( parts.Length != 2 )
				// {
				// 	throw new System.ArgumentException(
				// 		$"Invalid expression '{line}' in knot '{m_storyletInstance.KnotID}'."
				// 	);
				// }

				// string command = parts[0];
				// List<string> arguments = parts[1].Split( " " )
				// 	.Select( s => s.Trim() ).ToList();

				// switch (command)
				// {
				//     case "speaker":
				//         string speakerID = arguments[0];

				//         if (m_currentSpeaker != speakerID)
				//         {
				//             m_currentSpeaker = speakerID;
				//             arguments.RemoveAt(0);
				//             string[] speakerTags = arguments.ToArray();
				//             m_gameManager.SetSpeaker(
				//                 speakerID, speakerTags
				//             );
				//         }

				//         break;
				// }

			}
		}

		/// <summary>
		/// Register external functions with the Ink story.
		/// </summary>
		private void RegisterExternalFunctions()
		{
			OnRegisterExternalFunctions?.Invoke( m_story );

			m_story.BindExternalFunction(
				"SetLocation",
				(string locationID, string tags) =>
				{
					string[] tagsArr = tags.Split( "," )
						.Select( s => s.Trim() )
						.Where( s => s != "" )
						.ToArray();

					SetStoryLocation( locationID, tagsArr );
				}
			);

			m_story.BindExternalFunction(
				"SetSpeaker",
				(string characterID, string tags) =>
				{
					string[] tagsArr = tags.Split( "," )
						.Select( s => s.Trim() )
						.Where( s => s != "" )
						.ToArray();

					SetSpeaker( characterID, tagsArr );
				}
			);
		}

		#endregion
	}
}
