using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using PlaylistParser;

// ReSharper disable once CheckNamespace
namespace PlaylistParser.Playlist
{
	internal class PlaylistWpl : PlaylistBase, IPlaylist
	{

		#region Constructor

		public PlaylistWpl(string filePath) : base(filePath)
		{
			Parse();
		}

		#endregion


		#region Properties


		#endregion


		#region SavePlaylist

		public void SavePlaylist(bool overwrite)
		{
			if (String.IsNullOrWhiteSpace(PlaylistPath))
				throw new ArgumentNullException(nameof(PlaylistPath));

			var uriNew = NewPath;

			XmlWriterSettings xws = new XmlWriterSettings
			{
				OmitXmlDeclaration = true,
				Indent = true
			};

			ActualizeXItems(AppSettings.Instance.PlaylistItemPathFormat == PlaylistItemPath.Absolute);

			try
			{
				using (var xw = XmlWriter.Create(uriNew, xws))
				{
					Document.Save(xw);
				}
			}
			catch
			{
				File.Delete(uriNew);
				throw;
			}

			File.Copy(uriNew, PlaylistPath, true);

			File.Delete(uriNew);

		}

		#endregion


		#region Parse

		public void Parse()
		{
			Document = LoadXDoc(PlaylistPath);
			ActualizeItems();
			return;
		}

		private void ActualizeItems()
		{
			Items = new List<PlaylistItem>();
			var setx = Document?.XPathSelectElements("/smil/body/seq/media").Select(c => c.Attribute("src").Value);
			foreach (var item in setx)
				Add(item);
		}

		#endregion


		#region Static methods

		public static PlaylistWpl Load(string uri)
		{
			var wplplaylist = new PlaylistWpl(uri) { Document = LoadXDoc(uri), PlaylistPath = uri };
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

		public static PlaylistWpl Create(string uri, string title = null, List<string> items = null)
		{
			return new PlaylistWpl(uri) { Document = CreateXDoc(uri, title, items), PlaylistPath = uri };
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
			if (items != null && items.Count() > 0)
			{
				AddXBody(xdoc);
				SetXItems(xdoc, items);
			}
			return xdoc;
		}

		private static void AddXBody(XDocument xdoc)
		{
			xdoc.XPathSelectElement("/smil")?.Add(new XElement("body"));
		}

		private static void AddXItems(XDocument xdoc, IEnumerable<string> items)
		{
			if (items != null && items.Count() > 0)
			{
				xdoc.XPathSelectElement("/smil/body")?.Add(new XElement("seq", ItemsToMedia(items)));
			}
		}

		private static void SetXItems(XDocument xdoc, IEnumerable<string> items)
		{
			if (items != null && items.Count() > 0)
			{
				xdoc.XPathSelectElement("/smil/body")?.ReplaceNodes(new XElement("seq", ItemsToMedia(items)));
			}
		}

		private static XElement ItemsToSeq(IEnumerable<string> items)
		{
			return new XElement("seq", ItemsToMedia(items));
		}

		private static IEnumerable<XElement> ItemsToMedia(IEnumerable<string> items)
		{
			return items.Select(f => new XElement("media", new XAttribute("src", f)));
		}

		#endregion


		#region Add && Actualize

		private void AddX(IEnumerable<string> items)
		{
			AddXItems(Document, items);
		}

		private void ActualizeXItems(bool absolute)
		{
			SetXItems(Document, Items.Select(item => absolute ? item.AbsolutePath : item.RelativePath));
		}

		#endregion


		#region Properties & Members

		private static string Generator { get; set; } = App.AppTitle;


		public string PlaylistFolder
		{
			get => Path.GetDirectoryName(PlaylistPath);
		}

		public override string Title
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

		//private List<PlaylistItem> _items;

		//public override List<PlaylistItem> Items
		//{
		//	get
		//	{
		//		{
		//			if (_items == null)
		//			{
		//				_items = new List<PlaylistItem>(Document?.XPathSelectElements("/smil/body/seq/media")
		//					.Select(c => new PlaylistItem()
		//					{
		//						Path = c.Attribute("src").Value,
		//						AbsolutePath = PlaylistPath.GetAbsolutePath(c.Attribute("src").Value)
		//					}));
		//			}
		//			return _items;
		//		}
		//	}
		//	set
		//	{
		//		_items = value;
		//	}
		//}

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
