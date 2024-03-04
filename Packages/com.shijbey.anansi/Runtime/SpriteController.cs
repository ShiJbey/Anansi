using UnityEngine;

namespace Anansi
{
	/// <summary>
	/// Manages the currently displayed sprite for a character or background
	/// </summary>
	public abstract class SpriteController : MonoBehaviour
	{
		/// <summary>
		/// Set the current sprite using the given tags
		/// </summary>
		/// <param name="tags"></param>
		public abstract void SetSpriteFromTags(params string[] tags);
	}
}
