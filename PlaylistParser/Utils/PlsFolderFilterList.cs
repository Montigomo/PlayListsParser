using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PlaylistParser
{

	#region PlsFolderFilterList

	public class PlsFolderFilterList : ICollection<string>, INotifyPropertyChanged, INotifyCollectionChanged
	{

		public event PropertyChangedEventHandler PropertyChanged;

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		private Dictionary<int, string> _dictionary = new Dictionary<int, string>();

		private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private List<int> UsedCounter
		{
			get { return _dictionary.Keys.ToList(); }
		}

		private object _lock = new object();

		[XmlAttribute("index")]
		public int Index
		{
			get;
			set;
		}

		#region ICollection implimentation

		public int Count => _dictionary.Count;

		public bool IsReadOnly => throw new NotImplementedException();

		public void Add(string item)
		{
			int index;
			Add(item, out index);
		}

		public void Add(string item, out int index)
		{
			index = -1;
			if (!_dictionary.ContainsValue(item))
			{
				lock (_lock)
				{
					index = _dictionary.Count == 0 ? 0 : _dictionary.Keys.Max() + 1;
					_dictionary.Add(index, item);
				}
			}
			CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
			return;
		}

		public void Clear()
		{
			_dictionary.Clear();
			CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		public bool Contains(string item)
		{
			return _dictionary.ContainsValue(item);
		}

		public void CopyTo(string[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<string> GetEnumerator()
		{
			return _dictionary.Values.GetEnumerator();
		}

		public bool Remove(string item)
		{
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _dictionary.Values.GetEnumerator();
		}

		#endregion

		public string this[int key]
		{
			get
			{
				int entry = _dictionary.Keys.Contains(key) ? _dictionary.Keys.FirstOrDefault(_key => _key == key) : -1;
				if (entry >= 0)
					return _dictionary[entry];
				//ThrowHelper.ThrowKeyNotFoundException();
				return default(string);
			}

			set
			{
				_dictionary[key] = value;
			}
		}

	}

	#endregion
}
