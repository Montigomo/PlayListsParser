using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;

// ReSharper disable once CheckNamespace
namespace PlayListsParser.PlayLists
{
	interface IPlaylistParser
	{
		void SavePlaylist(string location, bool overwrite);

		bool SaveItems(string folderPath);

		Task<bool> SaveItemsAsync(string folderPath);

		event ProgressChangedEventHandler ProgressChanged;

		List<PlayListItem> Items { get; set; }

		void Add(string uri, string name);

		string FilePath { get; }

		string Title { get; }

		string Name { get; }
	}
	 
}
