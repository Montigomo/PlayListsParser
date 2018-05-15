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

		private Lazy<XmlDocument> _document = new Lazy<XmlDocument>();

		private XmlDocument Document
		{
			get
			{
				if (!_document.IsValueCreated)
				{
					_document.Value.Load(FilePath);
				}
				return _document.Value;
			}
		}

		#endregion

		#region Methods

		public void Parse()
		{


			XmlNodeList playlistNodes = Document.GetElementsByTagName("seq");
			XmlNodeList files;
			var playlistFolder = Path.GetDirectoryName(FilePath);

			Items = new List<PlayListItem>();

			if (playlistNodes.Count > 0)
			{
				files = playlistNodes[0].ChildNodes;
				for (int i = 0; i < files.Count; i++)
				{
					var filePath = files[i].Attributes["src"].Value;
					if (!File.Exists(filePath))
						filePath = Path.GetFullPath(playlistFolder + "\\" + files[i].Attributes["src"].Value);
					Items.Add(new PlayListItem() { Path = filePath });
				}
			}

			Title = Document.SelectSingleNode("//title").InnerText;

			return;
		}

		#endregion


	}
}
