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
namespace PlayListsParser
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
		public string OutputFolder { get; set; }

		//[PropertyOrder(1)]
		//[Category("General")]
		//[Description("This property is a complex property and has no default editor.")]
		//public string FilterRegex { get; set; } = @"(?<pre>((Av\.)|(A\.)))(?<name>[A-Za-z0-9.]+)\.(?<ext>wpl|m3u)";


		[PropertyOrder(1)]
		[Category("General")]
		[Editor(typeof(PgEditorFolderRegex), typeof(PgEditorFolderRegex))]
		[Description("This property is a complex property and has no default editor.")]
		public string PlsFilter { get; set; }


		[Browsable(false)]
		public PlsFolderFilterList PlsFilterCollection { get; set; } = new PlsFolderFilterList();


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
		public bool RemoveDuplicates { get; set; }

		[Category("General")]
		[Description("Clear destination folder.")]
		public bool ClearFolder { get; set; }

		////[Editor(typeof(FirstNameEditor), typeof(FirstNameEditor))]
		////public string FirstName { get; set; }

		////[Editor(typeof(PgEditorFile), typeof(PgEditorFile))]
		////public string LastName { get; set; }

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


	#region PlsFolderFilterList

	public class PlsFolderFilterList : ICollection<string>, INotifyPropertyChanged, INotifyCollectionChanged
	{

		public event PropertyChangedEventHandler PropertyChanged;
		
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		private Dictionary<int, string> _dictionary = new Dictionary<int, string>();

		private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private List<int> UsedCounter
		{
			get { return _dictionary.Keys.ToList(); }
		}

		private object _lock = new object();

		[XmlAttribute("index")]
		public int Index
		{
			get;
			set;
		}

		#region ICollection implimentation

		public int Count => _dictionary.Count;

		public bool IsReadOnly => throw new NotImplementedException();

		public void Add(string item)
		{
			int index;
			Add(item, out index);
		}

		public void Add(string item, out int index)
		{
			index = -1;
			if (!_dictionary.ContainsValue(item))
			{
				lock (_lock)
				{
					index = _dictionary.Count == 0 ? 0 : _dictionary.Keys.Max() + 1;
					_dictionary.Add(index, item);
				}
			}
			CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,item));
			return;
		}

		public void Clear()
		{
			_dictionary.Clear();
			CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		public bool Contains(string item)
		{
			return _dictionary.ContainsValue(item);
		}

		public void CopyTo(string[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<string> GetEnumerator()
		{
			return _dictionary.Values.GetEnumerator();
		}

		public bool Remove(string item)
		{
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _dictionary.Values.GetEnumerator();
		}

		#endregion

		public string this[int key]
		{
			get
			{
				int entry = _dictionary.Keys.Contains(key) ? _dictionary.Keys.FirstOrDefault(_key => _key == key) : -1;
				if (entry >= 0)
					return _dictionary[entry];
				//ThrowHelper.ThrowKeyNotFoundException();
				return default(string);
			}

			set
			{
				_dictionary[key] = value;
			}
		}

	}

	#endregion


	#region Enums

	enum SettingStore
	{
		Properties,
		File
	}

	#endregion

}
