using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Anansi
{
	/// <summary>
	/// A pairing of storylet with a specific set of bindings from running a storylet's
	/// precondition query.
	/// </summary>
	public class StoryletInstance
	{
		#region Properties

		/// <summary>
		/// The ID of the knot within an Ink Story.
		/// </summary>
		public string KnotID => Storylet.ID;

		/// <summary>
		/// The Ink Story this storylet belongs to.
		/// </summary>
		public Ink.Runtime.Story InkStory => Storylet.InkStory;

		/// <summary>
		/// Get the label displayed when this storylet is used as a choice.
		/// </summary>
		public string ChoiceLabel => Storylet.ChoiceLabel;

		/// <summary>
		/// A reference to the storylet this is an instance of.
		/// </summary>
		public Storylet Storylet { get; }

		/// <summary>
		/// The weight of this storylet instance.
		/// </summary>
		public int Weight { get; set; }

		/// <summary>
		/// RePraxis variable names mapped to their bound values from the database.
		/// </summary>
		public Dictionary<string, object> PreconditionBindings { get; }

		/// <summary>
		/// A Hash ID for this storylet
		/// </summary>
		public string HashID { get; private set; }

		#endregion

		#region Constructors

		public StoryletInstance(
			Storylet storylet,
			Dictionary<string, object> preconditionBindings,
			int weight
		)
		{
			Storylet = storylet;
			PreconditionBindings = new Dictionary<string, object>( preconditionBindings );
			Weight = weight;

			RecalculateHashId();
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// This method should only be called when the hash has not been used previously. Updating
		/// might cause errors with object lookups.
		/// </summary>
		public void RecalculateHashId()
		{
			StringBuilder dataString = new StringBuilder();

			dataString.Append( $"{Storylet.ID}(" );

			foreach ( var (variableName, obj) in PreconditionBindings )
			{
				dataString.Append( $"{variableName}:{obj}" );
			}

			dataString.Append( ")" );

			using ( SHA256 mySHA256 = SHA256.Create() )
			{
				byte[] bytes = Encoding.UTF8.GetBytes( dataString.ToString() );

				byte[] hash = mySHA256.ComputeHash( bytes );

				string hashString = string.Empty;

				foreach ( byte x in hash )
				{
					hashString += string.Format( "{0:x2}", x );
				}

				HashID = hashString;
			}
		}

		#endregion
	}
}
