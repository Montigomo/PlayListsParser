using Microsoft.WindowsAPICodePack.Dialogs;
using PlayListsParser.PlayLists;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;


namespace PlayListsParser
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		public MainWindow()
		{
			InitializeComponent();
			Title = App.AppTitle;
			BindGrid();
			SetBindings();
		}

		#region

		TextWriter _writer = null;

		public static void Log(string message)
		{

		}

		private void Logg(string message)
		{
			//Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => this.progressBarMain.Value = e.ProgressPercentage));
		}

		#endregion


		#region Bindings

		private void SetBindings()
		{
			// {Binding ElementName=wndMain, Path=OutputFolder}
			//var binding = new Binding();
			//binding.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Window), 1);
			//binding.Source = this;
			//binding.ElementName = "wndMain";
			//binding.Path = new PropertyPath(nameof(OutputFolder));
			//binding.Mode = BindingMode.TwoWay;
			//textBoxOutFolder.SetBinding(TextBlock.TextProperty, binding);


			//var converter = new NBoolYoBoolConverter();
			//var binding = new Binding(".");
			//binding.Mode = BindingMode.TwoWay;
			//binding.Converter = converter;
			//checkBoxEmptyFolder.SetBinding(CheckBox.IsCheckedProperty, binding);
		}

		bool _initGrid = false;

		private void BindGrid()
		{

			dataGridMain.ItemsSource = PlayLists;

			if (!_initGrid)
			{
				dataGridMain.AutoGenerateColumns = false;

				dataGridMain.Columns.Add(new DataGridTextColumn() { Binding = new Binding("Name"), Header = "Name" });
				dataGridMain.Columns.Add(new DataGridTextColumn() { Binding = new Binding("Title"), Header = "Title" });
				dataGridMain.Columns.Add(new DataGridTextColumn() { Binding = new Binding("FilePath"), Header = "Path" });
				_initGrid = true;
			}
		}
		
		#endregion

		#region Properties

		private string FindPlaylistsRegex = @"(?<pre>((Av\.)|(A\.)))(?<name>[A-Za-z0-9]+)\.(?<ext>wpl|m3u)";

		internal IEnumerable<PlayList> PlayLists
		{
			get
			{
				return PlayListsFodler.GetPlayLists(FindPlaylistsRegex);
			}
		}


		private bool _emptyFolder = AppSettings.Instance.EmptyFolder;

		public bool EmptyFolder
		{
			get
			{
				return _emptyFolder;
			}
			set
			{
				_emptyFolder = value;
				AppSettings.Instance.EmptyFolder = _emptyFolder;
				AppSettings.Instance.Save();
				RaisePropertyChanged();
			}
		}


		private bool _removeDuplicates = AppSettings.Instance.RemoveDuplicates;

		public bool RemoveDuplicates
		{
			get
			{
				return _removeDuplicates;
			}
			set
			{
				_removeDuplicates = value;
				AppSettings.Instance.RemoveDuplicates = _removeDuplicates;
				AppSettings.Instance.Save();
				RaisePropertyChanged();
			}
		}

		private string _playListsFolder;

		public string PlayListsFodler
		{
			get
			{
				if (string.IsNullOrWhiteSpace(_playListsFolder))
					_playListsFolder = AppSettings.Instance.PlaylistsFolder;
				return _playListsFolder;
			}
			set
			{
				_playListsFolder = value;
				AppSettings.Instance.PlaylistsFolder = _playListsFolder;
				AppSettings.Instance.Save();
				RaisePropertyChanged();
			}
		}

		private string _outputFolder;

		public string OutputFolder
		{
			get
			{
				if (string.IsNullOrWhiteSpace(_outputFolder))
					_outputFolder = AppSettings.Instance.OutputFolder;
				return _outputFolder;
			}
			set
			{
				_outputFolder = value;
				AppSettings.Instance.OutputFolder = _outputFolder;
				AppSettings.Instance.Save();
				RaisePropertyChanged();
			}
		}

		#endregion

		#region PropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		private void RaisePropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}


		#endregion

		#region Backgroundworkwer

		private void InitBackgroundWorker()
		{
			//backgroundWorker.WorkerReportsProgress = true;
			//backgroundWorker.ProgressChanged += ProgressChanged;
			//backgroundWorker.DoWork += DoWork;
			//// not required for this question, but is a helpful event to handle
			//backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
		}

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
			Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => this.progressBarMain.Value = e.ProgressPercentage));
		}

		private void ProgressBarInit(int max)
		{
			Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
			{
				Console.WriteLine("Init progress bar.");
				progressBarMain.Minimum = 0;
				progressBarMain.Maximum = max;
				PlayList.ProgressChanged += ProgressChanged;
				progressBarMain.Visibility = Visibility.Visible;
			}));
		}

		private void ProgressBarHide()
		{
			Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => progressBarMain.Visibility = Visibility.Hidden));
		}

		#endregion

		#region Open Close file

		private void PlayListsFolderPickup()
		{
			var dialog = new CommonOpenFileDialog() { IsFolderPicker = true, InitialDirectory = PlayListsFodler };
			//OpenFileDialog openFileDialog = new OpenFileDialog() { Filter = "Wmp Playlists|*.wpl;*.m3u", InitialDirectory = };
			if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
			{
				PlayListsFodler = dialog.FileName;
				BindGrid();
			}
		}

		private void OutputFolderPickup()
		{

			var dialog = new CommonOpenFileDialog() { IsFolderPicker = true, InitialDirectory = OutputFolder };

			if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
			{
				OutputFolder = dialog.FileName;
			}
		}

		#endregion
		
		#region Events

		private void buttonOpenFile_Click(object sender, RoutedEventArgs e)
		{
			PlayListsFolderPickup();
		}

		private void buttonSave_Click(object sender, RoutedEventArgs e)
		{
			OutputFolderPickup();
		}

		private async void buttonStart_Click(object sender, RoutedEventArgs e)
		{
			//ProgressBarInit(100);
			//ProgressChanged(null, new ProgressChangedEventArgs(25, null));

			if(EmptyFolder)
			{
				DirectoryInfo di = new DirectoryInfo(OutputFolder);

				foreach (FileInfo file in di.GetFiles())
				{
					file.Delete();
				}

				foreach (DirectoryInfo dir in di.GetDirectories())
				{
					SetAttributesNormal(dir);
					dir.Delete(true);
				}
			}

			await PlayList.SaveItemsAsync(PlayLists, OutputFolder, 
				(fn) => 
				{
					var result = string.Empty;
					var match = Regex.Match(fn, FindPlaylistsRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase);
					if (match.Success)
						result = match.Groups["name"].Value;
					return result;
				},
				(max) => ProgressBarInit(max)).ContinueWith((v) => ProgressBarHide());

		}


		private void SetAttributesNormal(DirectoryInfo dir)
		{
			foreach (var subDir in dir.GetDirectories())
				SetAttributesNormal(subDir);
			foreach (var file in dir.GetFiles())
			{
				file.Attributes = FileAttributes.Normal;
			}
		}
		#endregion

		private void checkBoxEmptyFolder_Click(object sender, RoutedEventArgs e)
		{
			AppSettings.Instance.EmptyFolder = checkBoxEmptyFolder.IsChecked.HasValue ? checkBoxEmptyFolder.IsChecked.Value : false;
		}

		private void wndMain_Loaded(object sender, RoutedEventArgs e)
		{
			// Instantiate the writer
			_writer = new TextBoxStreamWriter(textBoxLog);
			// Redirect the out Console stream
			Console.SetOut(_writer);

			//Console.WriteLine("Now redirecting output to the text box");
		}

		private void buttonRename_Click(object sender, RoutedEventArgs e)
		{
			foreach (var item in PlayLists)
				item.Rename();
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
			bool? result = false;

			return result;
		}

	}

	#endregion

	public class TextBoxStreamWriter : TextWriter
	{
		TextBox _output = null;

		public TextBoxStreamWriter(TextBox output)
		{
			_output = output;
		}

		public override void Write(char value)
		{
			base.Write(value);
			_output.Dispatcher.BeginInvoke(DispatcherPriority.Background, 
				new Action(() => _output.AppendText(value.ToString())));
			// When character data is written, append it to the text box.
		}

		public override Encoding Encoding
		{
			get { return System.Text.Encoding.UTF8; }
		}
	}

}
