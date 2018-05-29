using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;

namespace PlayListsParser.PlayLists
{
	interface IPlaylistParser
	{
		bool SaveFiles(string folderPath);

		Task<bool> SaveFilesAsync(string folderPath);

		event ProgressChangedEventHandler ProgressChanged;

		List<PlayListItem> Items { get; }

		string FilePath { get; }

		string Title { get; }

		string Name { get; }
	}

	public class PlaylistParserBase
	{

		public PlaylistParserBase(string filePath)
		{
			FilePath = filePath;
			Name = Path.GetFileNameWithoutExtension(filePath);

		}

		public event ProgressChangedEventHandler ProgressChanged;

		protected void RaiseProgressChangedEvent(ProgressChangedEventArgs e = null)
		{
			var eh = ProgressChanged;
			ProgressChanged?.Invoke(this, e);
		}

		public string Name { get; protected set; }

		public string Title { get; protected set; }

		public string FilePath { get; protected set; }

		private List<PlayListItem> _items = new List<PlayListItem>();

		public List<PlayListItem> Items
		{
			get
			{
				if (_items.Count == 0)
				{
					Parse();
				}
				return _items;
			}

			protected set
			{
				_items = value;
			}
		}

		private void Parse() { }

		public bool SaveFiles(string folderPath)
		{
			var totalCount = Items.Count;
			var currentCount = 0;
			var result = true;
			foreach (var item in Items)
			{
				var filePathDest = Path.GetFullPath(folderPath + "\\" + Path.GetFileName(item.Path));

				Directory.CreateDirectory(Path.GetDirectoryName(filePathDest));

				if (File.Exists(filePathDest))
					RemoveReadOnlyAttribute(filePathDest);

				try
				{
					File.Copy(item.Path, filePathDest, true);
				}
				catch (System.IO.DirectoryNotFoundException e)
				{
					Console.WriteLine($"{Name} - {e.Message}");
				}
				catch (System.IO.FileNotFoundException e)
				{
					Console.WriteLine($"{Name} - {e.Message}");
				}
				catch (Exception e)
				{
					throw e;
				}

				currentCount++;

				RaiseProgressChangedEvent();// new ProgressChangedEventArgs((currentCount * 100) / totalCount, null));
			}
			return result;
		}

		public Task<bool> SaveFilesAsync(string folderPath)
		{
			return Task.Run(() => SaveFiles(folderPath));
		}

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
				Console.WriteLine("The {0} file is no longer RO.", path);
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

	}

}
