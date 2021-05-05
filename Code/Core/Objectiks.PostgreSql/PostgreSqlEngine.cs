using Newtonsoft.Json.Linq;
using Npgsql;
using Npgsql.Json;
using Objectiks.Extentions;
using Objectiks.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace Objectiks.PostgreSql
{
    public class PostgreSqlEngine : DocumentEngine
    {
        public PostgreSqlEngine(DocumentProvider documentProvider, DocumentOption options)
            : base(documentProvider, options)
        {

        }

        public override bool LoadDocumentType(string typeOf, bool isInitialize = false)
        {
            var schema = GetDocumentSchema(typeOf);
            var meta = new DocumentMeta(typeOf, schema, Provider, Option);

            if (isInitialize && meta.Cache.Lazy)
            {
                return false;
            }

            meta.Partitions.Add(0, 0);

            var refs = meta.GetRefs(false);

            using (var reader = new PostgreSqlReader(typeOf, Provider, Option, Logger))
            {
                while (reader.Read())
                {
                    foreach (var row in reader.Rows)
                    {
                        var document = new Document
                        {
                            TypeOf = typeOf,
                            Data = row,
                            Partition = 0,
                            CreatedAt = DateTime.UtcNow
                        };

                        UpdateDocumentMeta(ref meta, ref document, 0, OperationType.Read);

                        if (Option.SupportDocumentParser)
                        {
                            ParseDocumentData(ref meta, ref document, new DocumentStorage());
                        }

                        Cache.Set(document, meta.Cache.Expire);

                        document.Dispose();
                    }
                }

                Cache.Set(meta, meta.Cache.Expire);
            }

            return true;
        }

        public override void BulkAppend(DocumentContext context, DocumentTransaction transaction)
        {

        }

        public override void BulkCreate(DocumentContext context, DocumentTransaction transaction)
        {
            //using (var writer = new PostgreSqlWriter(meta.TypeOf, Provider, Option, Logger))
            //{
            //    writer.BulkCreate(docs);
            //}
        }

        public override void BulkMerge(DocumentContext context, DocumentTransaction transaction)
        {

        }

        public override void BulkDelete(DocumentContext context, DocumentTransaction transaction)
        {

        }
    }
}
