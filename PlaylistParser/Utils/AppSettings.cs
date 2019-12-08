using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;
using System.IO;
using System.Windows;
using System.ComponentModel;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Runtime.CompilerServices;
using System.Linq;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using System.Collections.Specialized;

// ReSharper disable once CheckNamespace
namespace PlaylistParser
{

	[Description("Settings")]
	public class AppSettings : INotifyPropertyChanged
	{
		//static unsafe ref T Null<T>() where T : unmanaged => ref *(T*)null;

		#region Instance

		private static SettingStore Store { get; set; } = SettingStore.File;

		private static bool _subcribeOnExit = true;

		public static readonly string ExePath = Assembly.GetExecutingAssembly().Location;

		private static string _xmlFileName = @"Settings.xml";

		private static readonly string XmlFilePath = Path.Combine((System.IO.Path.GetDirectoryName(ExePath)) ?? throw new InvalidOperationException(), _xmlFileName);

		private static readonly Lazy<AppSettings> _instance = new Lazy<AppSettings>(Load);

		public static AppSettings Instance => _instance.Value;

		private AppSettings()
		{
			if (_subcribeOnExit && Application.Current != null)
			{
				Application.Current.Exit += App_Exit;
				_subcribeOnExit = false;
			}
		}

		static AppSettings()
		{

		}

		#region Load Save
		private static AppSettings Load()
		{
			AppSettings sws = new AppSettings();
			try
			{
				if (Store == SettingStore.File)
				{
					using (FileStream fs = new FileStream(XmlFilePath, FileMode.Open))
						sws = (AppSettings)Serializer.Deserialize(fs);
				}
				else
				{
					sws = Deserialize<AppSettings>(Properties.Settings.Default["AppSettings"].ToString());
				}
			}
			catch
			{
				// ignored
			}

			return sws;
		}

		public void Save()
		{
			if (Store == SettingStore.File)
			{
				using (TextWriter twriter = new StreamWriter(XmlFilePath))
				{
					Serializer.Serialize(twriter, Instance);
					twriter.Close();
				}
			}
			else
			{
				Properties.Settings.Default["AppSettings"] = Serialize(Instance);
				Properties.Settings.Default.Save();
			}
		}

		#endregion

		#region Serialize & Deserialize

		public static string Serialize<T>(T value)
		{

			if (value == null)
			{
				return null;
			}

			XmlSerializer serializer = new XmlSerializer(typeof(T));

			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Encoding = new UnicodeEncoding(false, false); // no BOM in a .NET string
			settings.Indent = false;
			settings.OmitXmlDeclaration = false;

			using (StringWriter textWriter = new StringWriter())
			{
				using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings))
				{
					serializer.Serialize(xmlWriter, value);
				}
				return textWriter.ToString();
			}
		}

		public static T Deserialize<T>(string xml)
		{

			if (string.IsNullOrEmpty(xml))
			{
				return default(T);
			}

			XmlSerializer serializer = new XmlSerializer(typeof(T));

			XmlReaderSettings settings = new XmlReaderSettings();
			// No settings need modifying here

			using (StringReader textReader = new StringReader(xml))
			{
				using (XmlReader xmlReader = XmlReader.Create(textReader, settings))
				{
					return (T)serializer.Deserialize(xmlReader);
				}
			}
		}

		#endregion

		private static XmlSerializer _serializer;

		private static XmlSerializer Serializer
		{
			get
			{
				if (_serializer == null)
					_serializer = new XmlSerializer(typeof(AppSettings));
				return _serializer;
			}
		}

		#endregion

		private static void App_Exit(object sender, ExitEventArgs e)
		{
			if (_instance != null)
				Instance.Save();
		}

		#region Values


		private string _playListsFolder = String.Empty;

		[Category("General")]
		[Editor(typeof(PgEditorFile), typeof(PgEditorFile))]
		[PropertyOrder(0)]
		[Description("Playlists location folder. Not support recursive search.")]
		[DisplayName("Playlists folder")]
		public string PlaylistsFolder
		{
			get => _playListsFolder;
			set
			{
				_playListsFolder = value;
				NotifyPropertyChanged();
			}
		}

		[PropertyOrder(2)]
		[Category("General")]
		[Editor(typeof(PgEditorFile), typeof(PgEditorFile))]
		[Description("Folder to which will be saved playlist items or other work on them results.")]
		public string OutputFolder { get; set; }


		private string _plsFilter;

		[PropertyOrder(1)]
		[Category("General")]
		[Editor(typeof(PgEditorFolderRegex), typeof(PgEditorFolderRegex))]
		[Description("Filter regex for playlist names.")]
		[DisplayName("Playlists filter")]
		public string PlsFilter
		{
			get { return _plsFilter; }
			set
			{
				if (String.Compare(_plsFilter, value, StringComparison.Ordinal) != 0)
				{
					_plsFilter = value;
					NotifyPropertyChanged();
				}
			}
		}

		// @"(?<pre>((Av\.)|(A\.)))(?<name>[A-Za-z0-9.]+)\.(?<ext>wpl|m3u)"
		[Browsable(false)]
		public PlsFolderFilterList PlsFilterCollection { get; set; } = new PlsFolderFilterList() { "*", @"(?<pre>((Av\.)|(A\.)))(?<name>[A-Za-z0-9.]+)\.(?<ext>wpl|m3u)" };

		private int _plsFilterIndex = 0;

		[Browsable(false)]
		public int PlsFilterIndex
		{
			get
			{
				return _plsFilterIndex;
			}
			set
			{
				_plsFilterIndex = value;
				NotifyPropertyChanged();
			}
		}

		[Category("General")]
		[Description("Save only one item in output folder.")]
		public bool RemoveDuplicates { get; set; }

		[Category("General")]
		[Description("Clear destination folder.")]
		public bool ClearFolder { get; set; }

		[Category("General")]
		[Description("Use Task for work on playlists or sequential proccesing.")]
		public bool UseTask { get; set; } = true;


		private bool _useOneFolder = false;

		[Category("General")]
		[Description("Use one folder for all playlists result items or different folder for each playlist.")]
		public bool UseOneFolder
		{
			get => _useOneFolder;
			set
			{
				_useOneFolder = value;
			}
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


	#region FirstNameEditor

	//Custom editors that are used as attributes MUST implement the ITypeEditor interface.
	public class FirstNameEditor : ITypeEditor
	{
		public FrameworkElement ResolveEditor(PropertyItem propertyItem)
		{
			TextBox textBox = new TextBox { Background = new SolidColorBrush(Colors.Red) };

			//create the binding from the bound property item to the editor
			var binding = new Binding("Value")
			{
				Source = propertyItem,
				ValidatesOnExceptions = true,
				ValidatesOnDataErrors = true,
				Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay
			}; //bind to the Value property of the PropertyItem

			BindingOperations.SetBinding(textBox, TextBox.TextProperty, binding);
			return textBox;
		}
	}

	#endregion


	#region PlsFilterComboBox

	//public class PlsFilterCombobox : ComboBoxEditor
	//{

	//	protected override void SetControlProperties(PropertyItem propertyItem)
	//	{
	//		base.SetControlProperties(propertyItem);

	//		this.Editor.IsEditable = true;
	//	}

	//	protected override IEnumerable CreateItemsSource(PropertyItem propertyItem)
	//	{
	//		if (AppSettings.Instance.PlsFilterItems?.Count == 0)
	//			AppSettings.Instance.PlsFilterItems.Add(@"(?<pre>((Av\.)|(A\.)))(?<name>[A-Za-z0-9.]+)\.(?<ext>wpl|m3u)");

	//		return AppSettings.Instance.PlsFilterItems;
	//	}

	//	//protected override void SetValueDependencyProperty()
	//	//{
	//	//	this.ValueProperty = ComboBox.ItemsSourceProperty;
	//	//}
	//}

	#endregion


	#region Enums

	enum SettingStore
	{
		Properties,
		File
	}

	#endregion

}
