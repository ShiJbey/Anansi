using System;
using System.Linq;
using System.Collections.Generic;
using RePraxis;

namespace Anansi
{
	/// <summary>
	/// This class manages the state of the story by wrapping an Ink story and providing
	/// infrastructure to support dynamic storylet-style content sequencing.
	/// </summary>
	public class Story
	{
		#region Fields

		/// <summary>
		/// A reference to the story constructed from the JSON data.
		/// </summary>
		private Ink.Runtime.Story m_story;

		/// <summary>
		/// All storylets related to moving the story forward.
		/// </summary>
		private Dictionary<string, Storylet> m_storylets;

		/// <summary>
		/// The storylet instance being presented to the player.
		/// </summary>
		private StoryletInstance m_currentStorylet;

		/// <summary>
		/// The next storylet to play after the current storylet concludes.
		/// </summary>
		private StoryletInstance m_storyletOnDeck;

		/// <summary>
		/// An internal cache of dynamic choices to present the player. This list is added to
		/// when we want to present storylet diverts as if they are native Ink choices.
		/// </summary>
		private List<Choice> m_dynamicChoices;

		/// <summary>
		/// The IDs of all the choices currently in the m_dynamic choices list.
		/// </summary>
		private HashSet<string> m_dynamicChoiceIds;

		/// <summary>
		/// This variable caches a reference to a storylet instance and is used by the
		/// GetBindingVar() external function when evaluating storylet instance weights.
		/// </summary>
		private StoryletInstance m_boundStoryletInstance;

		#endregion

		#region Properties

		/// <summary>
		/// The reference to the wrapped Ink story instance.
		/// </summary>
		public Ink.Runtime.Story InkStory => m_story;

		/// <summary>
		/// All tags belonging to the current line of dialogue.
		/// </summary>
		public List<string> CurrentTags { get; private set; }

		/// <summary>
		/// A list of choices current available at this point in the story.
		/// </summary>
		/// <value></value>
		public List<Choice> CurrentChoices
		{
			get
			{
				var choices = new List<Choice>();

				// Return any Ink-native choices first.
				var choicesFromInk = m_story.currentChoices;
				if ( choicesFromInk.Count() > 0 )
				{
					foreach ( var inkChoice in choicesFromInk )
					{
						choices.Add( new Choice( inkChoice ) );
					}
				}

				// Then try to return dynamic choices.
				if ( m_dynamicChoices.Count() > 0 )
				{
					foreach ( var storyletChoice in m_dynamicChoices )
					{
						choices.Add( storyletChoice );
					}
				}

				return choices;
			}
		}

		/// <summary>
		/// The database used for storylet queries.
		/// </summary>
		public RePraxisDatabase DB { get; set; }

		#endregion

		#region Events and Actions

		/// <summary>
		/// Allow external listeners to contribute the the weight of a storylet instance.
		/// </summary>
		public Action<StoryletInstance> OnScoreStoryletInstance;

		#endregion

		#region Constructors

		public Story(string storyJson)
		{
			DB = new RePraxisDatabase();
			m_storylets = new Dictionary<string, Storylet>();
			m_story = new Ink.Runtime.Story( storyJson );
			m_dynamicChoices = new List<Choice>();
			m_currentStorylet = null;
			m_storyletOnDeck = null;
			m_boundStoryletInstance = null;
			m_dynamicChoiceIds = new HashSet<string>();
			m_dynamicChoices = new List<Choice>();

			LoadStorylets();
			RegisterExternalFunctions();
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Progresses the story to the next line of content.
		/// </summary>
		/// <returns>The next line of content</returns>
		public string Continue()
		{
			// Follow the current storylet until it is exhausted
			if ( m_currentStorylet.InkStory.canContinue )
			{
				// Get the next line of text from Ink
				string line = m_currentStorylet.InkStory.Continue();

				// Process tags
				ExtractLineTags( m_currentStorylet.InkStory.currentTags );

				return line;
			}

			if ( m_storyletOnDeck != null )
			{
				GoToStoryletInstance( m_storyletOnDeck );
				m_storyletOnDeck = null;

				// Get the next line of text from Ink
				string line = m_currentStorylet.InkStory.Continue();

				ExtractLineTags( m_currentStorylet.InkStory.currentTags );

				return line;
			}

			throw new Exception( "Cannot get next line of dialogue that ended." );
		}

		/// <summary>
		/// Make a choice
		/// </summary>
		/// <param name="choiceIndex"></param>
		public void ChooseChoiceIndex(int choiceIndex)
		{
			var choice = CurrentChoices[choiceIndex];

			if ( choice.InkChoice != null )
			{
				m_currentStorylet.InkStory.ChooseChoiceIndex( choice.InkChoice.index );
			}
			else if ( choice.StoryletInstance != null )
			{
				GoToStoryletInstance( choice.StoryletInstance );
			}

			m_dynamicChoices.Clear();
			m_dynamicChoiceIds.Clear();
		}

		/// <summary>
		/// Check if the dialogue manager is waiting for the player to make a choice
		/// </summary>
		/// <returns></returns>
		public bool HasChoices()
		{
			return CurrentChoices.Count > 0;
		}

		/// <summary>
		/// Check if the dialogue can continue further.
		/// </summary>
		/// <returns></returns>
		public bool CanContinue()
		{
			// The story can continue if the current storylet can continue
			if ( m_currentStorylet.InkStory.canContinue ) return true;

			// We cannot continue if there are choices.
			if ( HasChoices() ) return false;

			// If the current story cannot continue and it does not have any choices,
			// check if there is a storylet on deck to transition to
			if ( !HasChoices() && m_storyletOnDeck != null ) return true;

			return false;
		}

		/// <summary>
		/// Attempt to instantiate and run a storylet.
		/// </summary>
		/// <param name="storylet"></param>
		public void GoToStorylet(Storylet storylet)
		{
			// Generally, the starting storylet should not contain a query, but if it does, then
			// there is the possibility that we will fail to create any storylet instances.
			// This is a bug that designers should figure out and is not a fault of Anansi.
			List<StoryletInstance> startInstances = CreateStoryletInstances(
				storylet,
				new Dictionary<string, object>()
			);

			if ( startInstances.Count == 0 )
			{
				throw new Exception( $"Failed to create instance of storylet: {storylet.ID}." );
			}

			// These should all be weighted the same, but this code will remain incase future
			// implementations allow for instances of the same storylet to have varying weights
			StoryletInstance selectedInstance = startInstances
				.RandomElementByWeight( s => s.Weight );

			GoToStoryletInstance( selectedInstance );
		}

		/// <summary>
		/// Progress the story from the given storylet instance.
		/// </summary>
		/// <param name="instance"></param>
		public void GoToStoryletInstance(StoryletInstance instance)
		{
			m_currentStorylet = instance;

			m_boundStoryletInstance = instance;

			// Decrement the cooldowns of all storylets
			foreach ( Storylet storylet in m_storylets.Values )
			{
				storylet.DecrementCooldown();
			}

			m_currentStorylet.Storylet.IncrementTimesPlayed();

			m_currentStorylet.Storylet.ResetCooldown();

			// Set global variables from the preconditionBindings
			foreach ( var substitution in instance.Storylet.VariableSubstitutions )
			{
				m_story.variablesState[substitution.Key] =
					instance.PreconditionBindings[substitution.Value];
			}

			// Set get the knot arguments to the query output variables
			object[] knotArgs = new object[instance.Storylet.Precondition.OutputVars.Count];
			for ( int i = 0; i < instance.Storylet.Precondition.OutputVars.Count; i++ )
			{
				string varName = instance.Storylet.Precondition.OutputVars[i];
				knotArgs[i] = instance.PreconditionBindings[varName];
			}

			m_currentStorylet.InkStory.ChoosePathString(
				m_currentStorylet.KnotID, false, knotArgs );
		}

		/// <summary>
		/// Get a storylet by it's ID
		/// </summary>
		/// <param name="storyletId"></param>
		/// <returns></returns>
		public Storylet GetStorylet(string storyletId)
		{
			return this.m_storylets[storyletId];
		}

		/// <summary>
		/// Returns all storylets satisfying the given tags.
		/// </summary>
		/// <param name="tags"></param>
		/// <returns></returns>
		public List<Storylet> GetStoryletsWithTags(IEnumerable<string> tags)
		{
			return ContentSelection.GetWithTags(
				m_storylets.Values.Select( s => (s, new HashSet<string>( s.Tags )) ),
				tags
			);
		}

		/// <summary>
		/// Returns all storylets satisfying the given tags.
		/// </summary>
		/// <param name="tags"></param>
		/// <returns></returns>
		public List<Storylet> GetStoryletsWithTags(params string[] tags)
		{
			return GetStoryletsWithTags( tags.ToList() );
		}

		/// <summary>
		/// Create storylet instances for a given storylet.
		/// </summary>
		/// <param name="storylet"></param>
		/// <returns></returns>
		public List<StoryletInstance> CreateStoryletInstances(Storylet storylet)
		{
			return CreateStoryletInstances( storylet, new Dictionary<string, object>() );
		}

		/// <summary>
		/// Create storylet instances for a given storylet.
		/// </summary>
		/// <param name="storylet"></param>
		/// <param name="bindings"></param>
		/// <returns></returns>
		public List<StoryletInstance> CreateStoryletInstances(
			Storylet storylet,
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
				var results = storylet.Precondition.Query.Run( this.DB, inputBindings );

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

		/// <summary>
		/// Queue a storylet instance as a choice to present to the player after the
		/// current dialogue line.
		/// </summary>
		/// <param name="instance"></param>
		public void AddDynamicChoice(StoryletInstance instance)
		{
			if ( !m_dynamicChoiceIds.Contains( instance.KnotID ) )
			{
				m_dynamicChoices.Add( new Choice(
					index: m_dynamicChoices.Count,
					text: instance.ChoiceLabel,
					tags: new string[0],
					storyletInstance: instance
				) );
				m_dynamicChoiceIds.Add( instance.KnotID );
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Extract all the storylets from the ink story instance.
		/// </summary>
		private void LoadStorylets()
		{
			List<string> knotIDs = GetAllKnotIDs();

			// Loop through all the knots, create storylets, and separate
			// them based on basic storylets, actions, and locations.
			foreach ( string knotID in knotIDs )
			{
				List<string> knotTags = GetKnotTags( knotID );

				// Storylets need to start with a storylet header
				if (
					knotTags == null
					|| knotTags.Count == 0
					|| !knotTags[0].StartsWith( "---" )
				)
				{
					continue;
				}

				Storylet storylet = CreateStorylet( knotID );

				m_storylets.Add( storylet.ID, storylet );
			}
		}

		/// <summary>
		/// Create a storylet for a knot
		/// </summary>
		/// <param name="story"></param>
		/// <param name="knotID"></param>
		/// <returns></returns>
		private Storylet CreateStorylet(string knotID)
		{

			Storylet storylet = new Storylet( knotID, m_story );

			// All storylet metadata is contained within the knot-level tags. The content between
			// the "---" and the "===" are called the storylet header. We need to parse each line
			// of the header and set the appropriate metadata values on the storylet instance we
			// just created.

			List<string> knotTags = GetKnotTags( knotID );

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
					RePraxis.DBQuery query = PreconditionQueryFromTags(
						knotID, knotTags, lineIndex, out lineIndex );

					List<string> outputVars = line.Split( " " )[1..^0].ToList();

					storylet.Precondition = new StoryletPrecondition(
						query
					)
					{
						OutputVars = outputVars
					};
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
			string knotID,
			List<string> tags,
			int startingIndex,
			out int lineIndex
		)
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
					$"Missing '@end' statement in precondition query for knot '{knotID}'"
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
		private List<string> GetAllKnotIDs()
		{
			List<string> knotList = new List<string>();

			Ink.Runtime.Container mainContentContainer = m_story.mainContentContainer;
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
		/// Get the tags for an Ink knot. This is a replacement method that fixes an error
		/// in Ink that prevents knots with parameters from having tags.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public List<string> GetKnotTags(string pathString)
		{
			var path = new Ink.Runtime.Path( pathString );

			// Expected to be global story, knot or stitch
			var flowContainer = m_story.ContentAtPath( path ).container;
			while ( true )
			{
				var firstContent = flowContainer.content[0];
				if ( firstContent is Ink.Runtime.Container )
					flowContainer = (Ink.Runtime.Container)firstContent;
				else break;
			}

			// Any initial tag objects count as the "main tags" associated with that story/knot/stitch
			bool inTag = false;
			List<string> tags = null;
			foreach ( var c in flowContainer.content )
			{
				var v = c as Ink.Runtime.VariableAssignment; if ( v != null ) { continue; }
				var command = c as Ink.Runtime.ControlCommand;
				if ( command != null )
				{
					if ( command.commandType == Ink.Runtime.ControlCommand.CommandType.BeginTag )
					{
						inTag = true;
					}
					else if ( command.commandType == Ink.Runtime.ControlCommand.CommandType.EndTag )
					{
						inTag = false;
					}
				}

				else if ( inTag )
				{
					var str = c as Ink.Runtime.StringValue;
					if ( str != null )
					{
						if ( tags == null ) tags = new List<string>();
						tags.Add( str.value );
					}
					else
					{
						m_story.Error( "Tag contained non-text content. Only plain text is allowed when using globalTags or TagsAtContentPath. If you want to evaluate dynamic content, you need to use story.Continue()." );
					}
				}

				// Any other content - we're done
				// We only recognise initial text-only tags
				else
				{
					break;
				}
			}

			return tags;
		}


		/// <summary>
		/// Removes any potential storylet header tags from a dialogue line. This is only relevant
		/// to the first dialogue line in a storylet. However, we have no may of knowing which are
		/// which. So, we run this on every dialogue line.
		/// </summary>
		/// <param name="tags"></param>
		private void ExtractLineTags(IList<string> tags)
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

					if ( line.StartsWith( "---" ) && !foundOpeningTag )
					{
						foundOpeningTag = true;
						continue;
					}

					if ( line.StartsWith( "===" ) && !foundClosingTag )
					{
						foundClosingTag = true;
						continue;
					}

					filteredTags.Add( line );
				}
			}

			CurrentTags = filteredTags;
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
					Storylet storylet = m_storylets[storyletId];

					List<StoryletInstance> instances = CreateStoryletInstances(
						storylet,
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

					List<Storylet> storyletsWithTags = GetStoryletsWithTags( tagList )
						.Where( storylet => storylet.IsEligible )
						.ToList();

					List<StoryletInstance> storyletInstances = new List<StoryletInstance>();
					List<StoryletInstance> mandatoryStorylets = new List<StoryletInstance>();

					foreach ( Storylet storylet in storyletsWithTags )
					{
						List<StoryletInstance> instances = CreateStoryletInstances(
							storylet,
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
						Storylet fallbackStorylet = m_storylets[fallback];

						List<StoryletInstance> instances = CreateStoryletInstances(
							fallbackStorylet,
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
					Storylet storylet = m_storylets[storyletId];

					List<StoryletInstance> instances = CreateStoryletInstances(
						storylet,
						new Dictionary<string, object>()
					);

					if ( instances.Count == 0 ) return;

					StoryletInstance selectedInstance = instances
						.RandomElementByWeight( s => s.Weight );

					AddDynamicChoice( selectedInstance );
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
						if ( m_dynamicChoiceIds.Contains( storylet.ID ) ) continue;

						List<StoryletInstance> instances = CreateStoryletInstances(
							storylet,
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

						AddDynamicChoice( selectedInstance );
					}
					else if ( storyletInstances.Count > 0 )
					{
						StoryletInstance selectedInstance = storyletInstances
							.RandomElementByWeight( s => s.Weight );

						AddDynamicChoice( selectedInstance );
					}
				}
			);

			m_story.BindExternalFunction(
				"TotalChoiceCount",
				() =>
				{
					int count = m_story.currentChoices.Count;

					count += m_dynamicChoices.Count;

					return count;
				}
			);
		}

		#endregion
	}
}
