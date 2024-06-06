using System.Collections.Generic;
using UnityEngine;
using TDRS;
using System;
using Ink.Runtime;
using Anansi.Scheduling;
using UnityEngine.Events;
using System.Linq;

namespace Anansi
{
	/// <summary>
	/// The GameManager coordinates the start of the game.
	///
	/// <remark>
	/// You can replace this manager with another class in your own projects. Just ensure that it
	/// calls <c>StoryController.Initialize()</c> and <c>StoryController.StartStory</c>
	/// </remark>
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
		/// Manages all the progression of the story and all storylets.
		/// </summary>
		[SerializeField]
		private StoryController m_storyController;

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
		private SocialEngineController m_socialEngine;

		/// <summary>
		/// A reference to the starting storylet.
		/// </summary>
		private Storylet m_startingStorylet;

		/// <summary>
		/// All storylets related to locations on the map.
		/// </summary>
		private Dictionary<string, Storylet> m_locationStorylets;

		/// <summary>
		/// All storylets related to actions the player can take at locations.
		/// </summary>
		private Dictionary<string, Storylet> m_actionStorylets;

		#endregion

		#region Properties

		public RePraxis.RePraxisDatabase DB => m_socialEngine.DB;

		public StoryController StoryController => m_storyController;

		#endregion

		#region Actions and Events

		public UnityAction<Location> OnStoryLocationChange;
		public UnityAction<Character> OnSpeakerChange;

		#endregion

		#region Unity Messages

		private void Awake()
		{
			m_startingStorylet = null;
			m_locationStorylets = new Dictionary<string, Storylet>();
			m_actionStorylets = new Dictionary<string, Storylet>();
		}

		private void Start()
		{

			SocialEngineController.Instance.Initialize();
			SocialEngineController.Instance.RegisterAgentsAndRelationships();

			m_storyController.OnRegisterExternalFunctions += this.RegisterExternalInkFunctions;

			m_storyController.Initialize();

			foreach ( var entry in m_storyController.GetStoryletsWithTags( new string[] { "location" } ) )
			{
				m_locationStorylets.Add( entry.KnotID, entry );
			}

			foreach ( var entry in m_storyController.GetStoryletsWithTags( new string[] { "action" } ) )
			{
				m_actionStorylets.Add( entry.KnotID, entry );
			}

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

			m_storyController.StartStory();
		}

		#endregion

		#region Public Methods

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

				if ( scheduleManager == null ) continue;

				ScheduleEntry entry = scheduleManager.GetEntry( currentDate );

				if ( entry == null ) continue;

				SetCharacterLocation( character, m_locationManager.GetLocation( entry.Location ) );
			}
		}

		/// <summary>
		/// Sets the games background image to the given location
		/// </summary>
		/// <param name="locationID"></param>
		/// <param name="tags"></param>
		public void SetStoryLocation(string locationID, params string[] tags)
		{
			m_locationManager.SetBackground( locationID, tags );
			OnStoryLocationChange?.Invoke( m_locationManager.GetLocation( locationID ) );
		}

		/// <summary>
		/// Move an Actor to a new location.
		/// </summary>
		/// <param name="character"></param>
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
					$"{character.UniqueID}.location!{character.Location.UniqueID}" );
				character.Location = null;
			}

			if ( location != null )
			{
				location.AddCharacter( character );
				character.Location = location;
				DB.Insert( $"{location.UniqueID}.characters.{character.UniqueID}" );
				DB.Insert( $"{character.UniqueID}.location!{location.UniqueID}" );
			}
		}

		/// <summary>
		/// Set the player's current location and change the background
		/// </summary>
		/// <param name="locationID"></param>
		/// <param name="tags"></param>
		public void SetPlayerLocation(string locationID)
		{
			Location location = m_locationManager.GetLocation( locationID );

			if ( m_player.Location != location )
			{
				SetCharacterLocation( m_player, location );
			}

			if ( location == null )
			{
				m_characterManager.HideSpeaker();
				return;
			}

			SetStoryLocation( locationID );
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

				if ( !eligibleLocations.Contains( storylet.LocationID ) ) continue;

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

		private void RegisterExternalInkFunctions(Story story)
		{
			story.BindExternalFunction(
				"AdvanceTime",
				() =>
				{
					Tick();
				}
			);

			story.BindExternalFunction(
				"AvailableActionChoices",
				() =>
				{
					foreach ( var storyletInstance in GetEligibleActionStorylets() )
					{
						m_storyController.AddDynamicChoice( storyletInstance );
					}
				}
			);

			story.BindExternalFunction(
				"AvailableLocationChoices",
				() =>
				{
					foreach ( var storyletInstance in GetEligibleLocationStorylets() )
					{
						m_storyController.AddDynamicChoice( storyletInstance );
					}
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

	public class SpeakerChangeInfo
	{
		public Character Character { get; }
		public string[] SpeakerTags { get; }

		public SpeakerChangeInfo(Character character, string[] tags)
		{
			this.Character = character;
			this.SpeakerTags = tags;
		}
	}
}
