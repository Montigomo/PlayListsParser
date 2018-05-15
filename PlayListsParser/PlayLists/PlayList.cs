using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;

namespace PlayListsParser.PlayLists
{
	internal class PlayList
	{

		#region Static save methods

		static internal Task<bool> SaveItemsAsync(IEnumerable<PlayList> files, string outFolder, Func<string, string> getFolder, Action<int> pbInit)
		{
			//var count = files.Aggregate(0, (result, element) => result + element.Items.Count);

			//Task.WhenAll(files.Select(t => t.SaveItemsAsync(outFolder, getFolder)).ToArray());

			return Task.Run(() => SaveItems(files, outFolder, getFolder, pbInit));
		}

		private static bool SaveItems(IEnumerable<PlayList> files, string outFolder, Func<string, string> getFolder, Action<int> pbInit)
		{

			// Execution time: 0 hours  2 minutes 38 seconds
			// Execution time: 0 hours  2 minutes 24 seconds

			var watch = System.Diagnostics.Stopwatch.StartNew();

			var count = files.Aggregate(0, (result, element) => result + element.Items.Count);

			pbInit(count);


			Task.WaitAll(files.Select(t => t.SaveItemsAsync(outFolder, getFolder)).ToArray());

			//foreach (var item in files)
			//	item.SaveItems(outFolder, getFolder);


			watch.Stop();

			Console.WriteLine($"Execution time: {watch.Elapsed.Hours} hours  {watch.Elapsed.Minutes} minutes {watch.Elapsed.Seconds} seconds");
			return true;
		}

		static public event ProgressChangedEventHandler ProgressChanged;

		#endregion

		#region Properties

		private IPlaylistParser _parser;

		public List<PlayListItem> Items
		{
			get
			{
				return _parser.Items;
			}
		}

		public string FilePath { get { return _parser.FilePath; } }

		public string Name { get { return _parser.Name; } }

		public string Title { get { return _parser.Title; } }

		public string OutFolder { get; set; }

		#endregion

		#region Constructor && Initialize

		public PlayList(string path)
		{
			Initialize(path);
		}

		private void Initialize(string filePath)
		{

			if (!File.Exists(filePath))
				return;

			switch (Path.GetExtension(filePath))
			{
				case ".m3u":
				default:
					_parser = new PlaylistParserM3u(filePath);
					break;
				case ".wpl":
					_parser = new PlaylistParserWpl(filePath);
					break;
			}

			_parser.ProgressChanged += _parser_ProgressChanged;

		}


		public void Rename()
		{
			if (Title != Name)
			{
				File.Move(FilePath, Path.Combine(Path.GetDirectoryName(FilePath), Title + Path.GetExtension(FilePath)));
			}
		}

		#endregion

		#region Events

		static int pbProgress = 0;

		private void _parser_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			pbProgress = pbProgress + 1;
			e = new ProgressChangedEventArgs(pbProgress, null);

			ProgressChanged?.Invoke(sender, e);
		}

		#endregion

		#region Parse & Save

		private Task<bool> SaveItemsAsync(string outFolder, Func<string, string> getFolder)
		{
			return Task.Run(() =>
			{
				SaveItems(outFolder, getFolder);
				return true;
			});
		}

		internal void SaveItems(string outFolder, Func<string, string> getFolder)
		{
			var folderName = getFolder(FilePath);
			OutFolder = System.IO.Path.Combine(outFolder, folderName);
			if (!String.IsNullOrWhiteSpace(OutFolder))
				_parser.SaveFiles(OutFolder);
		}

		#endregion

	}
}
