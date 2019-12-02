using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using PlayListsParser;

namespace PlayListsParser.PlayLists
{
	class WplPlaylist
	{

		#region Constructor

		public WplPlaylist()
		{
		}

		/// <summary>
		/// Mad constructor with multi action depans on its parameters
		/// </summary>
		/// <param name="uri"></param>
		/// <param name="items"></param>
		/// Case 1 - uri is folder, items = null, action - scan all files in folder and add its to new play list
		/// Case 2 - uri is existing playlist, items = null, action - read existing playlist(uri)
		/// Case 3 - uri is existing playlist, items = null, action - create playlist(uri) and then overwrite it
		/// Case 4 - uri is file path (exists or not), items != null, action -  create new playlist and add to it all items
		//public WplPlaylist(string uri, string items = null)
		//{

		//	PlaylistPath = uri;

		//	if (uri.IsDirectory())
		//	{

		//	}
		//	else if(File.Exists(uri))
		//	{

		//	}
		//	Document = LoadXDoc(uri);
		//}

		public static WplPlaylist Load(string uri)
		{
			var wplplaylist = new WplPlaylist() { Document = LoadXDoc(uri), PlaylistPath = uri };
			return wplplaylist;
		}

		private static XDocument LoadXDoc(string uri)
		{
			if (File.Exists(uri) && Path.GetExtension(uri) == ".wpl")
			{
				var xdoc = XDocument.Load(uri);


				return xdoc;
			}
			return null;
		}

		public static WplPlaylist Create(string uri, string title = null, List<string> items = null)
		{
			return new WplPlaylist() { Document = CreateXDoc(uri, title, items), PlaylistPath = uri };
		}

		private static XDocument CreateXDoc(string uri, string title, List<string> items)
		{
			title = title ?? Path.GetFileNameWithoutExtension(uri);

			var xdoc = new XDocument(
				new XProcessingInstruction("wpl", "version=\"1.0\""),
				new XElement("smil",
					new XElement("head",
						new XElement("meta", new XAttribute("name", "Generator"), new XAttribute("content", Generator)),
						new XElement("meta", new XAttribute("name", "ItemCount"), new XAttribute("content", items == null ? 0 : items.Count)),
						new XElement("title", title)
					)
				)
			);
			Add(xdoc, items);
			return xdoc;
		}

		private static void Add(XDocument xdoc, IEnumerable<string> items)
		{

			if (items != null && items.Count() > 0)
			{
				xdoc.XPathSelectElement("/smil").Add(
					new XElement("body",
						new XElement("seq", items.Select(f => new XElement("media", new XAttribute("src", f))))
					)
				);
			}
		}

		private void Add(IEnumerable<string> items)
		{
			Add(Document, items);
		}

		#endregion

		#region Properties & Members

		private static string Generator { get; set; } = App.AppTitle;

		public string PlaylistPath { get; private set; }

		public string PlaylistFolder
		{
			get => Path.GetDirectoryName(PlaylistPath);
		}

		public string Title
		{
			get => Document?.XPathSelectElement("/smil/head/title").Value;
		}

		private XDocument _document = new XDocument();

		private XDocument Document
		{
			get => _document;
			set => _document = value;
		}

		#endregion

		#region Items

		public IEnumerable<string> Items
		{
			get
			{
				return Document?.XPathSelectElements("/smil/body/seq/media").Select(c => Extensions.GetAbsolutePathSimple(PlaylistPath, c.Attribute("src").Value));
			}
			set
			{
				Add(value);
			}
		}

		#endregion

		#region Save

		public void Save(string uri = null, bool overwrite = false)
		{
			uri = uri ?? PlaylistPath;

			if (uri == null)
				throw new ArgumentNullException($@"{MethodBase.GetCurrentMethod().Name} path can't be null.");

			XmlWriterSettings xws = new XmlWriterSettings
			{
				OmitXmlDeclaration = true,
				Indent = true
			};
			using (XmlWriter xw = XmlWriter.Create(uri, xws))
				Document.Save(xw);
		}

		#endregion


		#region Create Playlist

		#endregion


		#region Files

		private bool FilterFiles(string file)
		{

			Regex FilterFile = new Regex(@"^[\w\-. ]+(\.mp4|\.wmv|\.mov|\.avi|\.mp3)$");

			return FilterFile.IsMatch(file);
		}

		private string StripFile(string oldFile, string path)
		{
			Regex SpecialChars = new Regex(@"[^a-zA-Z0-9._ ]+");
			//if (SpecialChars.IsMatch(oldFile))
			//{
			//	var newFile = AddFilePrefix(SpecialChars.Replace(oldFile, "_"));
			//	var newFilePath = MakeFilePath(newFile, path);
			//	if (!File.Exists(newFilePath))
			//	{
			//		var oldFilePath = MakeFilePath(oldFile, path);
			//		File.Copy(oldFilePath, newFilePath);
			//	}
			//	oldFile = newFile;
			//}
			return oldFile;
		}

		private List<string> GetFiles(DirectoryInfo di)
		{
			List<string> files = new List<string>();
			string stripedFile;
			string path = di.FullName;
			foreach (var file in di.GetFiles())
			{
				if (FilterFiles(file.Name))
				{
					stripedFile = StripFile(file.Name, path);
					files.Add(stripedFile);
				}
			}

			return files;
		}

		#endregion


	}
}
