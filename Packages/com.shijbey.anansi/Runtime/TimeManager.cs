using UnityEngine;
using UnityEngine.Events;

namespace Anansi
{
	/// <summary>
	/// Manages the current time in the simulation.
	///
	/// This code is adapted from this tutorial by Dan Pos:
	/// https://youtu.be/wTH5iwO4n0s
	/// </summary>
	public class TimeManager : MonoBehaviour
	{
		#region Fields

		/// <summary>
		/// The day used to initialize the starting date
		/// </summary>
		[SerializeField]
		private int m_startingDay = 1;

		/// <summary>
		/// The time of day used to initialize the starting date
		/// </summary>
		[SerializeField]
		private TimeOfDay m_startingTimeOfDay = TimeOfDay.Morning;

		/// <summary>
		/// The current date
		/// </summary>
		[SerializeField]
		[HideInInspector]
		private SimDateTime m_dateTime;

		#endregion

		#region Properties

		/// <summary>
		/// The current date
		/// </summary>
		public SimDateTime DateTime => new SimDateTime( m_dateTime );

		#endregion

		#region Actions and Events

		/// <summary>
		/// Event invoked when the date is updated
		/// </summary>
		public static UnityAction<SimDateTime> OnTimeChanged;

		#endregion

		#region Unity Messages

		private void Awake()
		{
			m_dateTime = new SimDateTime( m_startingDay, m_startingTimeOfDay );
		}

		// Start is called before the first frame update
		private void Start()
		{
			OnTimeChanged?.Invoke( m_dateTime );
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Advance the time by one timestep
		/// </summary>
		/// <param name="deltaTime"></param>
		public void AdvanceTime()
		{
			m_dateTime.AdvanceTime();
			OnTimeChanged?.Invoke( m_dateTime );
		}

		#endregion
	}
}
