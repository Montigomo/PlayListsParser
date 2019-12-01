﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using PlayListsParser.PlayLists;
using System.Windows;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Text;

// ReSharper disable once CheckNamespace
namespace PlayListsParser
{
	static class Extensions
	{

		#region GetRelativePath

		// Path.GetFullPath((new Uri(absolute_path)).LocalPath);

		public static String GetAbsolutePath(String path)
		{
			return GetAbsolutePath(null, path);
		}

		public static string GetAbsolutePathSimple(string basePath, string path)
		{
			basePath = Path.GetDirectoryName(basePath);

			string finalPath;


			if (!Path.IsPathRooted(path))
				finalPath = basePath + "\\" + path;
			else
				finalPath = path;

			return Path.GetFullPath(finalPath);
		}

		public static String GetAbsolutePath(String basePath, String path)
		{
			if (path == null)
				return null;

			basePath = Path.GetDirectoryName(basePath);

			if (basePath == null)
				basePath = Path.GetFullPath("."); // quick way of getting current working directory
			else
				basePath = GetAbsolutePath(null, basePath); // to be REALLY sure ;)

			String finalPath;

			// specific for windows paths starting on \ - they need the drive added to them.
			// I constructed this piece like this for possible Mono support.
			if (!Path.IsPathRooted(path) || "\\".Equals(Path.GetPathRoot(path)))
			{
				if (path.StartsWith(Path.DirectorySeparatorChar.ToString()))
					finalPath = Path.Combine(Path.GetPathRoot(basePath), path.TrimStart(Path.DirectorySeparatorChar));
				else
					finalPath = Path.Combine(basePath, path);
			}
			else
				finalPath = path;

			// resolves any internal "..\" to get the true full path.
			return Path.GetFullPath(finalPath);
		}


		public static string GetRelativePath(string fromPath, string toPath)
		{
			int fromAttr = GetPathAttribute(fromPath);
			int toAttr = GetPathAttribute(toPath);

			StringBuilder path = new StringBuilder(260); // MAX_PATH
			if (PathRelativePathTo(
					path,
					fromPath,
					fromAttr,
					toPath,
					toAttr) == 0)
			{
				throw new ArgumentException("Paths must have a common prefix");
			}
			return path.ToString();
		}

		private static int GetPathAttribute(this string path)
		{
			DirectoryInfo di = new DirectoryInfo(path);
			if (di.Exists)
			{
				return FILE_ATTRIBUTE_DIRECTORY;
			}

			FileInfo fi = new FileInfo(path);
			if (fi.Exists)
			{
				return FILE_ATTRIBUTE_NORMAL;
			}

			throw new FileNotFoundException();
		}

		private const int FILE_ATTRIBUTE_DIRECTORY = 0x10;
		private const int FILE_ATTRIBUTE_NORMAL = 0x80;

		[DllImport("shlwapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern int PathRelativePathTo(StringBuilder pszPath,
				string pszFrom, int dwAttrFrom, string pszTo, int dwAttrTo);


		public static bool IsDirectory(this string path)
		{
			// File.GetAttributes(path).HasFlag(FileAttributes.Directory);
			return path.GetPathAttribute() == FILE_ATTRIBUTE_DIRECTORY;
		}

		#endregion


		#region Others

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

		#endregion


		#region WPF

		public static IEnumerable<T> FindChildren<T>(this DependencyObject source) where T : DependencyObject
		{
			if (source != null)
			{
				var childs = GetChildObjects(source);
				foreach (DependencyObject child in childs)
				{
					//analyze if children match the requested type
					if (child != null && child is T)
					{
						yield return (T)child;
					}

					//recurse tree
					foreach (T descendant in FindChildren<T>(child))
					{
						yield return descendant;
					}
				}
			}
		}

		public static IEnumerable<DependencyObject> GetChildObjects(this DependencyObject parent)
		{
			if (parent == null) yield break;


			if (parent is ContentElement || parent is FrameworkElement)
			{
				//use the logical tree for content / framework elements
				foreach (object obj in LogicalTreeHelper.GetChildren(parent))
				{
					var depObj = obj as DependencyObject;
					if (depObj != null) yield return (DependencyObject)obj;
				}
			}
			else
			{
				//use the visual tree per default
				int count = VisualTreeHelper.GetChildrenCount(parent);
				for (int i = 0; i < count; i++)
				{
					yield return VisualTreeHelper.GetChild(parent, i);
				}
			}
		}

		public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject depObj) where T : DependencyObject
		{
			if (depObj != null)
			{
				for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
				{
					DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
					if (child != null && child is T)
					{
						yield return (T)child;
					}

					foreach (T childOfChild in FindVisualChildren<T>(child))
					{
						yield return childOfChild;
					}
				}
			}
		}
		#endregion


	}
}
