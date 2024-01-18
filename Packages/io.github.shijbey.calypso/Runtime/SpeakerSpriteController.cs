using System.Collections;
using UnityEngine;

namespace Calypso
{
    /// <summary>
    /// Manages the animations of character sprites and their transition
    /// on and off screen
    /// </summary>
    public class SpeakerSpriteController : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// Reference to the lookup table for IDs to character GameObjects
        /// </summary>
        [SerializeField]
        private CharacterManager m_characterManager;

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
        protected float m_slideOutSeconds = 0.1f;

        /// <summary>
        /// The fade-in duration time in seconds.
        /// </summary>
        [SerializeField]
        protected float m_slideInSeconds = 0.1f;

        /// <summary>
        /// A reference to the Coroutine responsible for fading the background
        /// when transitioning from one image sprite to another.
        /// </summary>
        private Coroutine m_transitionCoroutine = null;

        /// <summary>
        /// The currently displayed character
        /// </summary>
        private Actor m_displayedCharacter;

        #endregion

        #region Properties

        /// <summary>
        /// Is the speaker sprite currently hidden
        /// </summary>
        public bool IsHidden { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Reset all character sprites to be out of view
        /// </summary>
        public void ResetSprites()
        {
            foreach (var character in m_characterManager.Characters)
            {
                character.transform.position = m_offScreenPosition.position;
            }
        }

        /// <summary>
        /// Slide the current speaker into view
        /// </summary>
        public void ShowSpeaker()
        {
            if (m_displayedCharacter == null) return;

            if (m_transitionCoroutine != null)
            {
                StopCoroutine(m_transitionCoroutine);
            }

            m_transitionCoroutine = StartCoroutine(SlideIn(m_displayedCharacter.transform));
        }

        /// <summary>
        /// Slide the current speaker out of view
        /// </summary>
        public void HideSpeaker()
        {
            if (m_displayedCharacter == null) return;

            if (m_transitionCoroutine != null)
            {
                StopCoroutine(m_transitionCoroutine);
            }

            m_transitionCoroutine = StartCoroutine(SlideOut(m_displayedCharacter.transform));
        }

        /// <summary>
        /// Set the current speaker
        /// </summary>
        /// <param name="speakerID"></param>
        /// <param name="tags"></param>
        public void SetSpeaker(string speakerID, params string[] tags)
        {
            if (m_transitionCoroutine != null)
            {
                StopCoroutine(m_transitionCoroutine);
            }

            m_transitionCoroutine = StartCoroutine(TransitionSpeaker(speakerID, tags));
        }

        #endregion

        #region Private Coroutine Methods

        /// <summary>
        /// Slide the character image off the screen and slide the new on to the screen
        /// </summary>
        /// <returns></returns>
        private IEnumerator TransitionSpeaker(string speakerID, params string[] tags)
        {
            if (m_displayedCharacter != null)
            {
                if (m_displayedCharacter.UniqueID == speakerID)
                {
                    // Only change the speaker animation
                    m_displayedCharacter.SetSprite(tags);
                }
                else
                {
                    // Slide out the old speaker first then set the character
                    yield return SlideOut(m_displayedCharacter.transform);

                    m_displayedCharacter = m_characterManager.GetCharacter(speakerID);
                    m_displayedCharacter.SetSprite(tags);
                }
            }
            else
            {
                // There is not a character displayed so set it
                m_displayedCharacter = m_characterManager.GetCharacter(speakerID);
                m_displayedCharacter.SetSprite(tags);
            }

            yield return SlideIn(m_displayedCharacter.transform);
        }

        /// <summary>
        /// Slide a character sprite into the screen
        /// </summary>
        /// <returns></returns>
        private IEnumerator SlideIn(Transform spriteTransform)
        {
            Vector3 initialPosition = spriteTransform.position;
            float elapsedTime = 0f;
            IsHidden = false;

            while (elapsedTime < m_slideInSeconds)
            {
                elapsedTime += Time.deltaTime;
                spriteTransform.position =
                    Vector3.Lerp(
                        initialPosition,
                        m_onScreenPosition.position,
                        elapsedTime / m_slideOutSeconds);
                yield return null;
            }
        }

        /// <summary>
        /// Slide a character spite off screen
        /// </summary>
        /// <returns></returns>
        private IEnumerator SlideOut(Transform spriteTransform)
        {
            Vector3 initialPosition = spriteTransform.position;
            float elapsedTime = 0f;

            while (elapsedTime < m_slideOutSeconds)
            {
                elapsedTime += Time.deltaTime;
                spriteTransform.position =
                    Vector3.Lerp(
                        initialPosition,
                        m_offScreenPosition.position,
                        elapsedTime / m_slideOutSeconds);
                yield return null;
            }

            IsHidden = true;
        }

        #endregion
    }

}
