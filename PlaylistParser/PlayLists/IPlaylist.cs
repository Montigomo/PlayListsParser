using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;

// ReSharper disable once CheckNamespace
namespace PlaylistParser.Playlist
{
	public interface IPlaylist
	{
		/// <summary>
		/// Save playlist to it's location
		/// </summary>
		/// <param name="uri"></param>
		/// <param name="overwrite"></param>
		void SavePlaylist(bool overwrite = false);

		/// <summary>
		/// Save playlist items to the folderPath
		/// </summary>
		/// <param name="folderPath"></param>
		/// <returns></returns>
		bool SaveItems(string folderPath, Action<int> progressInit = null);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="folderPath"></param>
		/// <returns></returns>
		Task<bool> SaveItemsAsync(string folderPath, Action<int> progressInit = null);

		/// <summary>
		/// 
		/// </summary>
		void Repair(bool preview = false);

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		bool Check();

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		Task CheckAsync();

		List<PlaylistItem> Items { get; set; }

		string PlaylistPath { get; }

		string Title { get; }

		string Name { get; }

		bool Process { get; set; }

		bool WillRepair { get; set; }

		bool IsNeedRepair { get; }

		event ProgressChangedEventHandler ProgressChanged;

		event PropertyChangedEventHandler PropertyChanged;

	}
}
