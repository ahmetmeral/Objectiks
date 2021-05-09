using Objectiks.Engine.Query;
using Objectiks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading;
using Objectiks.Extentions;
using System.IO;
using Objectiks.Services;
using Newtonsoft.Json;
using Objectiks.Engine;

namespace Objectiks
{
    public class DocumentMeta : IDisposable
    {
        public string TypeOf { get; set; }
        public string ParseOf { get; set; }
        public string WorkOf { get; set; }
        public string UserOf { get; set; }
        public string PrimaryOf { get; set; }
        public object Sequence { get; set; } = 0;
        public long TotalRecords { get; set; }
        public DocumentKeyIndex Keys { get; set; }
        public DocumentKeyOfNames KeyOfNames { get; set; }
        public DocumentCacheInfo Cache { get; set; }
        public DocumentPartitions Partitions { get; set; }
        public string Extention { get; set; }
        public string Directory { get; set; }
        public bool HasData { get; set; }

        [JsonIgnore]
        public bool Exists { get; set; }


        public DocumentMeta() { }

        public DocumentMeta(string typeOf, DocumentType documentType, DocumentProvider fileProvider, DocumentOption option)
        {
            TypeOf = typeOf;
            ParseOf = documentType.ParseOf;
            PrimaryOf = documentType.PrimaryOf;
            UserOf = documentType.UserOf;
            KeyOfNames = documentType.KeyOf;
            Cache = documentType.Cache;
            Keys = new DocumentKeyIndex();
            Partitions = new DocumentPartitions();
            Partitions.Current = 0;
            Partitions.Next = 0;
            Directory = Path.Combine(fileProvider.BaseDirectory, DocumentDefaults.Documents, typeOf);
            Extention = option.Extention;
            Exists = true;
        }

        public override string ToString()
        {
            return $"{TypeOf} - Count: {TotalRecords} - Keys: {Keys?.Count} - Exist : {Exists}";
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
