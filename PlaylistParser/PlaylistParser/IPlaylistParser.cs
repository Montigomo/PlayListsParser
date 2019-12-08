using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;

// ReSharper disable once CheckNamespace
namespace PlaylistParser.PlayLists
{
	interface IPlaylistParser
	{

		void SavePlaylist(string location, bool overwrite);

		bool SaveItems(string folderPath);

		Task<bool> SaveItemsAsync(string folderPath);

		void Add(string uri, string name);

		void Repair();

		bool Check();


		event ProgressChangedEventHandler ProgressChanged;

		event PropertyChangedEventHandler PropertyChanged;

		List<PlayListItem> Items { get; set; }

		string PlaylistPath { get; }

		string Title { get; }

		string Name { get; }

		bool IsNeedRepair { get; }


	}
}
