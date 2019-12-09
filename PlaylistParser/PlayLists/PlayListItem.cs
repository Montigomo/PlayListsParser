


namespace PlaylistParser.Playlist
{
	public class PlayListItem
	{

		public PlayListItem()
		{
		}

		public PlayListItem(string uri, string absoluteruri)
		{
			Path = uri;
			AbsolutePath = absoluteruri;
		}

		public string Path { get; set; }

		public string AbsolutePath { get; set; }

		public string FileName => System.IO.Path.GetFileNameWithoutExtension(Path);

	}
}
