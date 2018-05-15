using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PlayListsParser.PlayLists
{
	public class PlayListItem
	{
		public string Path { get; set; }

		public string Name
		{
			get
			{
				return System.IO.Path.GetFileNameWithoutExtension(Path);
			}
		}

	}
}
