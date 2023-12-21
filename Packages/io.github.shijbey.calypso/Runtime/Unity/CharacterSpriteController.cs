using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Calypso.Unity
{
    /// <summary>
    /// Provides an interface for modifying what NPC image is displayed during dialogue.
    /// </summary>
    public class CharacterSpriteController : MonoBehaviour
    {
        #region Protected Fields

        /// <summary>
        /// A reference to the Image Script on the Background GameObject.
        /// </summary>
        [SerializeField]
        protected Image characterImage;

        /// <summary>
        /// The fade-out duration time in seconds.
        /// </summary>
        [SerializeField]
        protected float slideOutSeconds;

        /// <summary>
        /// The fade-in duration time in seconds.
        /// </summary>
        [SerializeField]
        protected float slideInSeconds;

        /// <summary>
        /// A reference to the Coroutine responsible for fading the background
        /// when transitioning from one image sprite to another.
        /// </summary>
        private Coroutine transitionCoroutine = null;

        /// <summary>
        /// The onscreen position of the speaker image.
        /// </summary>
        private Vector3 onScreenPosition;

        /// <summary>
        /// The offscreen position of the speaker image.
        /// </summary>
        private Vector3 offScreenPosition;

        #endregion

        #region Unity Lifecycle Methods

        private void Start()
        {
            // Configure the on and off-screen positions

            Vector3 startingPos = characterImage.rectTransform.position;
            onScreenPosition = new Vector3(startingPos.x, startingPos.y, startingPos.z);

            offScreenPosition = new Vector3(
                startingPos.x - (characterImage.rectTransform.rect.width + 200),
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
        public void ChangeSpeaker(Sprite speakerSprite)
        {
            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
            }

            transitionCoroutine = StartCoroutine(TransitionSpeaker(speakerSprite));
        }

        /// <summary>
        /// Permanently slide the character image off the screen
        /// </summary>
        /// <returns></returns>
        public void Hide()
        {
            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
            }

            transitionCoroutine = StartCoroutine(HideSpeakerTransition());
        }

        #endregion

        #region Private Coroutines

        /// <summary>
        /// Slide the character image off the screen and slide the new on to the screen
        /// </summary>
        /// <returns></returns>
        private IEnumerator TransitionSpeaker(Sprite speakerSprite)
        {
            yield return SlideOut();

            characterImage.sprite = speakerSprite;

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
            Vector3 initialPosition = characterImage.rectTransform.position;
            float elapsedTime = 0f;

            while (elapsedTime < slideInSeconds)
            {
                elapsedTime += Time.deltaTime;
                characterImage.rectTransform.position =
                    Vector3.Lerp(initialPosition, onScreenPosition, elapsedTime / slideOutSeconds);
                yield return null;
            }
        }

        /// <summary>
        /// Slide the character spite off screen
        /// </summary>
        /// <returns></returns>
        private IEnumerator SlideOut()
        {
            Vector3 initialPosition = characterImage.rectTransform.position;
            float elapsedTime = 0f;

            while (elapsedTime < slideOutSeconds)
            {
                elapsedTime += Time.deltaTime;
                characterImage.rectTransform.position =
                    Vector3.Lerp(initialPosition, offScreenPosition, elapsedTime / slideOutSeconds);
                yield return null;
            }
        }

        #endregion
    }
}
