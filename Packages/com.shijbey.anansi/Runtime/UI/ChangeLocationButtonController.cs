using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Anansi
{
	/// <summary>
	/// Controls interactivity for the button that opens the location selection menu
	/// </summary>
	public class ChangeLocationButtonController : MonoBehaviour
	{
		#region Fields

		/// <summary>
		/// A reference to the Button script on this GameObject
		/// </summary>
		private Button m_button;

		/// <summary>
		/// A reference to the GameManager
		/// </summary>
		private GameManager m_gameManager;

		/// <summary>
		/// A reference to the panel with buttons to select actions
		/// </summary>
		[SerializeField]
		private ActionSelectionDialogController m_choiceDialog;

		#endregion

		#region Unity Messages

		// Start is called before the first frame update
		private void Awake()
		{
			m_button = GetComponent<Button>();
			m_gameManager = FindObjectOfType<GameManager>();
		}

		private void OnEnable()
		{
			m_button.onClick.AddListener( HandleClick );
		}

		private void OnDisable()
		{
			m_button.onClick.RemoveListener( HandleClick );
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Tell the GameManager to start a conversation when the button is clicked.
		/// </summary>
		private void HandleClick()
		{
			if ( !m_choiceDialog.IsVisible )
			{
				m_choiceDialog.ClearChoices();

				IList<StoryletInstance> locations = m_gameManager.GetEligibleLocationStorylets();

				foreach ( StoryletInstance location in locations )
				{
					// Have to bind to a variable within the loop to prevent issues with
					// binding the incorrect variable within the anonymous function below
					StoryletInstance storyletInstance = location;

					m_choiceDialog.AddChoice(
						location.ChoiceLabel,
						() =>
						{
							m_choiceDialog.Hide();
							m_gameManager.Story.RunStoryletInstance( storyletInstance );
						}
					);
				}

				m_choiceDialog.Show();
			}
			else
			{
				m_choiceDialog.Hide();
				m_choiceDialog.ClearChoices();
			}
		}

		#endregion
	}
}
