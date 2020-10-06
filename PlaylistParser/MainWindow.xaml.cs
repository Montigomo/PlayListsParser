using Microsoft.WindowsAPICodePack.Dialogs;
using PlaylistParser.Playlist;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace PlaylistParser
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : INotifyPropertyChanged
	{

		#region Constructor & Log

		public MainWindow()
		{

			InitializeComponent();

			Initialize();

		}

		TextWriter _writer;

		private void Initialize()
		{
			_writer = new TextBoxStreamWriter(TextBoxLog);

			Title = App.AppTitle;

			Binding();

			//PropertyGridMain.SelectedObject = AppSettings.Instance;

			AppSettings.Instance.PropertyChanged += SettingsPropertyChanged;

			PlaylistBase.PropertyChangedLibrary += PlayList_PropertyChangedStatic;

			RepairToggle();
		}

		private void PlayList_PropertyChangedStatic(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case nameof(IPlaylist.IsNeedRepair):
					RepairToggle();
					break;
				default:
					break;
			}
		}

		private void SettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case nameof(AppSettings.PlaylistsFolder):
					RefreshPlaylists();
					break;
				case nameof(AppSettings.PlsFilter):
					RefreshPlaylists();
					break;
				default:
					break;
			}
		}


		private void RefreshPlaylists()
		{
			PlaylistBase.Refresh();
			Binding();
		}

		#endregion


		#region Bindings

		private void Binding()
		{
			BindGrid();
		}

		bool _initGrid;

		private void BindGrid()
		{

			var dgBinding = new Binding()
			{
				Source = Library,
				Mode = BindingMode.OneWay
			};

			DataGridMain.SetBinding(ItemsControl.ItemsSourceProperty, dgBinding);

			#region Create Datagrid columns

			if (!_initGrid)
			{
				DataGridMain.AutoGenerateColumns = false;

				DataGridMain.Columns.Add(new DataGridTextColumn()
				{
					Binding = new Binding("Name") { Mode = BindingMode.OneWay },
					Header = "Name"
				});

				//DataGridMain.Columns.Add(new DataGridTextColumn()
				//{
				//	Binding = new Binding("Title") { Mode = BindingMode.OneWay },
				//	Header = "Title"
				//});

				DataGridMain.Columns.Add(new DataGridTextColumn()
				{
					Binding = new Binding("PlaylistPath") { Mode = BindingMode.OneWay },
					Header = "Path"
				});

				DataGridMain.Columns.Add(new DataGridCheckBoxColumn()
				{
					Binding = new Binding("Process") { Mode = BindingMode.TwoWay },
					Header = CreateHeader("Extract", "Extract playlist items")
				});

				DataGridMain.Columns.Add(new DataGridCheckBoxColumn()
				{
					Binding = new Binding("WillRepair") { Mode = BindingMode.TwoWay },
					Header = CreateHeader("Repair", "Repair playlist from invalid path items")
				});

				_initGrid = true;
			}

			#endregion

		}

		private TextBlock CreateHeader(string text, string tooltip = null)
		{
			var tb = new TextBlock()
			{
				Text = text
			};

			if (!String.IsNullOrWhiteSpace(tooltip))
				tb.ToolTip = CreateTooltip(tooltip);

			return tb;
		}

		private ToolTip CreateTooltip(string content)
		{
			return new ToolTip()
			{
				Content = content,
				Background = new SolidColorBrush() { Color = Colors.LightGoldenrodYellow }
			};
		}


		#endregion


		#region Properties

		internal ObservableCollection<IPlaylist> Library => PlaylistBase.Playlists;

		#endregion


		#region PropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion


		#region Backgroundworkwer

		//private void InitBackgroundWorker()
		//{
		//backgroundWorker.WorkerReportsProgress = true;
		//backgroundWorker.ProgressChanged += ProgressChanged;
		//backgroundWorker.DoWork += DoWork;
		//// not required for this question, but is a helpful event to handle
		//backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
		//}

		//private BackgroundWorker backgroundWorker = new BackgroundWorker();

		//private void DoWork(object sender, DoWorkEventArgs e)
		//{

		//	_playList.SaveItems();
		//	for (int i = 0; i <= 100; i++)
		//	{
		//		Thread.Sleep(100);
		//		backgroundWorker.ReportProgress(i);
		//	}
		//}

		//private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		//{
		//	// This is called on the UI thread when the DoWork method completes
		//	// so it's a good place to hide busy indicators, or put clean up code
		//}

		#endregion


		#region ProgressBar

		private void ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
			{
				if (ProgressBarMain != null)
					ProgressBarMain.Value = e.ProgressPercentage;
			}));
		}

		private void ProgressBarInit(int max)
		{
			this.Dispatcher.Invoke(DispatcherPriority.Send, new Action(() =>
			{
				Console.WriteLine(@"Init progress bar.");
				ProgressBarMain.Value = 0;
				ProgressBarMain.Minimum = 0;
				ProgressBarMain.Maximum = max;
				PlaylistBase.ProgressChangedLibrary += ProgressChanged;
				ProgressBarMain.Visibility = Visibility.Visible;
			}));
		}

		private void ProgressBarHide()
		{
			this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
			{
				if (ProgressBarMain != null)
					ProgressBarMain.Visibility = Visibility.Hidden;
			}));
		}

		#endregion


		#region ToggleControls

		private void ToggleControls(params Control[] excludeControls)
		{
			var tc = this.FindVisualChildren<Control>().Where(ctrl =>
			{
				return ctrl.Tag != null && ctrl.Tag.ToString().StartsWith("toggle");
			}).ToList();

			foreach (var control in tc)
			{
				if (!excludeControls.Contains(control))
					control.IsEnabled = !control.IsEnabled;
				else
				{
					string[] tcarray = control.Tag.ToString().Split('|');
					if (tcarray.Length > 1)
					{
						string header = control.GetDependencyPropertyValue("Header", control) as string;
						if (header != null)
						{
							control.SetDependencyPropertyValue("Header", header == tcarray[1] ? tcarray[2] : tcarray[1]);
						}
					}
				}
			}
		}

		#endregion


		#region Check && Repair

		private async void Check()
		{
			if (Library == null)
			{
				Console.WriteLine("Playlists is empty.".WriteError());
				return;
			}
			ToggleControls(menuItemCheck);

			await Task.WhenAll(Library.Select(item => item.CheckAsync()));

			ToggleControls(menuItemCheck);
		}

		private void RepairToggle()
		{
			this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
			{
				var value = Library == null ? false : Library.Any(item => item.IsNeedRepair);
				this.menuItemRepare.IsEnabled = value;
				this.menuItemReparePreview.IsEnabled = value;
			}));
		}

		#endregion


		#region TEST

		private void Test()
		{
			var s10 = @"D:\music\Playlists\A.2005 - The Very Best Of Mozart.wpl";
			//var s11 = @"D:\music\Playlists\A.Pop.m3u";
			//var s12 = @"..\..\music\Ajad\Reiki Music Collection - 5CD\Ajad - Reiki Music Vol.5\\Ajad - 01 - Night of Love.mp3";
			//var s13 = @"D:\music\Ajad\Reiki Music Collection - 5CD\Ajad - Reiki Music Vol.5\\Ajad - 01 - Night of Love.mp3";

			//var s21 = s11.GetAbsolutePath(s12);
			//var s22 = s11.GetAbsolutePath(s13);

			//var s31 = s11.GetRelativePath(s12);
			//var s32 = s11.GetRelativePath(s13);


			var playlist0 = PlaylistBase.Create(s10);
			////var playlist1 = Library.FirstOrDefault(item => item.Title == "A.Pop");

			////var s0 = Path.GetPathRoot(playlist0.PlaylistPath);
			////var s1 = Path.GetPathRoot(playlist1.PlaylistPath);

			var t0 = playlist0.Check();

			//var t1 = playlist0.Check();

			//playlist0.Repair();
			//playlist0.SavePlaylist();

			//t1 = playlist0.Check();
		}

		#endregion


		#region Events

		private async void RunPlayListItemsSaveAsync()
		{
			ToggleControls(menuItemRun);

			await PlaylistBase.SaveAllItemsAsync(AppSettings.Instance.OutputFolder, ProgressBarInit).ContinueWith((v) => ProgressBarHide());

			ToggleControls(menuItemRun);
		}

		private void Cancel()
		{
			PlaylistBase.Cancel();
		}

		private void wndMain_Loaded(object sender, RoutedEventArgs e)
		{
			// Instantiate the writer
			//_writer = new TextBoxStreamWriter(TextBoxLog);

			// Redirect the out Console stream
			Console.SetOut(_writer);

			//Console.WriteLine("Now redirecting output to the text box");
		}

		private void columnHeader_Click(object sender, RoutedEventArgs e)
		{

			if (sender is DataGridColumnHeader columnHeader)
			{
				string header = (columnHeader.Content as TextBlock)?.Text;
				if (columnHeader.DisplayIndex == 2 && header == "Extract")
				{
					Library.ForEach(t => t.Process = !t.Process);
					//e.Handled = true;
				}
				else if (columnHeader.DisplayIndex == 3 && header == "Repair")
				{
					Library.ForEach(t => t.WillRepair = !t.WillRepair);
					//e.Handled = true;
				}
			}

		}
		private void DataGridMain_Sorting(object sender, DataGridSortingEventArgs e)
		{

			string header = (e.Column.Header as TextBlock)?.Text;
			if ((e.Column.DisplayIndex == 2 && header.Equals("Extract"))
				 || (e.Column.DisplayIndex == 3 && header.Equals("Repair")))
			{
				//PlayLists.ForEach(t => t.Prepare = !t.Prepare);
				e.Handled = true;
			}

		}

		private void menuItemRun_Click(object sender, RoutedEventArgs e)
		{

			if (menuItemRun.Header.ToString() == "Run")
			{
				RunPlayListItemsSaveAsync();
			}
			else if (menuItemRun.Header.ToString() == "Cancel")
			{
				Cancel();
			}

		}

		private void _testMenuItem_Click(object sender, RoutedEventArgs e)
		{
			Test();
		}


		#endregion

		private void MainMenu_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{

		}

		private void menuItemDgRefresh_Click(object sender, RoutedEventArgs e)
		{
			RefreshPlaylists();
		}

		private void menuItemRepare_Click(object sender, RoutedEventArgs e)
		{
			PlaylistBase.RepairAll();
		}

		private void menuItemAdd_Click(object sender, RoutedEventArgs e)
		{

		}

		private void menuItemCheck_Click(object sender, RoutedEventArgs e)
		{
			Check();
		}

		private void menuItemReparePreview_Click(object sender, RoutedEventArgs e)
		{
			PlaylistBase.RepairAll(false);
		}

		private void menuItemOpenFile_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new CommonOpenFileDialog()
			{
				IsFolderPicker = false,
				InitialDirectory = AppSettings.Instance.MenuItemOpenFolder,
				Multiselect = true
			};

			if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
			{
				//AppSettings.Instance.MenuItemOpenFolder = dialog.FileNames;
				PlaylistBase.AddItems(dialog.FileNames);
			}
		}

		private void menuItemOpenFolder_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new CommonOpenFileDialog()
			{
				IsFolderPicker = true,
				InitialDirectory = AppSettings.Instance.MenuItemOpenFolder
			};

			if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
			{
				AppSettings.Instance.MenuItemOpenFolder = dialog.FileName;
				PlaylistBase.AddItems(dialog.FileName);
			}
		}

		private void menuItemSettings_Click(object sender, RoutedEventArgs e)
		{
			WindowSettings settingsWindow = new WindowSettings();
			Nullable<bool> dlgResult = settingsWindow.ShowDialog();

		}
	}

	#region Converters

	public class NBoolYoBoolConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo cultureInfo)
		{
			return true;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo cultureInfo)
		{
			return false;
		}

	}

	#endregion

}
