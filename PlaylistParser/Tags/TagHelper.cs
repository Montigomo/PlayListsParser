using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using TagLib;

namespace PlaylistParser
{
	static class TagHelper
	{

		public static Encoding GetTagEncoding(string _filePath)
		{

			return Encoding.Default;
		}

		public static void Test()
		{

			//var fileName = @"";

			var MainFolder = @"D:\temp\audio\Ru";

			if (String.IsNullOrWhiteSpace(MainFolder) || !Directory.Exists(MainFolder))
				return;

			foreach(var file in  Directory.GetFiles(MainFolder))
			{

				var tfile = TagLib.File.Create(file);
				string title = $@"{String.Join(", ", tfile.Tag.Performers)} - {tfile.Tag.Title}";

				var encUtf8 = Encoding.GetEncoding("UTF-8");
				var encUtf16 = Encoding.GetEncoding("UTF-16");
				var encWin1251 = Encoding.GetEncoding("windows-1251");

				var chars1 = title.ToCharArray();
				var bytes1 = encWin1251.GetBytes(title);
				var bytes2 = encUtf16.GetBytes(title);
				var bytes3 = Encoding.Convert(encUtf16, encWin1251, bytes2);
				var t = encUtf8.GetString(bytes3);

				var str1 = "Ау";
				var str1chars1 = str1.ToCharArray();

				var str1bytes1 = Encoding.Convert(encUtf16, encUtf8, encUtf16.GetBytes(str1));
				var str1bytes2 = Encoding.Convert(encUtf16, encWin1251, encUtf16.GetBytes(str1));
				//var str1r1 = 


				TimeSpan duration = tfile.Properties.Duration;
				Console.WriteLine("Title: {0}, duration: {1}", title, duration);							 
			}
		}

	}
}
