using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

// ReSharper disable once CheckNamespace
namespace PlaylistParser.PlayLists
{
	internal class PlaylistParserWpl : PlaylistParserBase, IPlaylistParser
	{

		#region Constructor

		public PlaylistParserWpl(string filePath) : base(filePath)
		{
			Parse();
		}

		#endregion


		#region Properties

		//public event ProgressChangedEventHandler ProgressChanged;

		private WplPlaylist Playlist
		{
			get;
			set;
		}


		public override List<PlayListItem> Items
		{
			get
			{
				return Playlist.Items.Select(i => new PlayListItem() { Path = i }).ToList();
			}

			set => Playlist.Items = value.Select(i => i.Path);
		}

		#endregion


		#region SavePlaylist

		public void SavePlaylist(string uri, bool overwrite = false)
		{
			uri = uri ?? FilePath;

			if (String.IsNullOrWhiteSpace(uri))
				throw new ArgumentNullException("uri");

			Playlist.Save(uri, overwrite);
		}

		#endregion

		#region Add

		public void Add(string uri, string name)
		{

		}

		#endregion

		#region Parse

		public void Parse()
		{
			Playlist = WplPlaylist.Load(FilePath);

			Title = Playlist.Title;

			return;
		}

		#endregion


	}
}
