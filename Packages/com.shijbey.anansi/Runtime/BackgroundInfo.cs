namespace Anansi
{
	public class BackgroundInfo
	{
		public string BackgroundId { get; }
		public string[] Tags { get; }

		public BackgroundInfo(string backgroundId, string[] tags)
		{
			this.BackgroundId = backgroundId;
			this.Tags = tags;
		}
	}
}
