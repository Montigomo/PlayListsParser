using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.ComponentModel;
using System.Threading;

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

			var i = 0;

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

			return;
		}

		#endregion

	}
}
