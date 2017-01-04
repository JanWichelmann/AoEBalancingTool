using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace AoEBalancingTool
{
	public interface ICollectionView<out T> : ICollectionView, IEnumerable<T>
	{
		IEnumerable<T> SourceCollectionGeneric { get; }
	}

	/// <summary>
	/// Generic implementation of ICollectionView.
	/// </summary>
	/// <typeparam name="T">The collection element type.</typeparam>
	public class GenericCollectionView<T> : ICollectionView<T>
	{
		private readonly ICollectionView _collectionView;
		private readonly object _lockObj = new object();

		public GenericCollectionView(ICollectionView collectionView)
		{
			_collectionView = collectionView;
		}

		public bool CanFilter => _collectionView.CanFilter;
		public bool CanGroup => _collectionView.CanGroup;
		public bool CanSort => _collectionView.CanSort;

		public CultureInfo Culture
		{
			get { return _collectionView.Culture; }
			set { _collectionView.Culture = value; }
		}

		public object CurrentItem => _collectionView.CurrentItem;
		public int CurrentPosition => _collectionView.CurrentPosition;

		Predicate<object> ICollectionView.Filter
		{
			get { return _collectionView.Filter; }
			set { _collectionView.Filter = value; }
		}

		public Predicate<T> Filter
		{
			get { return (_collectionView.Filter == null) ? (Predicate<T>)null :(obj) => _collectionView.Filter(obj); }
			set { _collectionView.Filter = (value == null) ? (Predicate<object>)null : (obj) => value((T)obj); }
		}

		public ObservableCollection<GroupDescription> GroupDescriptions => _collectionView.GroupDescriptions;
		public ReadOnlyObservableCollection<object> Groups => _collectionView.Groups;
		public bool IsCurrentAfterLast => _collectionView.IsCurrentAfterLast;
		public bool IsCurrentBeforeFirst => _collectionView.IsCurrentBeforeFirst;
		public bool IsEmpty => _collectionView.IsEmpty;
		public SortDescriptionCollection SortDescriptions => _collectionView.SortDescriptions;
		public IEnumerable SourceCollection => _collectionView.SourceCollection;
		public IEnumerable<T> SourceCollectionGeneric => _collectionView.Cast<T>();

		public event NotifyCollectionChangedEventHandler CollectionChanged
		{
			add
			{
				lock(_lockObj)
				{
					_collectionView.CollectionChanged += value;
				}
			}
			remove
			{
				lock(_lockObj)
				{
					_collectionView.CollectionChanged -= value;
				}
			}
		}

		public event EventHandler CurrentChanged
		{
			add
			{
				lock(_lockObj)
				{
					_collectionView.CurrentChanged += value;
				}
			}
			remove
			{
				lock(_lockObj)
				{
					_collectionView.CurrentChanged -= value;
				}
			}
		}

		public event CurrentChangingEventHandler CurrentChanging
		{
			add
			{
				lock(_lockObj)
				{
					_collectionView.CurrentChanging += value;
				}
			}
			remove
			{
				lock(_lockObj)
				{
					_collectionView.CurrentChanging -= value;
				}
			}
		}

		public bool Contains(object item) => _collectionView.Contains(item);
		public IDisposable DeferRefresh() => _collectionView.DeferRefresh();
		public IEnumerator<T> GetEnumerator() => new EnumeratorGeneric(_collectionView.GetEnumerator());
		IEnumerator IEnumerable.GetEnumerator() => _collectionView.GetEnumerator();
		public bool MoveCurrentTo(object item) => _collectionView.MoveCurrentTo(item);
		public bool MoveCurrentToFirst() => _collectionView.MoveCurrentToFirst();
		public bool MoveCurrentToLast() => _collectionView.MoveCurrentToLast();
		public bool MoveCurrentToNext() => _collectionView.MoveCurrentToNext();
		public bool MoveCurrentToPosition(int position) => _collectionView.MoveCurrentToPosition(position);
		public bool MoveCurrentToPrevious() => _collectionView.MoveCurrentToPrevious();
		public void Refresh()
		{
			_collectionView.Refresh();
		}

		class EnumeratorGeneric : IEnumerator<T>
		{
			private readonly IEnumerator _enumerator;

			public EnumeratorGeneric(IEnumerator enumerator)
			{
				_enumerator = enumerator;
			}

			public T Current => (T)_enumerator.Current;
			object IEnumerator.Current => Current;
			public void Dispose() { }
			public bool MoveNext() => _enumerator.MoveNext();
			public void Reset()
			{
				_enumerator.Reset();
			}
		}
	}
}