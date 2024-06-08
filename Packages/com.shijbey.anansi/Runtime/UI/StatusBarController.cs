using UnityEngine;

namespace Anansi
{
	/// <summary>
	/// Controller behaviour for the status bar at the top of the screen
	/// </summary>
	public class StatusBarController : MonoBehaviour
	{
		#region Fields

		/// <summary>
		/// A reference to the text displaying the name of th. displayed location.
		/// </summary>
		[SerializeField]
		private TMPro.TMP_Text m_locationText;

		/// <summary>
		/// A reference to the text displaying the current time and date.
		/// </summary>
		[SerializeField]
		private TMPro.TMP_Text m_dateTimeText;

		/// <summary>
		/// A reference to the GameManager instance
		/// </summary>
		private GameManager m_gameManager;

		private SimulationController m_simulationController;

		#endregion

		#region Unity Messages

		private void Awake()
		{
			m_gameManager = FindObjectOfType<GameManager>();
			m_simulationController = FindObjectOfType<SimulationController>();
		}

		private void OnEnable()
		{
			m_simulationController.OnTick += HandleTimeChange;
			m_gameManager.OnStoryLocationChange += HandleLocationChange;
		}

		private void OnDisable()
		{
			m_simulationController.OnTick -= HandleTimeChange;
			m_gameManager.OnStoryLocationChange -= HandleLocationChange;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Callback executed when the current time in the story changes.
		/// </summary>
		/// <param name="dateTime"></param>
		public void HandleTimeChange()
		{
			m_dateTimeText.SetText( m_simulationController.DateTime.ToString() );
		}

		/// <summary>
		/// Callback executed when the story's location changes.
		/// </summary>
		/// <param name="location"></param>
		public void HandleLocationChange(Location location)
		{
			m_locationText.SetText( location.DisplayName );
		}

		#endregion
	}
}
