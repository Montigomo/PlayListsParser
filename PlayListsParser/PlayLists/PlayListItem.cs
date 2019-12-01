


namespace PlayListsParser.PlayLists
{
	public class PlayListItem
	{
		public string Path { get; set; }

		public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);

	}
}
