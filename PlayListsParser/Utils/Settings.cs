using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;
using System.IO;
using System.Windows;

namespace PlayListsParser
{
	public class AppSettings
	{


		#region Instance

		private static SettingStore Store { get; set; } = SettingStore.File;

		private static bool _subcribeOnExit = true;

		public static readonly string ExePath = Assembly.GetExecutingAssembly().Location;

		private static string _xmlFileName = @"Settings.xml";

		private static string _xmlFilePath = System.IO.Path.Combine((System.IO.Path.GetDirectoryName(ExePath)), _xmlFileName);

		private static readonly Lazy<AppSettings> _instance = new Lazy<AppSettings>(Load);

		public static AppSettings Instance { get { return _instance.Value; } }

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
					using (FileStream fs = new FileStream(_xmlFilePath, FileMode.Open))
						sws = (AppSettings)Serializer.Deserialize(fs);
				}
				else
				{
					sws = Deserialize<AppSettings>(PlayListsParser.Properties.Settings.Default["AppSettings"].ToString());
				}
			}
			catch { }
			return sws;
		}

		public void Save()
		{
			if (Store == SettingStore.File)
			{
				using (TextWriter twriter = new StreamWriter(_xmlFilePath))
				{
				  Serializer.Serialize(twriter, AppSettings.Instance);
				  twriter.Close();
				}
			}
			else
			{
				PlayListsParser.Properties.Settings.Default["AppSettings"] = Serialize<AppSettings>(AppSettings.Instance);
				PlayListsParser.Properties.Settings.Default.Save();
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
				AppSettings.Instance.Save();
		}

		#region Values

		public string PlaylistsFolder { get; set; }

		public string OutputFolder { get; set; }

		public bool RemoveDuplicates { get; set; }

		public bool EmptyFolder { get; set; }

		#endregion


	}

	enum SettingStore
	{
		Properties,
		File
	}
}
