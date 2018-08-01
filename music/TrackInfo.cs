using System;

namespace music
{
	public class TrackInfo
	{
		public string Title;
		public string Secondary;
		public string Artist;
		public string Duration;
		public string Bitrate;
		public DateTime AddedToPlaylist;
		public DateTime Downloaded;
		public bool IsDownloaded;
		public bool Skip;
		public string TitleFromZaycev;

		public TrackInfo()
		{
			Bitrate = "";
			IsDownloaded = false;
			Skip = false;
			Downloaded = DateTime.MinValue;
			AddedToPlaylist = DateTime.UtcNow;
			TitleFromZaycev = "";
		}

		public TrackInfo(string title, string artist, string duration, string bitrate, DateTime addedToPlaylist, DateTime dowloaded, bool isDownloaded, bool skip, string titleFromZaycev)
		{
			Title = title;
			Artist = artist;
			Duration = duration;
			Bitrate = bitrate;
			AddedToPlaylist = addedToPlaylist;
			Downloaded = dowloaded;
			IsDownloaded = isDownloaded;
			Skip = skip;
			TitleFromZaycev = titleFromZaycev;
		}
	}
}
