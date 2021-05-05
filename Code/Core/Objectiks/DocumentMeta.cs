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
        public bool HasRefs { get; set; }
        public bool HasData { get; set; }
        public DocumentKeyIndex Keys { get; set; }
        public DocumentKeyOfNames KeyOfNames { get; set; }
        public DocumentRefs Refs { get; set; }
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
            Refs = schema.Refs;
            Cache = schema.Cache;
            Keys = new DocumentKeyIndex();
            Partitions = new DocumentPartitions();
            Partitions.Current = 0;
            Partitions.Next = 0;
            Directory = Path.Combine(fileProvider.BaseDirectory, DocumentDefaults.Documents, typeOf);
            Extention = option.Extention;
            HasRefs = GetRefs(true).Count > 0;
            Exists = true;

            RefsIndexBuild();
        }

        internal void UpdateSequence(object primary)
        {
            if (Int32.TryParse(primary.ToString(), out var out_primary_int32))
            {
                Int32.TryParse(Sequence.ToString(), out var seq);

                if (out_primary_int32 > seq)
                {
                    Sequence = out_primary_int32;
                }
            }
            else if (Int64.TryParse(primary.ToString(), out var out_primary_int64))
            {
                Int64.TryParse(Sequence.ToString(), out var seq);

                if (out_primary_int64 > seq)
                {
                    Sequence = out_primary_int64;
                }
            }
            else
            {
                if (Guid.TryParse(primary.ToString(), out var primary_out))
                {
                    Sequence = primary_out;
                }
            }
        }

        internal void RefsIndexBuild()
        {
            if (Refs != null)
            {
                int index = 0;
                foreach (var item in Refs)
                {
                    item.Lazy = true;
                    item.Index = index;
                    index++;
                }

                HasRefs = GetRefs(true).Count > 0;
            }
        }

        public DocumentRefs GetRefs(bool lazy = true)
        {
            var refs = new DocumentRefs();

            if (Refs != null)
            {
                refs.AddRange(Refs.Where(r => r.Lazy == lazy && r.Disabled == false).ToList());
            }

            return refs;
        }

        public T GetCountFromQueryOf<T>(QueryOf query)
        {
            return Keys.AsQueryable().Count(query.AsWhere(),
                     query.AsWhereParameters()).ChangeType<T>();
        }

        public DocumentKey GetDocumentKeyFromCacheOf(string cacheOf)
        {
            return Keys.Where(k => k.CacheOf == cacheOf).FirstOrDefault();
        }

        public List<DocumentKey> GetDocumentKeysFromQueryOf(QueryOf query)
        {
            List<DocumentKey> result = null;

            if (query.HasPrimaryOf || query.HasKeyOf)
            {
                result = Keys.AsQueryable().Where(query.AsWhere(), query.AsWhereParameters())?.ToList();

                if (result != null && query.Take > 0)
                {
                    result = result.Skip(query.Skip).Take(query.Take).ToList();
                }
            }
            else
            {
                if (query.Take > 0)
                {
                    result = Keys.Skip(query.Skip).Take(query.Take).ToList();
                }
                else
                {
                    result = Keys.ToList();
                }
            }

            return result;
        }

        internal DocumentPartition GetAvailablePartition(int partition, int? partialStoreLimit, int partitionTemporyCount)
        {
            if (partialStoreLimit.HasValue)
            {
                var totalCount = Partitions[partition] + partitionTemporyCount;

                if (totalCount < partialStoreLimit.Value)
                {
                    return new DocumentPartition(partition, OperationType.Append);
                }

                return new DocumentPartition(CreateNextPartition(), OperationType.Create);
            }
            return new DocumentPartition(partition, OperationType.Append);
        }

        private int CreateNextPartition()
        {
            var newPartition = Partitions.Next;

            while (Partitions.ContainsKey(newPartition))
            {
                newPartition = newPartition + 1;
            }

            Partitions.Add(newPartition, 0);
            Partitions.Next = newPartition + 1;
            Partitions.Current = newPartition;

            return newPartition;
        }

        internal void AddKeys(DocumentKey documentKey)
        {
            Keys.Add(documentKey);
        }

        internal string SubmitChanges(Document document, OperationType operation)
        {
            if (!Partitions.ContainsKey(document.Partition))
            {
                Partitions.Add(document.Partition, 0);
            }

            if (operation == OperationType.Read || operation == OperationType.Create || operation == OperationType.Append)
            {
                AddKeys(
                    new DocumentKey(
                    document.PrimaryOf,
                    document.WorkOf,
                    document.UserOf,
                    document.CacheOf,
                    document.KeyOf,
                    document.Partition
                    ));

                TotalRecords++;
                Partitions[document.Partition]++;

                UpdateSequence(document.PrimaryOf);
            }
            else if (operation == OperationType.Merge)
            {
                UpdateKeys(
                    new DocumentKey(
                    document.PrimaryOf,
                    document.WorkOf,
                    document.UserOf,
                    document.CacheOf,
                    document.KeyOf,
                    document.Partition
                    ));
            }
            else if (operation == OperationType.Delete)
            {
                TotalRecords--;
                Partitions[document.Partition]--;
                RemoveKeys(document.PrimaryOf);
            }

            return document.PrimaryOf;
        }

        internal void UpdateKeys(DocumentKey documentKey)
        {
            DocumentKey key = Keys.Where(k => k.CacheOf == documentKey.CacheOf).FirstOrDefault();

            if (!key.Equals(null))
            {
                key.KeyOf = documentKey.KeyOf;
            }
        }

        internal void RemoveKeys(object primaryOf)
        {
            var primaryOfStr = primaryOf.ToString();
            DocumentKey key = Keys.Where(k => k.PrimaryOf == primaryOfStr).FirstOrDefault();

            if (!key.Equals(null) && !String.IsNullOrEmpty(key.PrimaryOf))
            {
                Keys.Remove(key);
            }
        }

        internal void ClearPartitions()
        {
            Partitions.Clear();
            Partitions.Current = 0;
            Partitions.Next = 1;
            Partitions.Add(0, 0);
        }

        internal void ClearStaticFiles()
        {
            if (String.IsNullOrWhiteSpace(Directory))
            {
                return;
            }

            var files = System.IO.Directory.GetFiles(Directory, Extention, SearchOption.TopDirectoryOnly);

            foreach (var item in files)
            {
                try
                {
                    //default file not remove
                    var typeOfNameCheck = Path.GetFileNameWithoutExtension(item);

                    if (typeOfNameCheck.ToLowerInvariant() == TypeOf.ToLowerInvariant())
                    {
                        try
                        {
                            new FileInfo(item).Delete();
                        }
                        catch { }

                        File.WriteAllText(item, "[]", Encoding.UTF8);

                        continue;
                    }

                    new FileInfo(item).Delete();
                }
                catch { }
            }
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
