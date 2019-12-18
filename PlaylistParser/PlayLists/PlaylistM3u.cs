using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using PlaylistParser;

// ReSharper disable once CheckNamespace
namespace PlaylistParser.Playlist
{
	public class PlaylistM3u : PlaylistBase, IPlaylist
	{

		#region Constructor

		public PlaylistM3u(string filePath) : base(filePath)
		{
			Parse();
		}



		#endregion


		#region Properties


		#region Items

		public override List<PlaylistItem> Items { get; set; } = new List<PlaylistItem>();


		#endregion

		#endregion


		#region SavePlaylist

		public void SavePlaylist(bool overwrite )
		{

			// m3u header
			string _headerm3u = $"#EXTM3U{Environment.NewLine}#PLAYLIST:{{0}}{Environment.NewLine}";

			// 0 - track length sec
			// 1 - track name
			// 2 - path to file
			string _linem3u = $"#EXTINF:{{0}},{{1}}{Environment.NewLine}{{2}}{Environment.NewLine}";

			StringBuilder sbn = new StringBuilder();

			sbn.AppendLine(String.Format(_headerm3u, Title));

			foreach (var item in Items)
			{
				try
				{
					string itemPath = AppSettings.Instance.PlaylistItemPathFormat == PlaylistItemPath.Absolute ? item.AbsolutePath : item.RelativePath;
					string line = String.Format(_linem3u, 0, item.FileName, itemPath);
					sbn.AppendLine(line);
				}
				catch (System.IO.FileNotFoundException)
				{ }

			}

			if (AppSettings.Instance.Debug)
			{
				System.IO.File.WriteAllText(NewPath, sbn.ToString());
			}
			else
				System.IO.File.WriteAllText(PlaylistPath, sbn.ToString());

			return;
		}

		#endregion


		#region Parse

		private void Parse()
		{
			if (!File.Exists(PlaylistPath))
				return;

			foreach (string line in File.ReadAllLines(PlaylistPath, Encoding.GetEncoding(1251)))
			{

				if (line.StartsWith(@"#EXTM3U"))
				{

				}
				else if (line.StartsWith(@"#EXTINF"))
				{

				}
				else if (line.StartsWith(@"#PLAYLIST:"))
				{

				}
				else if (!String.IsNullOrWhiteSpace(line))
				{
					Add(line);
				}
			}

			Title = System.IO.Path.GetFileNameWithoutExtension(PlaylistPath);
		}

		#endregion

	}
}
