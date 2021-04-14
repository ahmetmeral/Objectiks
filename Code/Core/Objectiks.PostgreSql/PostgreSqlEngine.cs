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
        public PostgreSqlEngine(DocumentProvider documentProvider, DocumentOption options) : base(documentProvider, options)
        {

        }

        public override bool LoadDocumentType(string typeOf)
        {
            var schema = GetDocumentSchema(typeOf);
            var meta = new DocumentMeta(typeOf, schema, Provider, Option);
           
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
                            ParseDocumentData(ref meta, ref document, new DocumentInfo());
                        }

                        if (Option.SupportLoaderInRefs)
                        {
                            ParseDocumentRefs(refs, ref document);
                        }

                        Cache.Set(document, meta.Cache.Expire);

                        document.Dispose();
                    }
                }

                Cache.Set(meta, meta.Cache.Expire);
            }

            return true;
        }

        public override void BulkAppend(DocumentMeta meta, DocumentInfo info, List<Document> docs, Format format = Format.None)
        {
            base.BulkAppend(meta, info, docs, format);
        }

        public override void BulkCreate(DocumentMeta meta, DocumentInfo info, List<Document> docs, Format format = Format.None)
        {
            base.BulkCreate(meta, info, docs, format);
        }

        public override void BulkMerge(DocumentMeta meta, DocumentInfo info, List<Document> docs, Format format = Format.None)
        {
            base.BulkMerge(meta, info, docs, format);
        }

        public override void BulkDelete(DocumentMeta meta, DocumentInfo info, List<Document> docs, Format format = Format.None)
        {
            base.BulkDelete(meta, info, docs, format);
        }
    }
}
