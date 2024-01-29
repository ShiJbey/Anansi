using System.Collections;
using UnityEngine;

namespace Calypso
{
	public class InteractionPanelController : MonoBehaviour
	{
		#region Fields

		/// <summary>
		/// Reference to the dialogue panel
		/// </summary>
		[SerializeField]
		private DialoguePanelController m_dialoguePanel;

		/// <summary>
		/// The on-screen position of the dialogue panel.
		/// </summary>
		private Vector3 m_onScreenPosition;

		/// <summary>
		/// The off-screen position of the dialogue panel.
		/// </summary>
		private Vector3 m_offScreenPosition;

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
		/// Reference to this MonoBehaviour's RectTransform.
		/// </summary>
		private RectTransform m_rectTransform;

		/// <summary>
		/// A reference to the coroutine that handles sliding the dialogue panel on and off screen
		/// </summary>
		private Coroutine m_panelSlideCoroutine = null;

		#endregion

		#region Unity Messages

		private void Start()
		{
			// Configure the on and off-screen positions
			m_rectTransform = GetComponent<RectTransform>();

			Vector3 startingPos = m_rectTransform.position;
			m_onScreenPosition = new Vector3( startingPos.x, startingPos.y, startingPos.z );

			m_offScreenPosition = new Vector3(
				startingPos.x,
				-(m_rectTransform.rect.height + 200),
				startingPos.z
			);

			m_dialoguePanel.OnHide.AddListener( Show );
			m_dialoguePanel.OnShow.AddListener( Hide );

			Show();
		}

		public void OnDisable()
		{
			m_dialoguePanel.OnHide.RemoveListener( Show );
			m_dialoguePanel.OnShow.RemoveListener( Hide );
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Show the interaction panel
		/// </summary>
		public void Show()
		{
			if ( m_panelSlideCoroutine != null )
			{
				StopCoroutine( m_panelSlideCoroutine );
			}

			m_panelSlideCoroutine = StartCoroutine( SlidePanelIn() );
		}

		/// <summary>
		/// Hide the interaction panel
		/// </summary>
		public void Hide()
		{
			if ( m_panelSlideCoroutine != null )
			{
				StopCoroutine( m_panelSlideCoroutine );
			}

			m_panelSlideCoroutine = StartCoroutine( SlidePanelOut() );
		}

		#endregion

		#region Private Coroutine Methods

		/// <summary>
		/// Slide the interaction panel off screen
		/// </summary>
		/// <returns></returns>
		private IEnumerator SlidePanelOut()
		{
			Vector3 initialPosition = m_rectTransform.position;
			float elapsedTime = 0f;

			while ( elapsedTime < m_slideOutSeconds )
			{
				elapsedTime += Time.deltaTime;
				m_rectTransform.position =
					Vector3.Lerp( initialPosition, m_offScreenPosition, elapsedTime / m_slideOutSeconds );
				yield return null;
			}
		}

		/// <summary>
		/// Slide the interaction panel onto the screen
		/// </summary>
		/// <returns></returns>
		private IEnumerator SlidePanelIn()
		{
			Vector3 initialPosition = m_rectTransform.position;
			float elapsedTime = 0f;

			while ( elapsedTime < m_slideInSeconds )
			{
				elapsedTime += Time.deltaTime;
				m_rectTransform.position =
					Vector3.Lerp( initialPosition, m_onScreenPosition, elapsedTime / m_slideInSeconds );
				yield return null;
			}
		}

		#endregion
	}
}
