using Objectiks.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Objectiks.Extentions
{
    public static class DocumentMetaExtentions
    {
        public static string SubmitChanges(this DocumentMeta meta, Document document, OperationType operation)
        {
            if (!meta.Partitions.ContainsKey(document.Partition))
            {
                meta.Partitions.Add(document.Partition, 0);
            }

            if (operation == OperationType.Load || operation == OperationType.Create || operation == OperationType.Append)
            {
                meta.AddKeys(
                    new DocumentKey(
                    document.PrimaryOf,
                    document.WorkOf,
                    document.UserOf,
                    document.CacheOf,
                    document.KeyOf,
                    document.Partition
                    ));

                meta.TotalRecords++;
                meta.Partitions[document.Partition]++;

                meta.UpdateSequence(document.PrimaryOf);
            }
            else if (operation == OperationType.Merge)
            {
                meta.UpdateKeys(
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
                meta.TotalRecords--;
                meta.Partitions[document.Partition]--;
                meta.RemoveKeys(document.PrimaryOf);
            }

            return document.PrimaryOf;
        }

        public static void UpdateSequence(this DocumentMeta meta, object primary)
        {
            if (Int32.TryParse(primary.ToString(), out var out_primary_int32))
            {
                Int32.TryParse(meta.Sequence.ToString(), out var seq);

                if (out_primary_int32 > seq)
                {
                    meta.Sequence = out_primary_int32;
                }
            }
            else if (Int64.TryParse(primary.ToString(), out var out_primary_int64))
            {
                Int64.TryParse(meta.Sequence.ToString(), out var seq);

                if (out_primary_int64 > seq)
                {
                    meta.Sequence = out_primary_int64;
                }
            }
            else
            {
                if (Guid.TryParse(primary.ToString(), out var primary_out))
                {
                    meta.Sequence = primary_out;
                }
            }
        }

        public static DocumentPartition GetPartition(this DocumentMeta meta, int partition, int? partialStoreLimit, int partitionTemporyCount)
        {
            if (partialStoreLimit.HasValue)
            {
                var totalCount = meta.Partitions[partition] + partitionTemporyCount;

                if (totalCount < partialStoreLimit.Value)
                {
                    return new DocumentPartition(partition, OperationType.Append);
                }

                return new DocumentPartition(meta.CreateNextPartition(), OperationType.Create);
            }
            return new DocumentPartition(partition, OperationType.Append);
        }

        public static int CreateNextPartition(this DocumentMeta meta)
        {
            var newPartition = meta.Partitions.Next;

            while (meta.Partitions.ContainsKey(newPartition))
            {
                newPartition = newPartition + 1;
            }

            meta.Partitions.Add(newPartition, 0);
            meta.Partitions.Next = newPartition + 1;
            meta.Partitions.Current = newPartition;

            return newPartition;
        }

        public static void AddKeys(this DocumentMeta meta, DocumentKey documentKey)
        {
            meta.Keys.Add(documentKey);
        }

        public static void UpdateKeys(this DocumentMeta meta, DocumentKey documentKey)
        {
            DocumentKey key = meta.Keys.Where(k => k.CacheOf == documentKey.CacheOf).FirstOrDefault();

            if (!key.Equals(null))
            {
                key.KeyOf = documentKey.KeyOf;
            }
        }

        public static void RemoveKeys(this DocumentMeta meta, object primaryOf)
        {
            var primaryOfStr = primaryOf.ToString();
            DocumentKey key = meta.Keys.Where(k => k.PrimaryOf == primaryOfStr).FirstOrDefault();

            if (!key.Equals(null) && !String.IsNullOrEmpty(key.PrimaryOf))
            {
                meta.Keys.Remove(key);
            }
        }

        public static void ClearPartitions(this DocumentMeta meta)
        {
            meta.Partitions.Clear();
            meta.Partitions.Current = 0;
            meta.Partitions.Next = 1;
            meta.Partitions.Add(0, 0);
        }

        public static void ClearStaticFiles(this DocumentMeta meta)
        {
            if (String.IsNullOrWhiteSpace(meta.Directory))
            {
                return;
            }

            var files = System.IO.Directory.GetFiles(meta.Directory, meta.Extention, SearchOption.TopDirectoryOnly);

            foreach (var item in files)
            {
                try
                {
                    //default file not remove
                    var typeOfNameCheck = Path.GetFileNameWithoutExtension(item);

                    if (typeOfNameCheck.ToLowerInvariant() == meta.TypeOf.ToLowerInvariant())
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
    }
}
