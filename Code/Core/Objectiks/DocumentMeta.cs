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
        public string Primary { get; set; }
        public string Workspace { get; set; }
        public string User { get; set; }
        public object Sequence { get; set; } = 0;
        public long TotalRecords { get; set; }
        public long DiskSize { get; set; }
        public long MemorySize { get; set; }
        public bool HasData { get; set; }
        public DocumentKeyIndex Keys { get; set; }
        public DocumentKeyOfNames KeyOfNames { get; set; }
        public DocumentCacheInfo Cache { get; set; }
        public DocumentPartitions Partitions { get; set; }
        public string Extention { get; set; }
        public string Directory { get; set; }

        [JsonIgnore]
        public bool Exists { get; set; }


        public DocumentMeta() { }

        public DocumentMeta(string typeOf, DocumentSchema schema, DocumentProvider fileProvider, DocumentOption option)
        {
            TypeOf = typeOf;
            ParseOf = schema.ParseOf;
            Primary = schema.PrimaryOf;
            User = schema.UserOf;
            KeyOfNames = schema.KeyOf;
            Cache = schema.Cache;
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
