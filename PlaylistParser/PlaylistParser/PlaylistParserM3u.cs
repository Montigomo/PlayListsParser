using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using PlaylistParser;

// ReSharper disable once CheckNamespace
namespace PlaylistParser.PlayLists
{
	public class PlaylistParserM3u : PlaylistParserBase, IPlaylistParser
	{

		#region Constructor

		public PlaylistParserM3u(string filePath) : base(filePath)
		{
			Parse();
		}



		#endregion


		#region Properties

		//public event ProgressChangedEventHandler ProgressChanged;


		#region Items

		public override List<PlayListItem> Items { get; set; } = new List<PlayListItem>();


		#endregion

		#endregion


		#region SavePlaylist

		public void SavePlaylist(string uri = null, bool overwrite = true)
		{
			// m3u header
			string _headerm3u = $"#EXTM3U{Environment.NewLine}#PLAYLIST:{{0}}{Environment.NewLine}";

			// 0 - track length sec
			// 1 - track name
			// 2 - path to file
			string _linem3u = $"#EXTINF:{{0}},{{1}}{Environment.NewLine}{{2}}{Environment.NewLine}";

			uri = uri ?? PlaylistPath;

			FileAttributes attr = File.GetAttributes(uri);

			if (attr.HasFlag(FileAttributes.Directory))
				uri = System.IO.Path.Combine(uri, System.IO.Path.GetFileName(PlaylistPath));

			StringBuilder sbn = new StringBuilder();

			sbn.AppendLine(String.Format(_headerm3u, Title));

			foreach (var item in Items)
			{
				try
				{
					string itemPath = AbsoluteFilePath ? item.Path : Extensions.GetRelativePath(uri, item.Path);
					string line = String.Format(_linem3u, 0, item.Name, itemPath);
					sbn.AppendLine(line);
				}
				catch (System.IO.FileNotFoundException)
				{ }

			}

			string testPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(uri), System.IO.Path.GetFileNameWithoutExtension(uri) + ".test" + System.IO.Path.GetExtension(uri));

			System.IO.File.WriteAllText(testPath, sbn.ToString());

			return;
		}

		#endregion


		#region Add

		public void Add(string uri, string name = null)
		{
			Items.Add(new PlayListItem() { Path = uri });
		}

		#endregion


		#region Parse

		private void Parse()
		{
			if (!File.Exists(PlaylistPath))
				return;

			//Items = new List<PlayListItem>();

			//var playlistFolder = System.IO.Path.GetDirectoryName(FilePath);

			foreach (string filePath in File.ReadAllLines(PlaylistPath, Encoding.GetEncoding(1251)))
			{
				//try
				//{
				if (filePath.StartsWith(@"#EXTM3U"))
				{

				}
				else if (filePath.StartsWith(@"#EXTINF"))
				{

				}
				else if (filePath.StartsWith(@"#PLAYLIST:"))
				{

				}
				else if (!String.IsNullOrWhiteSpace(filePath))
				{
					if(Path.IsPathRooted(filePath))
						Add(filePath);
					else
						Add(Extensions.GetAbsolutePathSimple(PlaylistPath, filePath));
				}
				//}
				//catch(Exception e)
				//{

				//}
			}

			Title = System.IO.Path.GetFileNameWithoutExtension(PlaylistPath);
		}

		#endregion

	}
}
