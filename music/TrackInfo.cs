namespace music
{
	public class TrackInfo
	{
		public string Title;
		public string Secondary;
		public string Artist;
		public string Duration;

		public TrackInfo()
		{
		}
		public TrackInfo(string title, string artist, string duration)
		{
			Title = title;
			Artist = artist;
			Duration = duration;
		}
	}
}
