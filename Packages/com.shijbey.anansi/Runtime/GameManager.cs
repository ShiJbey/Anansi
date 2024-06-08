using System.Collections.Generic;
using UnityEngine;
using TDRS;
using System;
using Anansi.Scheduling;
using UnityEngine.Events;
using System.Linq;

namespace Anansi
{
	/// <summary>
	/// Coordinates all game-specific logic.
	/// </summary>
	[DefaultExecutionOrder( 2 )]
	public class GameManager : MonoBehaviour
	{
		#region Fields

		/// <summary>
		/// A reference to the player's character.
		/// </summary>
		[SerializeField]
		protected Character m_player;

		/// <summary>
		/// Manages the underlying world simulation.
		/// </summary>
		[SerializeField]
		private SimulationController m_simulationController;

		/// <summary>
		/// Manages character relationships and all information in the logical database.
		/// </summary>
		[SerializeField]
		private SocialEngineController m_socialEngine;

		/// <summary>
		/// A reference to the script controlling the dialogue and story progression.
		/// </summary>
		[SerializeField]
		private DialogueManager m_dialogueManager;

		/// <summary>
		/// All storylets related to locations on the map.
		/// </summary>
		private Dictionary<string, Storylet> m_locationStorylets;

		/// <summary>
		/// All storylets related to actions the player can take at locations.
		/// </summary>
		private Dictionary<string, Storylet> m_actionStorylets;

		[SerializeField]
		private SpeakerSpriteManager m_speakerSpriteManager;

		[SerializeField]
		private BackgroundManager m_backgroundManager;

		#endregion

		#region Properties

		/// <summary>
		/// A reference to the simulations database.
		/// </summary>
		public RePraxis.RePraxisDatabase DB => m_socialEngine.DB;

		/// <summary>
		/// A reference to the controller's story instance.
		/// </summary>
		public Story Story => m_dialogueManager.Story;

		public DialogueManager DialogueManager => m_dialogueManager;

		#endregion

		#region Actions and Events

		public UnityAction<Location> OnStoryLocationChange;
		public UnityAction<Character> OnSpeakerChange;

		#endregion

		#region Unity Messages

		private void Awake()
		{
			m_locationStorylets = new Dictionary<string, Storylet>();
			m_actionStorylets = new Dictionary<string, Storylet>();
		}

		private void Start()
		{

			SocialEngineController.Instance.Initialize();
			SocialEngineController.Instance.RegisterAgentsAndRelationships();
			m_simulationController.Initialize();
			m_dialogueManager.OnRegisterExternalFunctions += this.RegisterExternalInkFunctions;
			m_dialogueManager.Story.DB = SocialEngineController.Instance.DB;
			m_dialogueManager.Initialize();

			this.m_actionStorylets = m_dialogueManager.Story
				.GetStoryletsWithTags( "action" )
				.ToDictionary( s => s.ID );

			this.m_locationStorylets = m_dialogueManager.Story
				.GetStoryletsWithTags( "location" )
				.ToDictionary( s => s.ID );

			// Reposition character and background sprites
			m_backgroundManager.ResetBackgrounds();
			m_speakerSpriteManager.ResetSprites();

			StartStory();
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Start the story.
		/// </summary>
		public void StartStory()
		{
			Storylet startStorylet = m_dialogueManager.Story.GetStorylet( "start" );
			m_dialogueManager.RunStorylet( startStorylet );
		}

		/// <summary>
		/// Set the player's current location and change the background
		/// </summary>
		/// <param name="locationID"></param>
		/// <param name="tags"></param>
		public void SetPlayerLocation(string locationID)
		{
			Location location = m_simulationController.GetLocation( locationID );

			if ( m_player.Location != location )
			{
				m_simulationController.SetCharacterLocation( m_player, location );
			}
		}

		/// <summary>
		/// Get a list of all location storylets a player could execute.
		/// </summary>
		/// <returns></returns>
		public List<StoryletInstance> GetEligibleLocationStorylets()
		{
			List<StoryletInstance> instances = new List<StoryletInstance>();
			HashSet<string> eligibleLocations = new HashSet<string>(
				m_player.Location.ConnectedLocations.Select( s => s.UniqueID )
			);

			foreach ( var (uid, storylet) in m_locationStorylets )
			{
				// Skip storylets still on cooldown
				if ( storylet.CooldownTimeRemaining > 0 ) continue;

				// Skip storylets that are not repeatable
				if ( !storylet.IsRepeatable && storylet.TimesPlayed > 0 ) continue;

				if ( !eligibleLocations.Contains( storylet.ID ) ) continue;

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
						new Dictionary<string, object>(),
						storylet.Weight
					) );
				}
			}

			return instances;
		}

		/// <summary>
		/// Get a list of all action storylets a player could execute.
		/// </summary>
		/// <returns></returns>
		public List<StoryletInstance> GetEligibleActionStorylets()
		{
			List<StoryletInstance> instances = new List<StoryletInstance>();

			foreach ( var (uid, storylet) in m_actionStorylets )
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

					if ( results.Bindings.Length == 0 )
					{
						instances.Add(
							new StoryletInstance(
								storylet,
								new Dictionary<string, object>(),
								storylet.Weight
							)
						);
					}
					else
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
							new Dictionary<string, object>(),
							storylet.Weight
						)
					);
				}
			}

			return instances;
		}


		#endregion

		#region Private Methods

		private void RegisterExternalInkFunctions(Ink.Runtime.Story story)
		{
			story.BindExternalFunction(
				"AdvanceTime",
				() =>
				{
					m_simulationController.Tick();
				}
			);

			story.BindExternalFunction(
				"SetPlayerLocation",
				(string locationID) =>
				{
					this.SetPlayerLocation( locationID );
				}
			);
		}

		#endregion
	}
}
