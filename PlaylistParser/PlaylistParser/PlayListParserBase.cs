using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace PlaylistParser.PlayLists
{
	public class PlaylistParserBase
	{


		#region Constructor & Parse

		public PlaylistParserBase(string filePath)
		{
			PlaylistPath = filePath;
			Name = System.IO.Path.GetFileNameWithoutExtension(filePath);

		}

		protected bool AbsoluteFilePath = false;


		#endregion


		#region EventHandler


		#region PropertyChanges

		public event PropertyChangedEventHandler PropertyChanged;

		// This method is called by the Set accessor of each property.
		// The CallerMemberName attribute that is applied to the optional propertyName
		// parameter causes the property name of the caller to be substituted as an argument.
		private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		//protected void RaisePropertyChangedEvent(PropertyChangedEventArgs e = null)
		//{
		//	PropertyChanged?.Invoke(this, e);
		//}

		#endregion

		public event ProgressChangedEventHandler ProgressChanged;

		protected void RaiseProgressChangedEvent(ProgressChangedEventArgs e = null)
		{
			ProgressChanged?.Invoke(this, e);
		}

		#endregion


		#region Properties & Members

		public string Name { get; protected set; }

		public string Title { get; protected set; }

		public string PlaylistPath { get; protected set; }

		//private List<PlayListItem> _items = new List<PlayListItem>();

		public virtual List<PlayListItem> Items { get; set; }

		private bool _isNeedRepair = false;

		public virtual bool IsNeedRepair
		{
			get
			{
				return _isNeedRepair;
			}
			private set
			{
				if (value != _isNeedRepair)
				{
					_isNeedRepair = value;
					NotifyPropertyChanged();
				}
			}
		}

		#endregion


		#region SavePlaylist

		//public bool SavePlaylist(string location, bool overwrite = true) => SavePlaylistRaw(location, overwrite);

		//protected virtual bool SavePlaylistRaw(string location, bool overwrite) => true;

		#endregion


		#region Check && Repair

		public bool Check()
		{
			var result = true;

			foreach (var item in Items)
			{
				if (!File.Exists(item.Path))
				{
					result = false;
					IsNeedRepair = true;
				}
			}
			return result;
		}

		public void Repair()
		{

		}

		#endregion

		#region SaveItemsRaw

		/// <summary>
		/// Save all playlist items to the destination folder
		/// </summary>
		/// <param name="folderPath"></param>
		/// <returns></returns>
		/// 
		public bool SaveItems(string folderPath)
		{
			//var totalCount = Items.Count;

			foreach (var item in Items)
			{
				string fileName = System.IO.Path.GetFileName(item.Path);

				var filePathDest = System.IO.Path.GetFullPath(folderPath + "\\" + fileName);

				var fileExtension = System.IO.Path.GetExtension(fileName);

				try
				{
					var tagVar = TagLib.File.Create(item.Path);
					var artist = String.Join(", ", tagVar.Tag.Performers);
					var title = tagVar.Tag.Title;

					if (!string.IsNullOrWhiteSpace(artist) && !string.IsNullOrWhiteSpace(title))
					{
						fileName = $@"{artist} - {title}";

						fileName = fileName.SanitizePath();

						filePathDest = System.IO.Path.GetFullPath(folderPath + "\\" + fileName + fileExtension);
					}
				}
				catch (Exception e)
				{
					Console.WriteLine($@"{Name} - {e.Message}");
				}

				Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePathDest) ?? throw new InvalidOperationException());

				if (File.Exists(filePathDest))
					RemoveReadOnlyAttribute(filePathDest);

				try
				{
					File.Copy(item.Path, filePathDest, true);
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

				RaiseProgressChangedEvent();// new ProgressChangedEventArgs((currentCount * 100) / totalCount, null));
			}

			return true;
		}

		/// <summary>
		/// Save all playlist items to the destination folder async
		/// </summary>
		/// <param name="folderPath"></param>
		/// <returns></returns>
		public Task<bool> SaveItemsAsync(string folderPath)
		{
			return Task.Run(() => SaveItems(folderPath));
		}

		#endregion


		#region Remove files Attributes

		protected void RemoveReadOnlyAttribute(string path)
		{
			if (!File.Exists(path))
				return;

			FileAttributes attributes = File.GetAttributes(path);

			if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
			{
				// Make the file RW
				attributes = RemoveAttribute(attributes, FileAttributes.ReadOnly);
				File.SetAttributes(path, attributes);
				Console.WriteLine(@"The {0} file is no longer RO.", path);
			}
			//else
			//{
			//	// Make the file RO
			//	File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.Hidden);
			//	Console.WriteLine("The {0} file is now RO.", path);
			//}
		}

		private FileAttributes RemoveAttribute(FileAttributes attributes, FileAttributes attributesToRemove)
		{
			return attributes & ~attributesToRemove;
		}

		#endregion

	}
}
