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
		bool SaveFiles(string folderPath);

		Task<bool> SaveFilesAsync(string folderPath);

		event ProgressChangedEventHandler ProgressChanged;

		List<PlayListItem> Items { get; }

		string FilePath { get; }

		string Title { get; }

		string Name { get; }
	}



}
