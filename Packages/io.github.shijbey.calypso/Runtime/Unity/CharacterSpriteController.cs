using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Calypso
{
    /// <summary>
    /// Provides an interface for modifying what NPC image is displayed during dialogue.
    /// </summary>
    public class CharacterSpriteController : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// A reference to the Image Script on the Background GameObject.
        /// </summary>
        [SerializeField]
        protected Image m_characterImage;

        /// <summary>
        /// The fade-out duration time in seconds.
        /// </summary>
        [SerializeField]
        protected float m_slideOutSeconds;

        /// <summary>
        /// The fade-in duration time in seconds.
        /// </summary>
        [SerializeField]
        protected float m_slideInSeconds;

        /// <summary>
        /// A reference to the Coroutine responsible for fading the background
        /// when transitioning from one image sprite to another.
        /// </summary>
        private Coroutine m_transitionCoroutine = null;

        /// <summary>
        /// The onscreen position of the speaker image.
        /// </summary>
        private Vector3 m_onScreenPosition;

        /// <summary>
        /// The offscreen position of the speaker image.
        /// </summary>
        private Vector3 m_offScreenPosition;

        #endregion

        #region Unity Messages

        private void Start()
        {
            // Configure the on and off-screen positions

            Vector3 startingPos = m_characterImage.rectTransform.position;
            m_onScreenPosition = new Vector3(startingPos.x, startingPos.y, startingPos.z);

            m_offScreenPosition = new Vector3(
                startingPos.x - (m_characterImage.rectTransform.rect.width + 200),
                startingPos.y,
                startingPos.z
            );

            Hide();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Swap the character sprite and play the slide out animation
        /// </summary>
        /// <param name="speakerSprite"></param>
        public void SetSpeaker(Sprite speakerSprite)
        {
            if (m_transitionCoroutine != null)
            {
                StopCoroutine(m_transitionCoroutine);
            }

            m_transitionCoroutine = StartCoroutine(TransitionSpeaker(speakerSprite));
        }

        /// <summary>
        /// Slide the character image into view
        /// </summary>
        /// <returns></returns>
        public void Show()
        {
            if (m_transitionCoroutine != null)
            {
                StopCoroutine(m_transitionCoroutine);
            }

            m_transitionCoroutine = StartCoroutine(ShowSpeakerTransition());
        }

        /// <summary>
        /// Slide the character image out of view
        /// </summary>
        /// <returns></returns>
        public void Hide()
        {
            if (m_transitionCoroutine != null)
            {
                StopCoroutine(m_transitionCoroutine);
            }

            m_transitionCoroutine = StartCoroutine(HideSpeakerTransition());
        }

        #endregion

        #region Private Coroutine Methods

        /// <summary>
        /// Slide the character image off the screen and slide the new on to the screen
        /// </summary>
        /// <returns></returns>
        private IEnumerator TransitionSpeaker(Sprite speakerSprite)
        {
            yield return SlideOut();

            m_characterImage.sprite = speakerSprite;

            yield return SlideIn();
        }

        /// <summary>
        /// Permanently slide the character image off the screen
        /// </summary>
        /// <returns></returns>
        private IEnumerator ShowSpeakerTransition()
        {
            yield return SlideIn();
        }

        /// <summary>
        /// Permanently slide the character image off the screen
        /// </summary>
        /// <returns></returns>
        private IEnumerator HideSpeakerTransition()
        {
            yield return SlideOut();
        }

        /// <summary>
        /// Slide the character image into the screen
        /// </summary>
        /// <returns></returns>
        private IEnumerator SlideIn()
        {
            Vector3 initialPosition = m_characterImage.rectTransform.position;
            float elapsedTime = 0f;

            while (elapsedTime < m_slideInSeconds)
            {
                elapsedTime += Time.deltaTime;
                m_characterImage.rectTransform.position =
                    Vector3.Lerp(initialPosition, m_onScreenPosition, elapsedTime / m_slideOutSeconds);
                yield return null;
            }
        }

        /// <summary>
        /// Slide the character spite off screen
        /// </summary>
        /// <returns></returns>
        private IEnumerator SlideOut()
        {
            Vector3 initialPosition = m_characterImage.rectTransform.position;
            float elapsedTime = 0f;

            while (elapsedTime < m_slideOutSeconds)
            {
                elapsedTime += Time.deltaTime;
                m_characterImage.rectTransform.position =
                    Vector3.Lerp(initialPosition, m_offScreenPosition, elapsedTime / m_slideOutSeconds);
                yield return null;
            }
        }

        #endregion
    }
}
