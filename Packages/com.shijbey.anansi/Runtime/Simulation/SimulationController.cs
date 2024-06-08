using System;
using System.Collections.Generic;
using Anansi.Scheduling;
using RePraxis;
using TDRS;
using UnityEngine;
using UnityEngine.Events;

namespace Anansi
{
	public class SimulationController : MonoBehaviour
	{

		/// <summary>
		/// A look-up table of actor IDs mapped to their instances.
		/// </summary>
		private Dictionary<string, Character> m_characters;

		/// <summary>
		/// IDs of locations mapped to their instances.
		/// </summary>
		private Dictionary<string, Location> m_locations;

		public IEnumerable<Character> Characters => m_characters.Values;

		public IEnumerable<Location> Locations => m_locations.Values;

		public SocialEngineController m_socialEngine;

		public RePraxisDatabase DB => m_socialEngine.DB;

		public SimDateTime m_dateTime;

		public SimDateTime DateTime => m_dateTime;

		public UnityAction OnTick;

		private void Awake()
		{
			m_characters = new Dictionary<string, Character>();
			m_locations = new Dictionary<string, Location>();
			m_socialEngine = FindObjectOfType<SocialEngineController>();
			m_dateTime = new SimDateTime();
		}


		/// <summary>
		/// Fills the lookup table using entries from the locations collection.
		/// </summary>
		public void Initialize()
		{
			var locations = FindObjectsOfType<Location>();
			foreach ( Location location in locations )
			{
				AddLocation( location );
				DB.Insert( $"{location.UniqueID}" );

			}

			var characters = FindObjectsOfType<Character>();
			foreach ( Character character in characters )
			{
				AddCharacter( character );
				DB.Insert( $"{character.UniqueID}" );
			}

			DB.Insert(
				$"date.time_of_day!{Enum.GetName( typeof( TimeOfDay ), m_dateTime.TimeOfDay )}" );
			DB.Insert(
				$"date.day!{m_dateTime.Day}" );
			DB.Insert(
				$"date.weekday!{Enum.GetName( typeof( WeekDay ), m_dateTime.WeekDay )}" );
			DB.Insert(
				$"date.week!{m_dateTime.Week}" );

			TickCharacterSchedules();
		}

		/// <summary>
		/// Get a location using their ID.
		/// </summary>
		/// <param name="locationID"></param>
		/// <returns></returns>
		public Location GetLocation(string locationID)
		{
			if ( m_locations.TryGetValue( locationID, out var location ) )
			{
				return location;
			}

			throw new KeyNotFoundException( $"Could not find location with ID: {locationID}" );
		}

		/// <summary>
		/// Add a location to the manager
		/// </summary>
		/// <param name="location"></param>
		public void AddLocation(Location location)
		{
			m_locations[location.UniqueID] = location;
		}

		/// <summary>
		/// Get a character using their ID.
		/// </summary>
		/// <param name="characterID"></param>
		/// <returns></returns>
		public Character GetCharacter(string characterID)
		{
			if ( m_characters.TryGetValue( characterID, out var character ) )
			{
				return character;
			}

			throw new KeyNotFoundException( $"Could not find character with ID: {characterID}" );
		}

		/// <summary>
		/// Add a character to the manager
		/// </summary>
		/// <param name="character"></param>
		public void AddCharacter(Character character)
		{
			m_characters.Add( character.UniqueID, character );
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
			m_dateTime.AdvanceTime();

			DB.Insert(
				$"date.time_of_day!{Enum.GetName( typeof( TimeOfDay ), m_dateTime.TimeOfDay )}" );
			DB.Insert(
				$"date.day!{m_dateTime.Day}" );
			DB.Insert(
				$"date.weekday!{Enum.GetName( typeof( WeekDay ), m_dateTime.WeekDay )}" );
			DB.Insert(
				$"date.week!{m_dateTime.Week}" );
		}

		/// <summary>
		/// Update character states based on what their schedules dictate.
		/// </summary>
		public void TickCharacterSchedules()
		{
			foreach ( Character character in Characters )
			{
				var scheduleManager = character.gameObject.GetComponent<ScheduleManager>();

				if ( scheduleManager == null ) continue;

				ScheduleEntry entry = scheduleManager.GetEntry( m_dateTime );

				if ( entry == null ) continue;

				SetCharacterLocation( character, GetLocation( entry.Location ) );
			}
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

	}
}
