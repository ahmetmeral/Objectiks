using Newtonsoft.Json;
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
        public string WorkOf { get; set; }
        public string UserOf { get; set; }
        public string CacheOf { get; set; }
        public string[] KeyOf { get; set; }
        public dynamic Data { get; set; }
        public int Partition { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool HasArray { get; set; }

        [JsonIgnore]
        public bool Exists { get; set; }

        public Document() { }

        public Document(DocumentInfo info)
        {
            if (info.Exists)
            {
                TypeOf = info.TypeOf;
                PrimaryOf = info.PrimaryOf.ToString();
                Partition = info.Partition;
            }

            Exists = info.Exists;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
