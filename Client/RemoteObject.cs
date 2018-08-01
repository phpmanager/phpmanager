//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

using System;
using System.Collections;
using System.Collections.Generic;

namespace Web.Management.PHP
{

    public sealed class RemoteObjectCollection<T> : IRemoteObject, ICollection, IList<T> where T : IRemoteObject, new()
    {

        private List<T> _list;

        public RemoteObjectCollection(ArrayList sourceList)
        {
            if (sourceList != null)
            {
                Initialize(sourceList);
            }
            else
            {
                _list = new List<T>();
            }
        }

        public RemoteObjectCollection()
        {
            _list = new List<T>();
        }

        public int Count
        {
            get
            {
                return _list.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((IList)_list).IsReadOnly;
            }
        }

        public T this[int index]
        {
            get
            {
                return _list[index];
            }
            set
            {
                _list[index] = value;
            }
        }

        public void Add(T item)
        {
            _list.Add(item);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public object GetData()
        {
            var items = new ArrayList(_list.Count);
            foreach (T item in _list)
            {
                items.Add(item.GetData());
            }

            return items;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        private void Initialize(ArrayList sourceList)
        {
            _list = new List<T>(sourceList.Count);

            foreach (object o in sourceList)
            {
                var item = new T();
                item.SetData(o);
                _list.Add(item);
            }
        }

        public void Insert(int index, T item)
        {
            _list.Insert(index, item);
        }

        public bool Remove(T item)
        {
            return _list.Remove(item);
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void SetData(object o)
        {
            Initialize((ArrayList)o);
        }

        #region ICollection Members

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)_list).CopyTo(array, index);
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return ((ICollection)_list).IsSynchronized;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return ((ICollection)_list).SyncRoot;
            }
        }

        #endregion
    }

    public interface IRemoteObject
    {

        object GetData();

        void SetData(object o);

    }
}