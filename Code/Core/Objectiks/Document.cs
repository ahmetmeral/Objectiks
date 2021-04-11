using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks
{
    public class Document : IDisposable
    {
        public string TypeOf { get; set; }
        public string CacheOf { get; set; }
        public int Partition { get; set; }
        public object Primary { get; set; }
        public dynamic Data { get; set; }
        public bool HasArray { get; set; }
        public bool HasLazy { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Exists { get; set; }

        public Document() { }


        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
