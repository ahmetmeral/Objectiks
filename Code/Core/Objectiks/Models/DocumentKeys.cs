using Objectiks.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Models
{
    public class DocumentKeys : DynamicArray<DocumentKey>
    {
       
    }

    public struct DocumentKey
    {
        public string PrimaryOf { get; set; }
        public string CacheOf { get; set; }
        public string[] KeyOf { get; set; }
        public int Partition { get; set; }
        public bool IsDirty { get; set; }

        public DocumentKey(string primary, string cacheOf, List<string> keyOf, int partition, bool dirty = false)
        {
            PrimaryOf = primary;
            CacheOf = cacheOf;
            KeyOf = keyOf.ToArray();
            Partition = partition;
            IsDirty = dirty;
        }
    }
}
