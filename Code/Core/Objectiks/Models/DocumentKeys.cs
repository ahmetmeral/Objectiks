using Objectiks.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Models
{
    public class DocumentKeyIndex : DynamicArray<DocumentKey>
    {

    }

    public struct DocumentKey
    {
        public string PrimaryOf { get; set; }
        public string AccountOf { get; set; }
        public string UserOf { get; set; }
        public string CacheOf { get; set; }
        public string[] KeyOf { get; set; }
        public int Partition { get; set; }
        public bool IsDirty { get; set; }

        public DocumentKey(string primaryOf, string accountOf, string userOf, string cacheOf,
            string[] keyOf, int partition, bool dirty = false)
        {
            PrimaryOf = primaryOf;
            AccountOf = accountOf;
            UserOf = userOf;
            CacheOf = cacheOf;
            KeyOf = keyOf;
            Partition = partition;
            IsDirty = dirty;
        }
    }
}
