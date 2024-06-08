using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Anansi
{
	/// <summary>
	/// Manages the presentation of background images in the game.
	/// </summary>
	public class BackgroundManager : MonoBehaviour
	{
		#region Fields

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

		#endregion

		#region Unity Messages

		private void Start()
		{
			m_simulationController = FindObjectOfType<SimulationController>();
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

			m_transitionCoroutine = StartCoroutine( FadeIn( m_displayedLocation ) );
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

			m_transitionCoroutine = StartCoroutine( FadeOut( m_displayedLocation ) );
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

		/// <summary>
		/// Reset transparency of background image to full opacity
		/// </summary>
		/// <param name="location"></param>
		private void ResetTransparency(Location location)
		{
			SpriteRenderer spriteRenderer = location.GetComponent<SpriteRenderer>();
			Color color = spriteRenderer.color;
			spriteRenderer.color = new Color( color.r, color.g, color.b, 1.0f );
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
				yield return FadeOut( m_displayedLocation );

				if ( m_displayedLocation.UniqueID == locationID )
				{
					// Only change the location sprite
					m_displayedLocation.SetSprite( tags );

					yield return FadeIn( m_displayedLocation );
				}
				else
				{
					// Move the current location off screen
					m_displayedLocation.transform.position = m_offScreenPosition.position;
					// Reset its transparency
					ResetTransparency( m_displayedLocation );

					m_displayedLocation = m_simulationController.GetLocation( locationID );
					m_displayedLocation.SetSprite( tags );

					yield return FadeOut( m_displayedLocation );

					m_displayedLocation.transform.position = m_onScreenPosition.position;

					yield return FadeIn( m_displayedLocation );
				}
			}
			else
			{
				m_displayedLocation = m_simulationController.GetLocation( locationID );
				m_displayedLocation.SetSprite( tags );

				yield return FadeOut( m_displayedLocation );

				m_displayedLocation.transform.position = m_onScreenPosition.position;

				yield return FadeIn( m_displayedLocation );
			}
		}

		/// <summary>
		/// A coroutine that fades the background Image to black.
		/// </summary>
		/// <param name="location"></param>
		/// <returns></returns>
		private IEnumerator FadeOut(Location location)
		{
			SpriteRenderer spriteRenderer = location.GetComponent<SpriteRenderer>();
			Color initialColor = spriteRenderer.color;
			Color targetColor = new Color(
				initialColor.r,
				initialColor.g,
				initialColor.b,
				0 );
			float elapsedTime = 0f;

			while ( elapsedTime < m_fadeOutSeconds )
			{
				elapsedTime += Time.deltaTime;
				spriteRenderer.color = Color.Lerp(
					initialColor, targetColor, elapsedTime / m_fadeOutSeconds );
				yield return null;
			}

			spriteRenderer.color = targetColor;
		}

		/// <summary>
		/// A coroutine that fades the background sprite back to normal color.
		/// </summary>
		/// <param name="location"></param>
		/// <returns></returns>
		private IEnumerator FadeIn(Location location)
		{
			SpriteRenderer spriteRenderer = location.GetComponent<SpriteRenderer>();
			Color initialColor = spriteRenderer.color;
			Color targetColor = new Color(
				initialColor.r,
				initialColor.g,
				initialColor.b,
				1f );
			float elapsedTime = 0f;

			while ( elapsedTime < m_fadeInSeconds )
			{
				elapsedTime += Time.deltaTime;
				spriteRenderer.color = Color.Lerp( initialColor, targetColor, elapsedTime / m_fadeInSeconds );
				yield return null;
			}

			spriteRenderer.color = targetColor;
		}

		#endregion
	}
}
