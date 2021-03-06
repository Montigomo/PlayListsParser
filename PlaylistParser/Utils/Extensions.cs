﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using PlaylistParser.Playlist;
using System.Windows;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Reflection;
using System.Windows.Controls;
using System.ComponentModel;


// ReSharper disable once CheckNamespace
namespace PlaylistParser
{
	static class Extensions
	{

		#region File && Folders path


		#region Remove files Attributes

		public static void RemoveReadOnlyAttribute(this string path)
		{
			path.RemoveAttribute(FileAttributes.ReadOnly);
		}

		public static void RemoveAttribute(this string path, FileAttributes attribute)
		{
			if (!File.Exists(path))
				return;

			FileAttributes attributes = File.GetAttributes(path);

			if ((attributes & attribute) == attribute)
			{
				attributes = RemoveAttribute(attributes, attribute);
				File.SetAttributes(path, attributes);
			}
		}
		private static FileAttributes RemoveAttribute(FileAttributes attributes, FileAttributes attributesToRemove)
		{
			return attributes & ~attributesToRemove;
		}

		#endregion

		public static void SetAttributesNormal(this DirectoryInfo dir)
		{
			foreach (var subDir in dir.GetDirectories())
				SetAttributesNormal(subDir);
			foreach (var file in dir.GetFiles())
			{
				file.Attributes = FileAttributes.Normal;
			}
		}

		public static string SanitizePath(this string path)
		{
			string regexSearch = Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()) + new string(System.IO.Path.GetInvalidPathChars()));

			string invalidRegStr = $@"([{regexSearch}]*\.+$)|([{regexSearch}]+)";

			Regex r = new Regex($"[{regexSearch}]");

			var reservedWords = new[]
			{
				"CON", "PRN", "AUX", "CLOCK$", "NUL", "COM0", "COM1", "COM2", "COM3", "COM4",
				"COM5", "COM6", "COM7", "COM8", "COM9", "LPT0", "LPT1", "LPT2", "LPT3", "LPT4",
				"LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
			};

			var result = r.Replace(path, "");

			//foreach (var reservedWord in reservedWords)
			//{
			//	var reservedWordPattern = $"^{reservedWord}\\.";
			//	result = Regex.Replace(result, reservedWordPattern, "", RegexOptions.IgnoreCase);
			//}

			return result;
		}

		#endregion


		#region Relative && Absolute path

		#region Absolute path

		//public static String GetAbsolutePath(String path)
		//{
		//	return GetAbsolutePath(null, path);
		//}

		//public static string GetAbsolutePathSimple(this string basePath, string path)
		//{
		//	basePath = Path.GetDirectoryName(basePath);

		//	string finalPath;

		//	if (!Path.IsPathRooted(path))
		//		finalPath = basePath + "\\" + path;
		//	else
		//		finalPath = path;

		//	return Path.GetFullPath(finalPath);
		//}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="basePath">from D:\temp</param>
		/// <param name="toPath">to ..\..\tools\z.tzt</param>
		/// <returns></returns>
		public static String GetAbsolutePath(this String basePath, String toPath)
		{
			if (toPath == null)
				return null;

			basePath = Path.GetDirectoryName(basePath);

			if (basePath == null)
				basePath = Path.GetFullPath("."); // quick way of getting current working directory
			else
				basePath = GetAbsolutePath(null, basePath); // to be REALLY sure ;)

			String finalPath;

			// specific for windows paths starting on \ - they need the drive added to them.
			// I constructed this piece like this for possible Mono support.
			if (!Path.IsPathRooted(toPath) || "\\".Equals(Path.GetPathRoot(toPath)))
			{
				if (toPath.StartsWith(Path.DirectorySeparatorChar.ToString()))
					finalPath = Path.Combine(Path.GetPathRoot(basePath), toPath.TrimStart(Path.DirectorySeparatorChar));
				else
					finalPath = Path.Combine(basePath, toPath);
			}
			else
				finalPath = toPath;

			// resolves any internal "..\" to get the true full path.
			return Path.GetFullPath(finalPath);
		}

		#endregion

		#region Relative path

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fromPath">D:\temp\1</param>
		/// <param name="toPath">D:\temp\2\z.txt</param>
		/// <returns></returns>
		public static string GetRelativePath(this string fromPath, string toPath)
		{
			
			if (!fromPath.IsPathAbsolute())
				return null;

			if (!toPath.IsPathAbsolute())
				return null;

			if (!File.Exists(toPath))
				return null;

			if (!Path.GetPathRoot(fromPath).Equals(Path.GetPathRoot(toPath), StringComparison.OrdinalIgnoreCase))
				return null;

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

		#endregion

		public static bool IsPathAbsolute(this string path)
		{
			return Path.IsPathRooted(path) && !Path.GetPathRoot(path).Equals(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal);
		}

		public static bool IsDirectory(this string path)
		{
			// File.GetAttributes(path).HasFlag(FileAttributes.Directory);
			return path.GetPathAttribute() == FILE_ATTRIBUTE_DIRECTORY;
		}

		#endregion


		#region Playlist

		/// <summary>
		/// Try to get [name] group by using PlsFilter
		/// </summary>
		/// <param name="path">Destination folder</param>
		/// <returns></returns>
		public static string TryGetFolder(this IPlaylist playlist, string toPath, bool oneFolder)
		{
			var result = playlist.Title;

			if (!Directory.Exists(toPath))
				throw new Exception($@"Folder {toPath} don't exist.");

			try
			{

				if (!oneFolder)
				{
					var match = Regex.Match(playlist.PlaylistPath, AppSettings.Instance.PlsFilter, RegexOptions.Compiled | RegexOptions.IgnoreCase);

					if (match.Success)
						result = match.Groups["name"].Value;
					else
						result = playlist.Title;
				}
				else
				{
					result = String.Empty;
				}
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
			}

			result = Path.Combine(toPath, result);

			return result;
		}

		public static string TryGetFolder(this IPlaylist playlist, string toPath)
		{
			return TryGetFolder(playlist, toPath, AppSettings.Instance.UseOneFolder);
		}

		#endregion


		#region Others

		public static string WriteError(this string outMessage)
		{
			return $@"ERROR: {outMessage}";
		}

		public static bool IsValidRegex(this string pattern)
		{
			if (string.IsNullOrEmpty(pattern)) return false;

			try
			{
				Regex.Match("", pattern);
			}
			catch (ArgumentException)
			{
				return false;
			}

			return true;
		}


		public static bool ToBoolean(this bool? value)
		{
			return value ?? false;
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

		public static object GetDependencyPropertyValue(this Control control, string name, object value)
		{
			var descriptor = DependencyPropertyDescriptor.FromName(name, control.GetType(), control.GetType());

			if (descriptor != null)
			{
				// now you can set property value with
				return descriptor.GetValue(control);

				// also, you can use the dependency property itself
				//var property = descriptor.DependencyProperty;
				//dependencyObject.SetValue(property, value);
			}
			return null;
		}

		public static void SetDependencyPropertyValue(this Control control, string name, object value)
		{
			var descriptor = DependencyPropertyDescriptor.FromName(name, control.GetType(), control.GetType());

			if (descriptor != null)
			{
				// now you can set property value with
				descriptor.SetValue(control, value);

				// also, you can use the dependency property itself
				//var property = descriptor.DependencyProperty;
				//dependencyObject.SetValue(property, value);
			}
		}


		public static DependencyProperty GetDependencyPropertyByName(this Control control, string name)
		{
			var descriptor = DependencyPropertyDescriptor.FromName(name, control.GetType(), control.GetType());

			return (descriptor != null) ? descriptor.DependencyProperty : null;
		}

		//public static DependencyProperty GetDependencyPropertyByName(DependencyObject dependencyObject, string dpName)
		//{
		//	return GetDependencyPropertyByName(dependencyObject.GetType(), dpName);
		//}

		//public static DependencyProperty GetDependencyPropertyByName(Type dependencyObjectType, string dpName)
		//{
		//	DependencyProperty dp = null;

		//	var fieldInfo = dependencyObjectType.GetField(dpName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
		//	if (fieldInfo != null)
		//	{
		//		dp = fieldInfo.GetValue(null) as DependencyProperty;
		//	}

		//	return dp;
		//}

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
