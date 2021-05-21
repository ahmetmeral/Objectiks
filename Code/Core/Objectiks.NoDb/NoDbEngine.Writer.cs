using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Objectiks.Engine;
using Objectiks.Engine.Query;
using Objectiks.Extentions;
using Objectiks.Models;
using Objectiks.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Linq.Dynamic.Core;
using Objectiks.Helper;

namespace Objectiks.NoDb
{
    public partial class NoDbEngine : DocumentEngine
    {
        public override void BulkCreate(DocumentContext context, DocumentTransaction transaction)
        {
            var formatting = context.Formatting == Format.Indented ? Formatting.Indented : Formatting.None;
            var json = new JSONSerializer(Logger);
            json.CreateRows(context.Storage, context.Documents.Select(d => d.Data).ToList(), formatting);
        }

        public override void BulkAppend(DocumentContext context, DocumentTransaction transaction)
        {
            var formatting = context.Formatting == Format.Indented ? Formatting.Indented : Formatting.None;
            var json = new JSONSerializer(Logger);
            json.AppendRows(context.Storage, context.Documents.Select(d => d.Data).ToList(), formatting);
        }

        public override void BulkMerge(DocumentContext context, DocumentTransaction transaction)
        {
            var formatting = context.Formatting == Format.Indented ? Formatting.Indented : Formatting.None;
            var json = new JSONSerializer(Logger);
            var map = new DocumentMap(context.Primary, context.Primary);
            json.MergeRows(context.Storage, context.Documents.Select(d => d.Data).ToList(), map, formatting);
        }

        public override void BulkDelete(DocumentContext context, DocumentTransaction transaction)
        {
            var formatting = context.Formatting == Format.Indented ? Formatting.Indented : Formatting.None;
            var json = new JSONSerializer(Logger);
            var map = new DocumentMap(context.Primary, context.Primary);
            json.DeleteRows(context.Storage, context.Documents.Select(d => d.Data).ToList(), map, formatting);
        }

        public override void TruncateTypeOf(string typeOf)
        {
            var meta = GetTypeMeta(typeOf);

            TruncateTypeOf(meta);
        }

        public override void TruncateTypeOf(DocumentMeta meta)
        {
            if (meta.Keys.Count > 0)
            {
                var typeOf = meta.TypeOf;

                foreach (var item in meta.Keys)
                {
                    Cache.Remove(Cache.CacheOfDoc(typeOf, item.PrimaryOf));
                    Cache.Remove(Cache.CacheOfDocInfo(typeOf, item.PrimaryOf));
                }
            }

            meta.Keys = new DocumentKeyIndex();
            meta.TotalRecords = 0;
            meta.HasData = false;
            meta.Sequence = 0;
            meta.ClearPartitions();
            meta.ClearStaticFiles();

            Cache.Set(meta, meta.Cache.Expire);
            Cache.Set(new DocumentSequence(meta.TypeOf, 0));
        }

        public override int Delete<T>(DocumentQuery query)
        {
            var list = ReadList<T>(query);

            int numberOfRows = list.Count;

            if (numberOfRows == 0)
            {
                return 0;
            }

            using (var writer = new DocumentWriter<T>(this, query.TypeOf))
            {
                writer.DeleteDocuments(list);
                writer.SubmitChanges();
            }

            return numberOfRows;
        }

        public override void OnChangeDocuments(DocumentMeta meta, DocumentContext context)
        {
            int count = context.Documents.Count;


            if (context.Operation == OperationType.Delete)
            {
                for (int i = 0; i < count; i++)
                {
                    var primaryOf = meta.SubmitChanges(context.Documents[i], OperationType.Delete);

                    Cache.Remove(meta.TypeOf, primaryOf);
                }
            }
            else
            {
                var parser = GetDocumentParser(meta.TypeOf, context.Operation);

                for (int i = 0; i < count; i++)
                {
                    Document document = context.Documents[i];

                    if (document.KeyOf == null)
                    {
                        document.KeyOf = new string[] { };
                    }

                    if (Option.SupportDocumentParser && parser != null)
                    {
                        parser.Parse(this, meta, document, context.Storage);
                    }

                    meta.SubmitChanges(document, context.Operation);

                    Cache.Set(document, meta.Cache.Expire);
                }
            }
        }
    }
}
