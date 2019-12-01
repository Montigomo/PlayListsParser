using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
namespace PlayListsParser.PlayLists
{
	public class PlaylistParserBase
	{

		#region Constructor & Parse

		public PlaylistParserBase(string filePath)
		{
			FilePath = filePath;
			Name = System.IO.Path.GetFileNameWithoutExtension(filePath);

		}

		protected bool AbsoluteFilePath = false;


		#endregion


		#region EventHandler

		public event ProgressChangedEventHandler ProgressChanged;

		protected void RaiseProgressChangedEvent(ProgressChangedEventArgs e = null)
		{
			ProgressChanged?.Invoke(this, e);
		}

		#endregion


		#region Properties & Members

		public string Name { get; protected set; }

		public string Title { get; protected set; }

		public string FilePath { get; protected set; }

		//private List<PlayListItem> _items = new List<PlayListItem>();

		public virtual List<PlayListItem> Items { get; set; }

		#endregion


		#region SavePlaylist

		//public bool SavePlaylist(string location, bool overwrite = true) => SavePlaylistRaw(location, overwrite);

		//protected virtual bool SavePlaylistRaw(string location, bool overwrite) => true;

		#endregion


		#region SaveItemsRaw

		/// <summary>
		/// Save all playlist items to the destination folder
		/// </summary>
		/// <param name="folderPath"></param>
		/// <returns></returns>
		/// 
		public bool SaveItemsRaw(string folderPath)
		{
			//var totalCount = Items.Count;

			foreach (var item in Items)
			{
				string title = System.IO.Path.GetFileName(item.Path);

				var filePathDest = System.IO.Path.GetFullPath(folderPath + "\\" + title);

				var fileExtension = System.IO.Path.GetExtension(title);

				try
				{

					var tagVar = TagLib.File.Create(item.Path);

					var artist = String.Join(", ", tagVar.Tag.Performers);

					if (!string.IsNullOrWhiteSpace(artist) && !string.IsNullOrWhiteSpace(tagVar.Tag.Title))
					{
						title = $@"{String.Join(", ", tagVar.Tag.Performers)} - {tagVar.Tag.Title}";

						//string illegal = folderPath + "\\" + title;
						string regexSearch = new string(System.IO.Path.GetInvalidFileNameChars()) + new string(System.IO.Path.GetInvalidPathChars());

						Regex r = new Regex($"[{Regex.Escape(regexSearch)}]");

						title = r.Replace(title, "");

						filePathDest = System.IO.Path.GetFullPath(folderPath + "\\" + title + fileExtension);
					}
				}
				catch (Exception e)
				{
					//title = Path.GetFileName(item.Path);
					Console.WriteLine($@"{Name} - {e.Message}");
				}

				Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePathDest) ?? throw new InvalidOperationException());

				if (File.Exists(filePathDest))
					RemoveReadOnlyAttribute(filePathDest);

				try
				{
					if (!(AppSettings.Instance.UseTask && File.Exists(filePathDest)))
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

				RaiseProgressChangedEvent();// new ProgressChangedEventArgs((currentCount * 100) / totalCount, null));
			}

			return true;
		}

		/// <summary>
		/// Save all playlist items to the destination folder async
		/// </summary>
		/// <param name="folderPath"></param>
		/// <returns></returns>
		public Task<bool> SaveItemsRawAsync(string folderPath)
		{
			return Task.Run(() => SaveItemsRaw(folderPath));
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
