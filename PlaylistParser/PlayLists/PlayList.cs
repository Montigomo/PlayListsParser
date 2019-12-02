using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace PlaylistParser.PlayLists
{
	internal class PlayList : INotifyPropertyChanged
	{

		#region Static save methods

		static PlayList()
		{
			AppSettings.Instance.PropertyChanged += SettingsPropertyChanged;

		}

		public static void Refresh()
		{
			if (_playLists != null)
				_playLists.Clear();
			_playLists = new ObservableCollection<PlayList>(AppSettings.Instance.PlaylistsFolder.GetPlayLists(AppSettings.Instance.PlsFilter));
		}

		private static void SettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(AppSettings.PlaylistsFolder))
			{
				_playLists = null;
			}
		}

		private static ObservableCollection<PlayList> _playLists;

		internal static ObservableCollection<PlayList> PlayLists
		{
			get
			{
				if (_playLists == null)
					Refresh();
				return _playLists;
			}
		}

		internal static Task SavePlaylistsAsync(string outFolder, Func<string, string> getFolder, Action<int> pbInit)
		{
			return Task.Run(() => SavePlaylists(outFolder, getFolder, pbInit));
		}

		private static void SavePlaylists(string outFolder, Func<string, string> getFolder, Action<int> pbInit)
		{

			var watch = System.Diagnostics.Stopwatch.StartNew();

			var workedPlaylists = PlayLists.Where(p => p.Prepare).ToList();

			var count = workedPlaylists.Aggregate(0, (result, element) => result + element.Items.Count);

			pbInit(count);

			if (AppSettings.Instance.UseTask)
				Task.WaitAll(workedPlaylists.Select(t => t.SaveItemsAsync(outFolder, getFolder)).ToArray());
			else
				foreach (var item in workedPlaylists)
					item.SaveItems(outFolder, getFolder);

			watch.Stop();

			Console.WriteLine($@"Execution time: {watch.Elapsed.Hours} hours  {watch.Elapsed.Minutes} minutes {watch.Elapsed.Seconds} seconds");
			_pbProgress = 0;
		}

		public static event ProgressChangedEventHandler ProgressChanged;

		#endregion

		#region Properties

		private IPlaylistParser _parser;

		public List<PlayListItem> Items => _parser.Items.ToList();

		private bool _prepare = true;

		public bool Prepare
		{
			get => _prepare;
			set
			{
				_prepare = value;
				NotifyPropertyChanged();
			}
		}

		public string FilePath => _parser.FilePath;

		public string Name => _parser.Name;

		public string Title => _parser.Title;

		public string OutFolder { get; set; }

		private bool _repair = false;

		public bool Repair
		{
			get => _repair;
			set
			{
				_repair = value;
				NotifyPropertyChanged();
			}
		}

		private bool _needRepair = false;

		public bool NeedRepair
		{
			get => _needRepair;
			set
			{
				_needRepair = true;
				NotifyPropertyChanged();
			}
		}

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

		private static int _pbProgress = 0;

		private void _parser_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			_pbProgress = _pbProgress + 1;
			e = new ProgressChangedEventArgs(_pbProgress, null);

			ProgressChanged?.Invoke(sender, e);
		}

		#endregion

		#region SavePlaylist

		internal void SavePlaylist(string location = null, bool overwrite = true)
		{
			_parser.SavePlaylist(location, overwrite);
		}


		#endregion

		#region Save Items

		private Task SaveItemsAsync(string outFolder, Func<string, string> getFolder)
		{
			return Task.Factory.StartNew(() =>
					{
						SaveItems(outFolder, getFolder);
					},
					TaskCreationOptions.LongRunning);
		}

		internal void SaveItems(string outFolder, Func<string, string> getFolder)
		{

			if (!AppSettings.Instance.UseOneFolder)
			{
				var folderName = getFolder(FilePath);
				OutFolder = System.IO.Path.Combine(outFolder, folderName);
			}
			else
				OutFolder = outFolder;

			if (!String.IsNullOrWhiteSpace(OutFolder))
				_parser.SaveItems(OutFolder);
		}

		#endregion

		#region PropertyChanges

		public event PropertyChangedEventHandler PropertyChanged;

		// This method is called by the Set accessor of each property.
		// The CallerMemberName attribute that is applied to the optional propertyName
		// parameter causes the property name of the caller to be substituted as an argument.
		private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

	}
}
