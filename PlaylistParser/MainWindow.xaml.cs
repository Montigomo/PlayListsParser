using PlaylistParser.Playlist;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Threading.Tasks;

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

			PropertyGridMain.SelectedObject = AppSettings.Instance;

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
			Application.Current?.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
			{
				if (ProgressBarMain != null)
					ProgressBarMain.Value = e.ProgressPercentage;
			}));
		}

		private void ProgressBarInit(int max)
		{
			Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
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
			this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
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

		#endregion


		#region Folders pickup

		//private void PlayListsFolderPickup()
		//{
		//	var dialog = new CommonOpenFileDialog() { IsFolderPicker = true, InitialDirectory = PlayListsFodler };
		//	//OpenFileDialog openFileDialog = new OpenFileDialog() { Filter = "Wmp Playlists|*.wpl;*.m3u", InitialDirectory = };
		//	if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
		//	{
		//		PlayListsFodler = dialog.FileName;
		//		BindGrid();
		//	}
		//}

		//private void OutputFolderPickup()
		//{
		//	var dialog = new CommonOpenFileDialog() { IsFolderPicker = true, InitialDirectory = OutputFolder };
		//	if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
		//	{
		//		OutputFolder = dialog.FileName;
		//	}
		//}

		#endregion


		#region TEST

		#region Check && Repair

		private async void Check()
		{
			await Task.WhenAll(Library.Select(item => item.CheckAsync()));
		}

		private void RepairToggle()
		{
			this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
			{
				this.menuItemRepare.IsEnabled = Library.Any(item => item.IsNeedRepair);
			}));
		}

		#endregion

		private void Test()
		{
			var playlist = PlaylistBase.Create(@"D:\\music\\Playlists\\A.Ambient.wpl");
			if(playlist != null)
			{
				playlist.Repair();
			}

			playlist.SavePlaylist();
		}

		#endregion


		#region Events

		private async void RunPlayListItemsSaveAsync(bool cancel = false)
		{
			ToggleControls(menuItemRun);

			await PlaylistBase.SaveItemsAsync(AppSettings.Instance.OutputFolder,ProgressBarInit).ContinueWith((v) => ProgressBarHide());

			ToggleControls(menuItemRun);
		}


		private void wndMain_Loaded(object sender, RoutedEventArgs e)
		{
			// Instantiate the writer
			//_writer = new TextBoxStreamWriter(TextBoxLog);

			// Redirect the out Console stream
			Console.SetOut(_writer);

			//Console.WriteLine("Now redirecting output to the text box");
		}

		private void SysMenuItem_ItemClick_1(object sender, EventArgs e)
		{

			SettingWindow setWindow = new SettingWindow { Owner = this };

			setWindow.ShowDialog();
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
				RunPlayListItemsSaveAsync(true);
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

		}

		private void menuItemAdd_Click(object sender, RoutedEventArgs e)
		{

		}

		private void menuItemCheck_Click(object sender, RoutedEventArgs e)
		{
			Check();
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
