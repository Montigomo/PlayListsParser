


namespace PlaylistParser.Playlist
{
	public class PlaylistItem
	{

		public PlaylistItem()
		{

		}

		public PlaylistItem(string uri)
		{
			Path = uri;
		}

		public string Path { get; set; }

		public string AbsolutePath { get; set; }

		public string RelativePath { get; set; }

		public string FileName => System.IO.Path.GetFileNameWithoutExtension(Path);

	}
}
