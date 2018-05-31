﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;

namespace PlayListsParser.PlayLists
{
    internal class PlayList
	{

        #region Static save methods

	    static PlayList()
	    {
	        AppSettings.Instance.PropertyChanged += SettingsPropertyChanged;

	    }

	    private static void SettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
	    {
	        if (e.PropertyName == nameof(AppSettings.PlaylistsFolder))
	        {
	            _playLists = null;
	        }
	    }

	   //private ObservableCollection<PlayList> _playLists;
	    private static List<PlayList> _playLists;

	    internal static List<PlayList> PlayLists
	    {
	        get
	        {
	            if (_playLists == null)
	                _playLists = AppSettings.Instance.PlaylistsFolder.GetPlayLists(AppSettings.Instance.FindPlaylistsRegex).ToList();
	            return _playLists;
	        }
	    }

        internal static Task SaveItemsAsync(string outFolder, Func<string, string> getFolder, Action<int> pbInit)
		{
			return Task.Run(() => SaveItems(outFolder, getFolder, pbInit));
		}

		private static void SaveItems(string outFolder, Func<string, string> getFolder, Action<int> pbInit)
		{

			var watch = System.Diagnostics.Stopwatch.StartNew();

		    //var playLists = files as PlayList[] ?? files.ToArray();

		    var count = PlayLists.Aggregate(0, (result, element) => result + element.Items.Count);

			pbInit(count);

			Task.WaitAll(PlayLists.Where(p=> p.Prepare).Select(t => t.SaveItemsAsync(outFolder, getFolder)).ToArray());

			//foreach (var item in files)
			//	item.SaveItems(outFolder, getFolder);

			watch.Stop();

			Console.WriteLine($@"Execution time: {watch.Elapsed.Hours} hours  {watch.Elapsed.Minutes} minutes {watch.Elapsed.Seconds} seconds");
		}

		public static event ProgressChangedEventHandler ProgressChanged;

		#endregion

		#region Properties

		private IPlaylistParser _parser;

		public List<PlayListItem> Items => _parser.Items;

	    public bool Prepare { get; set; } = true;

	    public string FilePath => _parser.FilePath;

	    public string Name => _parser.Name;

	    public string Title => _parser.Title;

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
				File.Move(FilePath, Path.Combine(Path.GetDirectoryName(FilePath) ?? throw new InvalidOperationException(), Title + Path.GetExtension(FilePath)));
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

		private Task SaveItemsAsync(string outFolder, Func<string, string> getFolder)
		{
			return Task.Run(() =>
			{
				SaveItems(outFolder, getFolder);
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
