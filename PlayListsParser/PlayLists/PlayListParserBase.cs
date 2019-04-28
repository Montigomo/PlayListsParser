using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace PlayListsParser.PlayLists
{
	public class PlaylistParserBase
	{

		#region Constructor & Parse

		public PlaylistParserBase(string filePath)
		{
			FilePath = filePath;
			Name = Path.GetFileNameWithoutExtension(filePath);

		}

		private void Parse() { }

		#endregion

		#region EventHandler

		public event ProgressChangedEventHandler ProgressChanged;

		protected void RaiseProgressChangedEvent(ProgressChangedEventArgs e = null)
		{
			ProgressChanged?.Invoke(this, e);
		}

		#endregion

		#region Properties

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

			protected set => _items = value;

		}

		#endregion

		#region SaveFiles

		public bool SaveFiles(string folderPath)
		{
			var totalCount = Items.Count;

			foreach (var item in Items)
			{
				var filePathDest = Path.GetFullPath(folderPath + "\\" + Path.GetFileName(item.Path));

				Directory.CreateDirectory(Path.GetDirectoryName(filePathDest) ?? throw new InvalidOperationException());

				if (File.Exists(filePathDest))
					RemoveReadOnlyAttribute(filePathDest);

				try
				{
					if(!(AppSettings.Instance.UseTask && File.Exists(filePathDest)))
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
				catch (Exception)
				{
					throw;
				}


				RaiseProgressChangedEvent();// new ProgressChangedEventArgs((currentCount * 100) / totalCount, null));
			}

			return true;
		}

		public Task<bool> SaveFilesAsync(string folderPath)
		{
			return Task.Run(() => SaveFiles(folderPath));
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
