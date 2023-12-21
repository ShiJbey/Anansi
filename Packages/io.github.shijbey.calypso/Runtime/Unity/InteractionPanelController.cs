using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Calypso.Unity
{
    public class InteractionPanelController : MonoBehaviour
    {
        [SerializeField]
        private Button talkButton;

        [SerializeField]
        private Button changeLocationButton;

        /// <summary>
        /// The on-screen position of the dialogue panel.
        /// </summary>
        private Vector3 onScreenPosition;

        /// <summary>
        /// The off-screen position of the dialogue panel.
        /// </summary>
        private Vector3 offScreenPosition;

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
        /// Reference to this MonoBehaviour's RectTransform.
        /// </summary>
        [SerializeField]
        private RectTransform rectTransform;

        /// <summary>
        /// A reference to the coroutine that handles sliding the dialogue panel on and off screen
        /// </summary>
        private Coroutine panelSlideCoroutine = null;

        private void Start()
        {
            // Configure the on and off-screen positions

            Vector3 startingPos = rectTransform.position;
            onScreenPosition = new Vector3(startingPos.x, startingPos.y, startingPos.z);

            offScreenPosition = new Vector3(
                startingPos.x,
                -(rectTransform.rect.height + 200),
                startingPos.z
            );

            Show();
        }

        /// <summary>
        /// Show the interaction panel
        /// </summary>
        public void Show()
        {
            if (panelSlideCoroutine != null)
            {
                StopCoroutine(panelSlideCoroutine);
            }

            panelSlideCoroutine = StartCoroutine(SlidePanelIn());
        }

        /// <summary>
        /// Hide the interaction panel
        /// </summary>
        public void Hide()
        {
            if (panelSlideCoroutine != null)
            {
                StopCoroutine(panelSlideCoroutine);
            }

            panelSlideCoroutine = StartCoroutine(SlidePanelOut());
        }

        /// <summary>
        /// Slide the interaction panel off screen
        /// </summary>
        /// <returns></returns>
        private IEnumerator SlidePanelOut()
        {
            Vector3 initialPosition = rectTransform.position;
            float elapsedTime = 0f;

            while (elapsedTime < slideOutSeconds)
            {
                elapsedTime += Time.deltaTime;
                rectTransform.position =
                    Vector3.Lerp(initialPosition, offScreenPosition, elapsedTime / slideOutSeconds);
                yield return null;
            }
        }

        /// <summary>
        /// Slide the interaction panel onto the screen
        /// </summary>
        /// <returns></returns>
        private IEnumerator SlidePanelIn()
        {
            Vector3 initialPosition = rectTransform.position;
            float elapsedTime = 0f;

            while (elapsedTime < slideInSeconds)
            {
                elapsedTime += Time.deltaTime;
                rectTransform.position =
                    Vector3.Lerp(initialPosition, onScreenPosition, elapsedTime / slideInSeconds);
                yield return null;
            }
        }

    }
}
