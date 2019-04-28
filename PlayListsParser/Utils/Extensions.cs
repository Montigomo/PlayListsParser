using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using PlayListsParser.PlayLists;
using System.Windows;
using System.Windows.Media;

// ReSharper disable once CheckNamespace
namespace PlayListsParser
{
	static class Extensions
	{
		public static int PPP { get; set; }

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

	}



}
