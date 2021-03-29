﻿using Objectiks.Engine.Query;
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

namespace Objectiks
{
    public class DocumentMeta : IDisposable
    {
        public string TypeOf { get; set; }
        public string ParseOf { get; set; }
        public string Primary { get; set; }
        public object Sequence { get; set; } = 0;
        public long TotalRecords { get; set; }
        public long DiskSize { get; set; }
        public long MemorySize { get; set; }
        public bool HasLazy { get; set; }
        public bool HasData { get; set; }
        public DocumentKeys Keys { get; set; }
        public DocumentSchemaKeys KeyOf { get; set; }
        public DocumentRefs Refs { get; set; }
        public DocumentCacheInfo Cache { get; set; }
        public DocumentPartitions Partitions { get; set; }
        public string Directory { get; set; }
        public bool Exists { get; set; }


        public DocumentMeta() { }

        public DocumentMeta(string typeOf, DocumentSchema schema, IDocumentConnection connection)
        {
            TypeOf = typeOf;
            ParseOf = schema.ParseOf;
            Primary = schema.Primary;
            KeyOf = schema.KeyOf;
            Refs = schema.Refs;
            Cache = schema.Cache;
            Keys = new DocumentKeys();
            Partitions = new DocumentPartitions();
            Partitions.Current = 0;
            Partitions.Next = 0;
            Directory = Path.Combine(connection.BaseDirectory, DocumentDefaults.Documents, typeOf);
            HasLazy = GetRefs(true).Count > 0;
            Exists = true;

            RefsIndexBuild();
        }

        public void UpdateSequence(object primary)
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

        public object GetNewSequenceId(Type type)
        {
            TypeCode typeCode = Type.GetTypeCode(type);

            if (typeCode == TypeCode.Int32)
            {
                int seq = 0;
                Int32.TryParse(Sequence.ToString(), out seq);
                Sequence = Interlocked.Increment(ref seq);
            }
            else if (typeCode == TypeCode.Int64)
            {
                long seq = 0;
                Int64.TryParse(Sequence.ToString(), out seq);
                Sequence = Interlocked.Increment(ref seq);
            }
            else if (typeCode == TypeCode.String || typeCode == TypeCode.Object)
            {
                Sequence = Guid.NewGuid();
            }
            else
            {
                throw new Exception($"Undefined sequence type {typeCode}");
            }

            return Sequence;
        }

        public void RefsIndexBuild()
        {
            if (Refs != null)
            {
                int index = 0;
                foreach (var item in Refs)
                {
                    item.Index = index;
                    index++;
                }
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
                result = Keys.AsQueryable().Where(query.AsWhere(),
                       query.AsWhereParameters()).ToList();

                if (query.Take > 0)
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

        public DocumentPartition GetAvailablePartition(int partition, int? partialStoreLimit, int partitionTemporyCount)
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
            Partitions.Add(newPartition, 0);
            Partitions.Next = newPartition + 1;
            Partitions.Current = newPartition;
            return newPartition;
        }

        public void AddKeys(DocumentKey documentKey)
        {
            Keys.AddNew(documentKey);
        }

        public void UpdateKeys(DocumentKey documentKey)
        {
            DocumentKey key = Keys.Where(k => k.CacheOf == documentKey.CacheOf).FirstOrDefault();

            if (!key.Equals(null))
            {
                key.KeyOf = documentKey.KeyOf;
            }
        }

        public void RemoveKeys(object primaryOf)
        {
            var primaryOfStr = primaryOf.ToString();
            DocumentKey key = Keys.Where(k => k.PrimaryOf == primaryOfStr).FirstOrDefault();

            if (!key.Equals(null) && !String.IsNullOrEmpty(key.PrimaryOf))
            {
                Keys.Remove(key);
                ObjectiksOf.Core.Cache.Remove(TypeOf, key.PrimaryOf);
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