using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Anansi
{
	/// <summary>
	/// Controls the background image shown when dialogue is displayed.
	/// </summary>
	public class DialogueBackgroundController : MonoBehaviour
	{
		#region Fields

		/// <summary>
		/// The default fade-out duration time in seconds.
		/// </summary>
		[SerializeField]
		private float m_defaultFadeOutSeconds;

		/// <summary>
		/// The default fade-in duration time in seconds.
		/// </summary>
		[SerializeField]
		private float m_defaultFadeInSeconds;

		/// <summary>
		/// A reference to the Coroutine responsible for fading the background
		/// when transitioning from one image sprite to another.
		/// </summary>
		private Coroutine m_transitionCoroutine;

		/// <summary>
		/// The image component for the background.
		/// </summary>
		[SerializeField]
		private Image m_image;

		#endregion

		#region Unity Messages

		void Awake()
		{
			m_transitionCoroutine = null;
		}

		void Start()
		{
			// m_image = GetComponent<Image>();
			Hide();
		}

		#endregion

		/// <summary>
		/// Change the background image without fade transition.
		/// </summary>
		/// <param name="backgroundSprite"></param>
		public void ShowBackground(Sprite backgroundSprite)
		{
			gameObject.SetActive( true );
			m_image.sprite = backgroundSprite;
			m_image.color = Color.white;
		}

		/// <summary>
		/// Change background image with fade times.
		/// </summary>
		/// <param name="backgroundSprite"></param>
		/// <param name="fadeOutSeconds"></param>
		/// <param name="fadeInSeconds"></param>
		public void ShowBackgroundWithFade(
			Sprite backgroundSprite,
			float fadeOutSeconds = -1,
			float fadeInSeconds = -1
		)
		{
			gameObject.SetActive( true );

			if ( m_image.sprite == backgroundSprite ) return;

			if ( m_transitionCoroutine != null )
			{
				StopCoroutine( m_transitionCoroutine );
			}

			fadeInSeconds = (fadeInSeconds >= 0) ? fadeInSeconds : m_defaultFadeInSeconds;
			fadeOutSeconds = (fadeOutSeconds >= 0) ? fadeOutSeconds : m_defaultFadeOutSeconds;

			m_transitionCoroutine = StartCoroutine(
				TransitionBackground( backgroundSprite, fadeOutSeconds, fadeInSeconds )
			);
		}

		/// <summary>
		/// Hide the background image by making it transparent.
		/// </summary>
		public void Hide()
		{
			Color color = m_image.color;
			m_image.sprite = null;
			m_image.color = new Color( color.r, color.g, color.b, 0.0f );
		}

		#region Private Methods

		/// <summary>
		/// Slide the character image off the screen and slide the new on to the screen
		/// </summary>
		/// <returns></returns>
		private IEnumerator TransitionBackground(
			Sprite backgroundSprite,
			float fadeOutSeconds,
			float fadeInSeconds
		)
		{
			yield return FadeTo( Color.black, fadeOutSeconds );

			m_image.sprite = backgroundSprite;

			yield return FadeTo( Color.white, fadeInSeconds );
		}

		/// <summary>
		/// A coroutine that fades the background image back to a color.
		/// </summary>
		/// <returns></returns>
		private IEnumerator FadeTo(Color targetColor, float fadeInSeconds)
		{
			Color initialColor = m_image.color;
			float elapsedTime = 0f;

			while ( elapsedTime < fadeInSeconds )
			{
				elapsedTime += Time.deltaTime;
				m_image.color = Color.Lerp(
					initialColor, targetColor, elapsedTime / fadeInSeconds );
				yield return null;
			}

			m_image.color = targetColor;
		}


		#endregion
	}
}
