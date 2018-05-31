using PlayListsParser.PlayLists;
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
            PropertyGridMain.SelectedObject = AppSettings.Instance;

            AppSettings.Instance.PropertyChanged += SettingsPropertyChanged;

        }

        TextWriter _writer;


        private  void SettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AppSettings.PlaylistsFolder))
            {
                BindGrid();
            }
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

            DataGridMain.ItemsSource = PlayLists;

            if (!_initGrid)
            {
                DataGridMain.AutoGenerateColumns = false;
                DataGridMain.Columns.Add(new DataGridCheckBoxColumn()
                {
                    Binding = new Binding("Prepare") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.TwoWay },
                    //CanUserSort = false,
                    Header = "#"
                });
                DataGridMain.Columns.Add(new DataGridTextColumn() { Binding = new Binding("Name") { Mode = BindingMode.OneWay }, Header = "Name" });
                DataGridMain.Columns.Add(new DataGridTextColumn() { Binding = new Binding("Title") { Mode = BindingMode.OneWay }, Header = "Title" });
                DataGridMain.Columns.Add(new DataGridTextColumn() { Binding = new Binding("FilePath") { Mode = BindingMode.OneWay }, Header = "Path" });
                _initGrid = true;
            }
        }

        #endregion

        #region Properties

        internal List<PlayList> PlayLists => PlayList.PlayLists;

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
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => ProgressBarMain.Value = e.ProgressPercentage));
        }

        private void ProgressBarInit(int max)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                Console.WriteLine(@"Init progress bar.");
                ProgressBarMain.Minimum = 0;
                ProgressBarMain.Maximum = max;
                PlayList.ProgressChanged += ProgressChanged;
                ProgressBarMain.Visibility = Visibility.Visible;
            }));
        }

        private void ProgressBarHide()
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => ProgressBarMain.Visibility = Visibility.Hidden));
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

            await PlayList.SaveItemsAsync(
                AppSettings.Instance.OutputFolder,
                (fn) =>
                {
                    var result = string.Empty;
                    var match = Regex.Match(fn, AppSettings.Instance.FindPlaylistsRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    if (match.Success)
                        result = match.Groups["name"].Value;
                    return result;
                },
                ProgressBarInit).ContinueWith((v) => ProgressBarHide());

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
            _writer = new TextBoxStreamWriter(TextBoxLog);

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
