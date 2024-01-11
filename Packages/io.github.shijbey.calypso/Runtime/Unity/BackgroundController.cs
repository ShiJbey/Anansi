using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Calypso
{
    /// <summary>
    /// Provides an interface for modifying what background image is displayed.
    /// </summary>
    public class BackgroundController : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// A reference to the Image Script on the Background GameObject.
        /// </summary>
        [SerializeField]
        protected Image m_backgroundImage;

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

        #endregion

        #region Public Methods

        /// <summary>
        /// Change the image displayed in the background.
        /// </summary>
        /// <param name="backgroundSprite"></param>
        public void SetBackground(Sprite backgroundSprite)
        {
            if (m_transitionCoroutine != null)
            {
                StopCoroutine(m_transitionCoroutine);
            }

            m_transitionCoroutine = StartCoroutine(TransitionBackground(backgroundSprite));
        }

        #endregion

        #region Private Coroutine Methods

        /// <summary>
        /// Fade out the old background image, replace it with a new one, and fade it in.
        /// </summary>
        /// <param name="backgroundSprite"></param>
        /// <returns></returns>
        private IEnumerator TransitionBackground(Sprite backgroundSprite)
        {
            // Here we lerp the alpha channel to 0.0 before replacing the source image and
            // interpolating back to 1.0.

            // fade out
            yield return Fade(0f, m_fadeOutSeconds);

            // swap image
            m_backgroundImage.sprite = backgroundSprite;

            // fade in
            yield return Fade(1f, m_fadeInSeconds);
        }

        /// <summary>
        /// A coroutine that fades the background Image toward a given alpha transparency.
        /// </summary>
        /// <param name="targetAlpha"></param>
        /// <param name="fadeTime"></param>
        /// <returns></returns>
        private IEnumerator Fade(float targetAlpha, float fadeTime)
        {
            Color initialColor = m_backgroundImage.color;
            Color targetColor = new Color(initialColor.r, initialColor.g, initialColor.b, targetAlpha);
            float elapsedTime = 0f;

            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                m_backgroundImage.color = Color.Lerp(initialColor, targetColor, elapsedTime / fadeTime);
                yield return null;
            }
        }

        #endregion
    }
}
