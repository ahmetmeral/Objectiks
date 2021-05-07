using Objectiks.Models;
using System;
using System.Collections.Generic;
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
    }
}
