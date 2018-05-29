using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using PlayListsParser.PlayLists;

namespace PlayListsParser
{
	static class Extensions
	{

		public static bool ToBoolean(this bool? value)
		{
			return value ?? false;
		}

		public static IEnumerable<PlayList> GetPlayLists(this string folderPath, string regexString)
		{
			if (!string.IsNullOrWhiteSpace(folderPath) && File.GetAttributes(folderPath).HasFlag(FileAttributes.Directory))
			{

				var items = Directory.GetFiles(folderPath)
                    .Where(d => Regex.IsMatch(Path.GetFileName(d), regexString, RegexOptions.Compiled | RegexOptions.IgnoreCase));

				foreach (var item in items)
					yield return new PlayList(item);
			}
		}

        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (T item in enumeration)
            {
                action(item);
            }
        }

    }
}
