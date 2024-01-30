using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

namespace Calypso
{
	/// <summary>
	/// Handles displaying options for locations where the player may move to on the map.
	/// </summary>
	public class ActionSelectionDialogController : MonoBehaviour
	{
		#region Fields

		/// <summary>
		/// A reference to the prefab used to create choice buttons.
		/// </summary>
		[SerializeField]
		private Button m_choiceButtonPrefab;

		/// <summary>
		/// A reference to the container that holds the choice buttons.
		/// </summary>
		[SerializeField]
		private RectTransform m_choiceButtonContainer;

		/// <summary>
		/// The on-screen position of the dialogue panel.
		/// </summary>
		private Vector3 m_onScreenPosition;

		/// <summary>
		/// The off-screen position of the dialogue panel.
		/// </summary>
		private Vector3 m_offScreenPosition;

		/// <summary>
		/// Reference to this MonoBehaviour's RectTransform.
		/// </summary>
		private RectTransform m_rectTransform;

		/// <summary>
		/// A reference to the UI element holding the choices.
		/// </summary>
		private List<Button> m_choiceButtons;

		/// <summary>
		/// Is the panel currently visible on the screen
		/// </summary>
		private bool m_isVisible = true;

		#endregion

		#region Properties

		/// <summary>
		/// Is the panel currently visible on the screen
		/// </summary>
		public bool IsVisible => m_isVisible;

		#endregion

		#region Unity Messages

		private void Awake()
		{
			m_choiceButtons = new List<Button>();
			m_rectTransform = gameObject.transform as RectTransform;
		}

		private void Start()
		{
			// Configure the on and off-screen positions

			Vector3 startingPos = m_rectTransform.position;
			m_onScreenPosition = new Vector3( startingPos.x, startingPos.y, startingPos.z );

			m_offScreenPosition = new Vector3(
				startingPos.x,
				-(m_rectTransform.rect.height + 200),
				startingPos.z
			);

			Hide();
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Displays the dialog box
		/// </summary>
		public void Show()
		{
			// m_choiceButtonContainer.anchoredPosition = new Vector3(
			// 	m_choiceButtonContainer.position.x,
			// 	0,
			// 	m_choiceButtonContainer.position.z
			// );
			m_rectTransform.position = m_onScreenPosition;
			m_isVisible = true;
		}

		/// <summary>
		/// Hide the dialog box
		/// </summary>
		public void Hide()
		{
			m_rectTransform.position = m_offScreenPosition;
			m_isVisible = false;
		}

		/// <summary>
		/// Destroy all current choice buttons
		/// </summary>
		public void ClearChoices()
		{
			foreach ( Button button in m_choiceButtons )
			{
				Destroy( button.gameObject );
			}

			m_choiceButtons.Clear();
		}

		/// <summary>
		/// Add a choice to the list
		/// </summary>
		/// <param name="label"></param>
		/// <param name="callback"></param>
		public void AddChoice(string label, UnityAction callback)
		{
			Button choiceButton = Instantiate( m_choiceButtonPrefab );
			choiceButton.transform.SetParent( m_choiceButtonContainer.transform, false );
			m_choiceButtons.Add( choiceButton );

			// sets text on the button
			var buttonText = choiceButton.GetComponentInChildren<TMP_Text>();
			buttonText.text = label;

			//  Adds the onClick callback
			choiceButton.onClick.AddListener( callback );
		}

		#endregion
	}
}
