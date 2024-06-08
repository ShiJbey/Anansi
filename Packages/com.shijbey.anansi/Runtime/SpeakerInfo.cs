namespace Anansi
{
	public class SpeakerInfo
	{
		public string SpeakerId { get; }
		public string SpeakerName { get; }
		public string[] Tags { get; }

		public SpeakerInfo(string speakerId, string speakerName, string[] tags)
		{
			this.SpeakerId = speakerId;
			this.SpeakerName = speakerName;
			this.Tags = tags;
		}
	}
}
