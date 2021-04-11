using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Helper
{
    public class DynamicArray<T> : ICollection<T> where T : struct
    {
        private List<T> _items = new List<T>();

        public DynamicArray() { }

        public DynamicArray(params T[] items)
        {
            _items.AddRange(items);
        }

        public T this[int index]
        {
            get { return _items[index]; }
            set { _items[index] = value; }
        }

        public int Count { get { return _items.Count; } }

        public bool IsReadOnly => false;

        public void Add(T item)
        {
            _items.Add(item);
        }

        public void Clear()
        {
            _items.Clear();
        }

        public bool Contains(T item)
        {
            return _items.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public bool Remove(T item)
        {
            return _items.Remove(item);
        }

        public void RemoveAt(int index)
        {
            _items.RemoveAt(index);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
    }
}
