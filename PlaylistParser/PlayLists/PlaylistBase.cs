using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using PlaylistParser;


namespace PlaylistParser.Playlist
{
	public class PlaylistBase : INotifyPropertyChanged, IDisposable
	{

		#region IDisposable

		bool _disposed = false;

		public void Dispose()
		{
			Dispose(true);
			//GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{

			}

			//CancelationToken.Dispose();
		}

		~PlaylistBase()
		{
			Dispose(false);
		}

		#endregion


		#region Library

		static PlaylistBase()
		{
			AppSettings.Instance.PropertyChanged += SettingsPropertyChanged;
			GC.SuppressFinalize(CancelationToken);
			Refresh();
		}

		private static void SettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(AppSettings.PlaylistsFolder))
			{
				Refresh();
			}
		}

		private static ObservableCollection<IPlaylist> _playLists = new ObservableCollection<IPlaylist>();

		public static ObservableCollection<IPlaylist> Playlists
		{
			get
			{
				return _playLists;
			}
		}

		#region SaveAllItems

		private static void CheckCancelationToken()
		{
			try
			{
				CancelationToken.Dispose();
				CancelationToken = new CancellationTokenSource();
			}
			catch (ObjectDisposedException)
			{
				CancelationToken = new CancellationTokenSource();
			}
		}

		private static void TaskCompleted(bool antecedent)
		{
			CheckCancelationToken();
		}


		public static Task SaveAllItemsAsync(string outFolder, Action<int> pbInit)
		{
			var tsc = new TaskCompletionSource<bool>();

			if (CancelationToken.Token.IsCancellationRequested)
				return Task.CompletedTask;

			//Task.Factory.StartNew(() =>
			//{
			//	SaveItems(outFolder, pbInit);
			//	tsc.SetResult(true);
			//});

			new Thread(() =>
			{
				SaveAllItems(outFolder, pbInit);
				tsc.SetResult(true);
			}).Start();

			return tsc.Task.ContinueWith((antecedent) => TaskCompleted(antecedent.Result));

			//return Task.Run(() => SaveItems(outFolder, pbInit));

			//return Task.Factory.StartNew(
			//	() => SaveItems(outFolder, pbInit),
			//	CancellationToken.None, 
			//	TaskCreationOptions.LongRunning,
			//	TaskScheduler.Default);
		}

		private static void SaveAllItems(string outFolder, Action<int> pbInit)
		{
			if (String.IsNullOrWhiteSpace(outFolder))
			{
				Console.WriteLine("Invalid output directory".WriteError());
				return;
			}

			var watch = System.Diagnostics.Stopwatch.StartNew();

			var workedPlaylists = Playlists.Where(p => p.Process).ToList();

			var count = workedPlaylists.Aggregate(0, (result, element) => result + element.Items.Count);

			if (pbInit != null)
				pbInit(count);

			if (AppSettings.Instance.ClearFolder)
			{
				DirectoryInfo di = new DirectoryInfo(AppSettings.Instance.OutputFolder);

				foreach (FileInfo file in di.GetFiles())
				{
					if (CancelationToken.Token.IsCancellationRequested)
						return;
					file.Delete();
				}

				foreach (DirectoryInfo dir in di.GetDirectories())
				{
					if (CancelationToken.Token.IsCancellationRequested)
						return;
					dir.SetAttributesNormal();
					dir.Delete(true);
				}
			}

			if (AppSettings.Instance.UseTask)
				Task.WaitAll(workedPlaylists.Select(t => t.SaveItemsAsync(t.TryGetFolder(outFolder))).ToArray());
			else
				foreach (var item in workedPlaylists)
					item.SaveItems(item.TryGetFolder(outFolder));

			watch.Stop();

			Console.WriteLine($@"Execution time: {watch.Elapsed.Hours} hours  {watch.Elapsed.Minutes} minutes {watch.Elapsed.Seconds} seconds");
			_pbProgress = 0;
		}

		#endregion

		public static void Refresh()
		{
			if (Directory.Exists(AppSettings.Instance.PlaylistsFolder))
				AddItems(AppSettings.Instance.PlaylistsFolder, true, true);
			else
				Console.WriteLine("Invalid playlists directory".WriteError());
		}

		internal static CancellationTokenSource CancelationToken { get; set; } = new CancellationTokenSource();

		public static void Cancel()
		{
			CancelationToken.Cancel();
		}

		public static void AddItems(string url, bool filter = true, bool clear = false)
		{
			if (!string.IsNullOrWhiteSpace(url))
			{
				if (url.IsDirectory())
				{
					AddItems(Directory.GetFiles(url), filter, clear);
				}
				else
				{
					AddItems(new List<string>() { url }, filter, clear);
				}
			}
		}

		public static void AddItems(IEnumerable<string> files, bool filter = true, bool clear = false)
		{
			if (clear)
				_playLists = new ObservableCollection<IPlaylist>(NamesFilter(files));
			else
				NamesFilter(files).ForEach(_playLists.Add);
		}

		private static IEnumerable<IPlaylist> NamesFilter(IEnumerable<string> items, bool filter = true)
		{
			string regex = AppSettings.Instance.PlsFilter;
			return items.Where(item =>
			{
				var result = (Path.GetExtension(item) == ".m3u" || Path.GetExtension(item) == ".wpl");
				if (filter)
					result = result && (regex.IsValidRegex() ? Regex.IsMatch(Path.GetFileName(item), regex, RegexOptions.Compiled | RegexOptions.IgnoreCase) : true);
				return result;
			}).Select(item => PlaylistBase.Create(item));
		}


		#endregion


		#region Properties

		public virtual string Name { get => Title; protected set { } }

		public virtual string Title { get; protected set; }

		public virtual string PlaylistPath { get; protected set; }

		public virtual List<PlaylistItem> Items { get; set; }

		private bool _process = true;

		/// <summary>
		/// Is playlist will be processed
		/// </summary>
		public virtual bool Process
		{
			get => _process;
			set
			{
				_process = value;
				NotifyPropertyChanged();
			}
		}

		private bool _willRepair = false;

		/// <summary>
		/// Is play list will be repaired
		/// </summary>
		public virtual bool WillRepair
		{
			get => _willRepair;
			set
			{
				_willRepair = value;
				NotifyPropertyChanged();
			}
		}


		private bool _isNeedRepair = false;

		/// <summary>
		/// Is playlist contains missed or broken items and need to repair
		/// </summary>
		public bool IsNeedRepair
		{
			get => _isNeedRepair;
			protected set
			{
				_isNeedRepair = value;
				NotifyPropertyChanged();
				NotifyPropertyChangedLibrary();
			}
		}

		#endregion


		#region Methods

		#region Add

		protected virtual void Add(string uri, string name = null)
		{
			var item = new PlaylistItem(uri);
			ActualizePathes(item);
			Items.Add(item);
		}

		#endregion

		#region Rename

		public void Rename()
		{
			if (Title != Name)
			{
				File.Move(PlaylistPath, Path.Combine(Path.GetDirectoryName(PlaylistPath) ?? throw new InvalidOperationException(), Title + Path.GetExtension(PlaylistPath)));
			}
		}

		#endregion

		#region Check && Repair

		public static void CheckAll()
		{
			Playlists.Select(item => item.Check());
		}

		public static void RepairAll(bool preview = false)
		{
			var items = Playlists.Where(item => item.IsNeedRepair && item.WillRepair);

			//Playlists.AsParallel().ForAll(item => item.Repair());

			items.ForEach(item => item.Repair(preview));

			if (!preview)
			{
				items.ForEach(item => item.SavePlaylist(true));
			}
		}

		public Task CheckAsync()
		{
			return Task.Factory.StartNew(() =>
			{
				Check();
			},
			TaskCreationOptions.LongRunning);
		}

		public bool Check()
		{
			var result = true;
			foreach (var item in Items)
				if (!CheckPath(item))
					result = false;
			IsNeedRepair = !result;
			return result;
		}

		/// <summary>
		/// Detect if AbsolutePath and Path have correct values
		/// </summary>
		/// <param name="item"></param>
		/// <returns>true if valid false if not valid</returns>
		private bool CheckPath(PlaylistItem item, bool preview = false)
		{
			var result = File.Exists(item.AbsolutePath);

			// detect if member Path absolute or not and then check it value for correct path
			if (result && !preview)
			{
				if(Path.IsPathRooted(item.Path))
				{
					result = File.Exists(item.Path);
				}
				else
				{
					var t = Extensions.GetRelativePath(PlaylistPath, item.AbsolutePath);
					result = item.Path.Equals(t);
				}
			}
			return result;
		}

		private void RepairPath()
		{
			var corrupted = Items.Where(item => !CheckPath(item));
			foreach (var item in corrupted)
			{
				if (File.Exists(item.AbsolutePath))
				{
					item.Path = Extensions.GetRelativePath(PlaylistPath, item.AbsolutePath);
				}
			}
		}

		public void Repair(bool preview = false)
		{
			int cc = 0, dc = 0;

			if (!Check())
			{
				var corrupted = Items.Where(item => !CheckPath(item));

				cc = corrupted.Count();

				if(!preview)
					RepairPath();

				var todelete = Items.Where(item => !CheckPath(item, preview));

				dc = todelete.Count();

				Console.WriteLine($@"Playlist {Title} repaired - {cc} removed - {dc}");

				if (todelete.Count() > 0)
				{
					var t = String.Join("", todelete.Select(item => $@"{item.Path} {Environment.NewLine}"));
					t = t.Substring(0, t.Length - 2);
					Console.WriteLine(t);
				}

				if (!preview)
				{
					Items.RemoveAll(item => !CheckPath(item));
				}
			}
		}

		#endregion

		protected void ActualizePathes(PlaylistItem item, string prevPath = null)
		{
			item.AbsolutePath = PlaylistPath.GetAbsolutePath(item.Path);
			item.RelativePath = PlaylistPath.GetRelativePath(item.AbsolutePath);
		}

		protected void ActualizePathesAll(string previousPath = null)
		{
			foreach (var item in Items)
			{
				ActualizePathes(item);
			}
		}


		protected virtual string NewPath
		{
			get
			{
				var uriNew = $@"{Path.GetDirectoryName(PlaylistPath)}\{Path.GetFileNameWithoutExtension(PlaylistPath)}.new{Path.GetExtension(PlaylistPath)}";
				return uriNew;
			}
		}

		#endregion


		#region Constructor && Initialize


		public static IPlaylist Create(string playlistPath)
		{
			if (!File.Exists(playlistPath))
				throw new FileNotFoundException($@"File {playlistPath} not found");

			IPlaylist playlist;
			var playlistExtension = Path.GetExtension(playlistPath);
			switch (playlistExtension)
			{
				case ".m3u":
					playlist = new PlaylistM3u(playlistPath);
					break;
				case ".wpl":
					playlist = new PlaylistWpl(playlistPath);
					break;
				default:
					throw new Exception($@"Playlist {playlistExtension} does not supprts");
			}
			return playlist;
		}

		protected PlaylistBase(string path)
		{
			Initialize(path);
		}

		private void Initialize(string filePath)
		{
			//try
			//{
			//	var rexs = @"^(?<root>[^\:]+\:)\\{2,}";
			//	if(Regex.IsMatch(filePath, rexs))
			//	{
			//		var match = Regex.Match(filePath, rexs);
			//		var roots = match.Groups["root"]?.Value;
			//		if(!String.IsNullOrWhiteSpace(roots))
			//		{
			//			filePath = Regex.Replace(filePath, rexs, roots + @"\");
			//		}
			//	}
			//}
			//catch { }
			PlaylistPath = filePath;
		}

		#endregion


		#region Events

		#region Library events

		public static event PropertyChangedEventHandler PropertyChangedLibrary;


		private void NotifyPropertyChangedLibrary([CallerMemberName] String propertyName = "")
		{
			PropertyChangedLibrary?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}


		public static event ProgressChangedEventHandler ProgressChangedLibrary;

		private static int _pbProgress = 0;

		private static void NotifyProgressChangedLibrary(object sender, ProgressChangedEventArgs e = null)
		{
			_pbProgress += 1;
			e = new ProgressChangedEventArgs(_pbProgress, null);
			ProgressChangedLibrary?.Invoke(sender, e);
		}

		#endregion

		private static int _localProgress;

		public event ProgressChangedEventHandler ProgressChanged;

		private void NotifyProgressChanged(object sender, ProgressChangedEventArgs e = null)
		{
			_localProgress += 1;
			e = new ProgressChangedEventArgs(_localProgress, null);
			ProgressChanged?.Invoke(sender, e);
		}

		#region PropertyChanges

		public event PropertyChangedEventHandler PropertyChanged;

		private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

		#endregion


		#region Save Items

		/// <summary>
		/// Save all playlist items to the destination folder
		/// </summary>
		/// <param name="folderPath"></param>
		/// <returns></returns>
		public bool SaveItems(string folderPath, Action<int> progressInit = null)
		{
			var count = Items.Count;

			_localProgress = 0;

			if (progressInit != null)
				progressInit(count);

			foreach (var item in Items)
			{
				if (CancelationToken.Token.IsCancellationRequested)
					return false;

				var fileName = System.IO.Path.GetFileName(item.Path);

				var filePathDest = System.IO.Path.GetFullPath(folderPath + "\\" + fileName);

				var fileExtension = System.IO.Path.GetExtension(fileName);

				try
				{
					var tagVar = TagLib.File.Create(item.AbsolutePath);
					var artist = String.Join(", ", tagVar.Tag.Performers);
					var title = tagVar.Tag.Title;
					var album = tagVar.Tag.Album;

					//album = album ?? 

					if (!string.IsNullOrWhiteSpace(artist) && !string.IsNullOrWhiteSpace(title))
					{
						fileName = $@"{artist} - {title}";

						if (!string.IsNullOrWhiteSpace(album))
							fileName += $@" - {album}";

						fileName = fileName.SanitizePath();

						filePathDest = System.IO.Path.GetFullPath(folderPath + "\\" + fileName + fileExtension);
					}

					Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePathDest) ?? throw new InvalidOperationException());

					if (File.Exists(filePathDest))
						filePathDest.RemoveReadOnlyAttribute();

					File.Copy(item.AbsolutePath, filePathDest, true);
				}
				catch (System.IO.DirectoryNotFoundException e)
				{
					Console.WriteLine($@"{Name} - {e.Message}");
				}
				catch (System.IO.FileNotFoundException e)
				{
					Console.WriteLine($@"{Name} - {e.Message}");
				}
				catch (System.IO.IOException ex) when (ex.HResult == unchecked((int)0x80070020) && AppSettings.Instance.UseTask)
				{
					//0x80070020
				}
				NotifyProgressChanged(this);
				NotifyProgressChangedLibrary(this);
			}

			return true;
		}

		/// <summary>
		/// Save all playlist items to the destination folder async
		/// </summary>
		/// <param name="folderPath"></param>
		/// <returns></returns>
		public Task<bool> SaveItemsAsync(string folderPath, Action<int> progressInit = null)
		{
			//return Task.Run(() => SaveItems(folderPath));
			return Task.Factory.StartNew(() => SaveItems(folderPath, progressInit), TaskCreationOptions.LongRunning);
		}



		#endregion


	}
}
