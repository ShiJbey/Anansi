
using System;
using System.Collections.Generic;
using System.Linq;
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

			List<string> bestMatches = ContentSelection.GetWithTags(
				m_animations.Select( a => (a.animationName, new HashSet<string>( a.tags )) ),
				tags
			);

			if ( bestMatches.Count > 0 )
			{
				chosenAnim = bestMatches[
					UnityEngine.Random.Range( 0, bestMatches.Count )
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
