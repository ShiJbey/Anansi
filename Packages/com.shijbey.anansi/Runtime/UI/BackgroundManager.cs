using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Anansi
{
	/// <summary>
	/// Manages the presentation of background images in the game.
	/// </summary>
	public class BackgroundManager : MonoBehaviour
	{
		#region Fields

		/// <summary>
		/// An overlay image to control fading background to black.
		/// </summary>
		[SerializeField]
		private Image m_backgroundOverlay;

		/// <summary>
		/// The onscreen position of the speaker image.
		/// </summary>
		[SerializeField]
		private Transform m_onScreenPosition;

		/// <summary>
		/// The offscreen position of the speaker image.
		/// </summary>
		[SerializeField]
		private Transform m_offScreenPosition;

		/// <summary>
		/// The fade-out duration time in seconds.
		/// </summary>
		[SerializeField]
		protected float m_fadeOutSeconds;

		/// <summary>
		/// The fade-in duration time in seconds.
		/// </summary>
		[SerializeField]
		protected float m_fadeInSeconds;

		/// <summary>
		/// A reference to the Coroutine responsible for fading the background
		/// when transitioning from one image sprite to another.
		/// </summary>
		private Coroutine m_transitionCoroutine = null;

		/// <summary>
		/// The currently displayed character
		/// </summary>
		private Location m_displayedLocation;

		private SimulationController m_simulationController;

		private DialogueManager m_dialogueManager;

		#endregion

		#region Unity Messages

		private void Awake()
		{
			m_simulationController = FindObjectOfType<SimulationController>();
			m_dialogueManager = FindObjectOfType<DialogueManager>();
		}

		private void OnEnable()
		{
			m_dialogueManager.OnBackgroundChange += HandleBackgroundChange;
		}

		private void OnDisable()
		{
			m_dialogueManager.OnBackgroundChange -= HandleBackgroundChange;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Move all location backgrounds out of view
		/// </summary>
		public void ResetBackgrounds()
		{
			foreach ( var location in m_simulationController.Locations )
			{
				location.transform.position = m_offScreenPosition.position;
			}
		}

		/// <summary>
		/// Slide the current speaker into view
		/// </summary>
		public void ShowBackground()
		{
			if ( m_displayedLocation == null ) return;

			if ( m_transitionCoroutine != null )
			{
				StopCoroutine( m_transitionCoroutine );
			}

			m_transitionCoroutine = StartCoroutine( FadeTo( new Color( 0, 0, 0, 0 ), m_fadeInSeconds ) );
		}

		/// <summary>
		/// Slide the current speaker out of view
		/// </summary>
		public void HideBackground()
		{
			if ( m_displayedLocation == null ) return;

			if ( m_transitionCoroutine != null )
			{
				StopCoroutine( m_transitionCoroutine );
			}

			m_transitionCoroutine = StartCoroutine( FadeTo( Color.black, m_fadeOutSeconds ) );
		}

		/// <summary>
		/// Set the current speaker
		/// </summary>
		/// <param name="locationID"></param>
		/// <param name="tags"></param>
		public void SetBackground(string locationID, params string[] tags)
		{
			if ( m_transitionCoroutine != null )
			{
				StopCoroutine( m_transitionCoroutine );
			}

			m_transitionCoroutine = StartCoroutine( TransitionBackground( locationID, tags ) );
		}

		#endregion

		#region Private Methods

		private void HandleBackgroundChange(BackgroundInfo info)
		{
			if ( info == null )
			{
				HideBackground();
			}
			else
			{
				SetBackground( info.BackgroundId, info.Tags );
			}
		}

		/// <summary>
		/// Slide the character image off the screen and slide the new on to the screen
		/// </summary>
		/// <returns></returns>
		private IEnumerator TransitionBackground(string locationID, params string[] tags)
		{
			if ( m_displayedLocation != null )
			{
				// Fade out the existing background
				yield return FadeTo( Color.black, m_fadeOutSeconds );

				if ( m_displayedLocation.UniqueID == locationID )
				{
					// Only change the location sprite
					m_displayedLocation.SetSprite( tags );

					yield return FadeTo( new Color( 0, 0, 0, 0 ), m_fadeInSeconds );
				}
				else
				{
					// Move the current location off screen
					m_displayedLocation.transform.position = m_offScreenPosition.position;

					m_displayedLocation = m_simulationController.GetLocation( locationID );
					m_displayedLocation.SetSprite( tags );

					yield return FadeTo( Color.black, m_fadeOutSeconds );

					m_displayedLocation.transform.position = m_onScreenPosition.position;

					yield return FadeTo( new Color( 0, 0, 0, 0 ), m_fadeInSeconds );
				}
			}
			else
			{
				m_displayedLocation = m_simulationController.GetLocation( locationID );
				m_displayedLocation.SetSprite( tags );

				yield return FadeTo( Color.black, m_fadeOutSeconds );

				m_displayedLocation.transform.position = m_onScreenPosition.position;

				yield return FadeTo( new Color( 0, 0, 0, 0 ), m_fadeInSeconds );
			}
		}

		/// <summary>
		/// A coroutine that fades the background image back to a color.
		/// </summary>
		/// <returns></returns>
		private IEnumerator FadeTo(Color targetColor, float fadeInSeconds)
		{
			Color initialColor = m_backgroundOverlay.color;
			float elapsedTime = 0f;

			while ( elapsedTime < fadeInSeconds )
			{
				elapsedTime += Time.deltaTime;
				m_backgroundOverlay.color = Color.Lerp(
					initialColor, targetColor, elapsedTime / fadeInSeconds );
				yield return null;
			}

			m_backgroundOverlay.color = targetColor;
		}

		#endregion
	}
}
