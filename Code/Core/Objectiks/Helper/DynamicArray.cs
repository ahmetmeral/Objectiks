using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Helper
{
    public class DynamicArray<T> : IEnumerable<T> where T : struct
    {
        private List<T> _items = new List<T>();

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

        public void AddNew(T item)
        {
            _items.Add(item);
        }
        public void RemoveAt(int index)
        {
            _items.RemoveAt(index);
        }
        public void Remove(T item)
        {
            _items.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
    }
}
