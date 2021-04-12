using Objectiks.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks
{
    public class Document : IDisposable
    {
        public string TypeOf { get; set; }
        public string PrimaryOf { get; set; }
        public string AccountOf { get; set; }
        public string UserOf { get; set; }
        public string CacheOf { get; set; }
        public string[] KeyOf { get; set; }
        public dynamic Data { get; set; }
        public int Partition { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool HasArray { get; set; }
        public bool Exists { get; set; }

        public Document() { }


        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
