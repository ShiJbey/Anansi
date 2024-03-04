
using System.Collections.Generic;
using UnityEngine;

namespace Anansi
{
	/// <summary>
	/// Manages animated sprites for a character or location.
	/// </summary>
	public class AnimatedSpriteController : SpriteController
	{
		#region Fields

		/// <summary>
		/// Sprites used to display the character
		/// </summary>
		[SerializeField]
		protected AnimationEntry[] m_animations;

		/// <summary>
		/// A reference to the GameObjects's animator component
		/// </summary>
		private Animator m_animator;

		/// <summary>
		/// The name of the animation to fallback to if none is found
		/// </summary>
		private string m_fallbackAnimationName;

		#endregion

		#region Unity Messages

		public void Awake()
		{
			m_animator = GetComponent<Animator>();

			if ( m_animator == null )
			{
				throw new System.Exception(
					$"AnimatedSpriteController missing for {gameObject.name}." );
			}

			foreach ( var entry in m_animations )
			{
				if ( entry.isFallback )
				{
					m_fallbackAnimationName = entry.animationName;
				}
			}

			if ( m_fallbackAnimationName == null )
			{
				throw new System.Exception(
					$"No fallback animation given for {gameObject.name}" );
			}
		}

		#endregion

		#region Public Methods

		public override void SetSpriteFromTags(params string[] tags)
		{
			var chosenAnim = m_fallbackAnimationName;
			var eligibleAnimations = new List<string>();

			foreach ( var entry in m_animations )
			{
				var spriteTags = new HashSet<string>( entry.tags );
				bool hasTags = true;

				foreach ( string t in tags )
				{
					if ( !spriteTags.Contains( t ) )
					{
						hasTags = false;
						break;
					}
				}

				if ( hasTags )
				{
					eligibleAnimations.Add( entry.animationName );
				}
			}

			if ( eligibleAnimations.Count > 0 )
			{
				chosenAnim = eligibleAnimations[
					UnityEngine.Random.Range( 0, eligibleAnimations.Count )
				];
			}

			m_animator.Play( chosenAnim );
		}

		#endregion

		#region Helper Classes

		/// <summary>
		/// Associates a sprite animation with a set of descriptive tags.
		/// </summary>
		[System.Serializable]
		public class AnimationEntry
		{
			/// <summary>
			/// The name of the animation to play.
			/// </summary>
			public string animationName;

			/// <summary>
			/// Tags used to retrieve the image.
			/// Examples: neutral, smiling, scowling, sad, blushing, laughing
			/// </summary>
			public string[] tags;

			/// <summary>
			/// Fallback to this animation if none is found
			/// </summary>
			public bool isFallback;
		}

		#endregion
	}
}
