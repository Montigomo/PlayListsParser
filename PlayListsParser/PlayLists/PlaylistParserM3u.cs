using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// ReSharper disable once CheckNamespace
namespace PlayListsParser.PlayLists
{
    public class PlaylistParserM3u : PlaylistParserBase, IPlaylistParser
	{

		#region Constructor

		public PlaylistParserM3u(string filePath): base(filePath)
		{
			Parse();
		}

		#endregion

		#region Properties

		//public event ProgressChangedEventHandler ProgressChanged;

		#endregion

		#region Methods

		private void Parse()
		{

			var playlistFolder = Path.GetDirectoryName(FilePath);


			Items = new List<PlayListItem>();

			foreach (string filePath in File.ReadAllLines(FilePath, Encoding.GetEncoding(1251)))
			{
				if (filePath.StartsWith(@"#EXTM3U"))
				{

				}
				else if (filePath.StartsWith(@"#EXTINF"))
				{

				}
				else if (!String.IsNullOrWhiteSpace(filePath))
				{
					Items.Add(new PlayListItem() { Path = Path.GetFullPath(playlistFolder + "\\" + filePath) });
				}
			}

			Title = Path.GetFileNameWithoutExtension(FilePath);
		}

		#endregion

	}
}
