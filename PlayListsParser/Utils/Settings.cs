using System;
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

// ReSharper disable once CheckNamespace
namespace PlayListsParser
{
	[Description("Settings")]
	public class AppSettings : INotifyPropertyChanged
	{

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
		public string PlaylistsFolder
		{
			get => _playListsFolder;
			set
			{
				_playListsFolder = value;
				NotifyPropertyChanged();

			}
		}

		[Category("General")]
		[Editor(typeof(PgEditorFile), typeof(PgEditorFile))]
		public string OutputFolder { get; set; }

		[Category("General")]
		public bool RemoveDuplicates { get; set; }

		[Category("General")]
		[Description("Clear destination folder.")]
		public bool ClearFolder { get; set; }

		////[Editor(typeof(FirstNameEditor), typeof(FirstNameEditor))]
		////public string FirstName { get; set; }

		////[Editor(typeof(PgEditorFile), typeof(PgEditorFile))]
		////public string LastName { get; set; }

		[Category("General")]
		[Description("This property is a complex property and has no default editor.")]
		public string FindPlaylistsRegex { get; set; } = @"(?<pre>((Av\.)|(A\.)))(?<name>[A-Za-z0-9]+)\.(?<ext>wpl|m3u)";


		[Category("General")]
		[Description("Use Task<T>.")]
		public bool UseTask { get; set; } = true;


		private bool _useOneFolder = false;

		[Category("General")]
		[Description("Use one folder")]
		public bool UseOneFolder
		{
			get => _useOneFolder;
			set
			{
				_useOneFolder = value;
			}
		}

		//[Category("General")]
		//[Description("Remove duplicates")]
		//public bool RemoveDuplicates
		//{			get; set;		}

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
	enum SettingStore
	{
		Properties,
		File
	}
}
