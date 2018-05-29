using PlayListsParser.PlayLists;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;

namespace PlayListsParser
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
            Title = App.AppTitle;
            Binding();
            propertyGridMain.SelectedObject = AppSettings.Instance;
        }

        TextWriter _writer;

        public static void Log(string message)
        {

        }

        #endregion

        #region Bindings

        private void Binding()
        {
            BindGrid();
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

        bool _initGrid;

        private void BindGrid()
        {

            dataGridMain.ItemsSource = PlayLists;

            if (!_initGrid)
            {
                dataGridMain.AutoGenerateColumns = false;
                dataGridMain.Columns.Add(new DataGridCheckBoxColumn()
                {
                    Binding = new Binding("Prepare") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.TwoWay },
                    //CanUserSort = false,
                    Header = "#"
                });
                dataGridMain.Columns.Add(new DataGridTextColumn() { Binding = new Binding("Name") { Mode = BindingMode.OneWay }, Header = "Name" });
                dataGridMain.Columns.Add(new DataGridTextColumn() { Binding = new Binding("Title") { Mode = BindingMode.OneWay }, Header = "Title" });
                dataGridMain.Columns.Add(new DataGridTextColumn() { Binding = new Binding("FilePath") { Mode = BindingMode.OneWay }, Header = "Path" });
                _initGrid = true;
            }
        }

        #endregion

        #region Properties

        private string FindPlaylistsRegex = @"(?<pre>((Av\.)|(A\.)))(?<name>[A-Za-z0-9]+)\.(?<ext>wpl|m3u)";

        private ObservableCollection<PlayList> _playLists;

        internal ObservableCollection<PlayList> PlayLists
        {
            get
            {
                if (_playLists == null)
                    _playLists = new ObservableCollection<PlayList>(AppSettings.Instance.PlaylistsFolder.GetPlayLists(FindPlaylistsRegex));
                return _playLists;
            }
            set
            {
                _playLists = value;
            }
        }


        ////private bool _emptyFolder = AppSettings.Instance.EmptyFolder;

        ////public bool EmptyFolder
        ////{
        ////    get
        ////    {
        ////        return _emptyFolder;
        ////    }
        ////    set
        ////    {
        ////        _emptyFolder = value;
        ////        AppSettings.Instance.EmptyFolder = _emptyFolder;
        ////        AppSettings.Instance.Save();
        ////        RaisePropertyChanged();
        ////    }
        ////}


        //private bool _removeDuplicates = AppSettings.Instance.RemoveDuplicates;

        //public bool RemoveDuplicates
        //{
        //    get
        //    {
        //        return _removeDuplicates;
        //    }
        //    set
        //    {
        //        _removeDuplicates = value;
        //        AppSettings.Instance.RemoveDuplicates = _removeDuplicates;
        //        AppSettings.Instance.Save();
        //        RaisePropertyChanged();
        //    }
        //}

        //private string _playListsFolder;

        //public string PlayListsFodler
        //{
        //    get
        //    {
        //        if (string.IsNullOrWhiteSpace(_playListsFolder))
        //            _playListsFolder = AppSettings.Instance.PlaylistsFolder;
        //        return _playListsFolder;
        //    }
        //    set
        //    {
        //        _playListsFolder = value;
        //        AppSettings.Instance.PlaylistsFolder = _playListsFolder;
        //        AppSettings.Instance.Save();
        //        RaisePropertyChanged();
        //    }
        //}

        //private string _outputFolder;

        //public string OutputFolder
        //{
        //    get
        //    {
        //        if (string.IsNullOrWhiteSpace(_outputFolder))
        //            _outputFolder = AppSettings.Instance.OutputFolder;
        //        return _outputFolder;
        //    }
        //    set
        //    {
        //        _outputFolder = value;
        //        AppSettings.Instance.OutputFolder = _outputFolder;
        //        AppSettings.Instance.Save();
        //        RaisePropertyChanged();
        //    }
        //}

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
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => progressBarMain.Value = e.ProgressPercentage));
        }

        private void ProgressBarInit(int max)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                Console.WriteLine(@"Init progress bar.");
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

        #region Events

        private async void RunPlayListItemsSaveAsync()
        {
            if (AppSettings.Instance.EmptyFolder)
            {
                DirectoryInfo di = new DirectoryInfo(AppSettings.Instance.OutputFolder);

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

            await PlayList.SaveItemsAsync(PlayLists, AppSettings.Instance.OutputFolder,
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

        private void wndMain_Loaded(object sender, RoutedEventArgs e)
        {
            // Instantiate the writer
            _writer = new TextBoxStreamWriter(textBoxLog);

            // Redirect the out Console stream
            Console.SetOut(_writer);

            //Console.WriteLine("Now redirecting output to the text box");
        }

        private void SysMenuItem_ItemClick_1(object sender, EventArgs e)
        {
            SettingWindow setWindow = new SettingWindow();
            setWindow.Owner = this;
            setWindow.ShowDialog();
        }

        private void columnHeader_Click(object sender, RoutedEventArgs e)
        {
            var columnHeader = sender as DataGridColumnHeader;
            if (columnHeader != null)
            {
                if (columnHeader.DisplayIndex == 0 && columnHeader.Content.ToString() == "#")
                    PlayLists.ForEach(t => t.Prepare = !t.Prepare);

                e.Handled = true;
            }
        }

        private void _runMenuItem_Click(object sender, RoutedEventArgs e)
        {
            RunPlayListItemsSaveAsync();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in PlayLists)
                item.Rename();
        }

        #endregion

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

    #region TextBoxStreamWriter

    public class TextBoxStreamWriter : TextWriter
    {
        readonly TextBox _output;

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

        public override Encoding Encoding => Encoding.UTF8;

    }

    #endregion

}
