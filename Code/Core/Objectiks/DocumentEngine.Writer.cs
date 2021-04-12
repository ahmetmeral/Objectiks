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

namespace Objectiks
{
    public partial class DocumentEngine : IDocumentEngine
    {
        public virtual void SubmitChanges(DocumentMeta meta, DocumentInfo info, List<Document> docs, OperationType operation, Format format)
        {
            int count = docs.Count;

            if (count > 0)
            {
                Logger?.Debug(DebugType.Engine, $"Document write : number of documents : {count}");

                try
                {
                    if (operation == OperationType.Append)
                    {
                        BulkAppend(meta, info, docs, format);
                    }
                    else if (operation == OperationType.Merge)
                    {
                        BulkMerge(meta, info, docs, format);
                    }
                    else if (operation == OperationType.Create)
                    {
                        BulkCreate(meta, info, docs, format);
                    }
                    else if (operation == OperationType.Delete)
                    {
                        BulkDelete(meta, info, docs, format);
                    }
                }
                catch (Exception ex)
                {
                    Logger?.Fatal(ex);

                    throw ex;
                }
            }
        }

        protected virtual void OnChangeDocuments(DocumentMeta meta, DocumentInfo info, List<Document> docs, OperationType operation)
        {
            int count = docs.Count;

            if (operation == OperationType.Delete)
            {
                for (int i = 0; i < count; i++)
                {
                    var primaryOf = meta.SubmitChanges(docs[i], OperationType.Delete);

                    Cache.Remove(meta.TypeOf, primaryOf);
                }
            }
            else
            {
                var refs = meta.GetRefs(false);

                for (int i = 0; i < count; i++)
                {
                    Document document = docs[i];

                    if (Option.SupportDocumentParser)
                    {
                        ParseDocumentData(ref meta, ref document, info);
                    }

                    meta.SubmitChanges(document, operation);

                    if (!Option.SupportLoaderInRefs)
                    {
                        Cache.Set(document, meta.Cache.Expire);
                    }
                    else
                    {
                        ParseDocumentRefs(refs, ref document);
                        Cache.Set(document, meta.Cache.Expire);
                    }
                }
            }

            Cache.Set(meta, meta.Cache.Expire);
        }

        public virtual void BulkCreate(DocumentMeta meta, DocumentInfo info, List<Document> docs, Format format = Format.None)
        {
            var formatting = format == Format.Indented ? Formatting.Indented : Formatting.None;
            var json = new JSONSerializer(Logger);
            var map = new DocumentMap(meta.Primary, meta.Primary);
            json.CreateRows(info, docs.Select(d => d.Data).ToList(), formatting);

            OnChangeDocuments(meta, info, docs, OperationType.Create);
        }

        public virtual void BulkAppend(DocumentMeta meta, DocumentInfo info, List<Document> docs, Format format = Format.None)
        {
            var formatting = format == Format.Indented ? Formatting.Indented : Formatting.None;
            var json = new JSONSerializer(Logger);
            var map = new DocumentMap(meta.Primary, meta.Primary);
            json.AppendRows(info, docs.Select(d => d.Data).ToList(), true, formatting);

            OnChangeDocuments(meta, info, docs, OperationType.Append);
        }

        public virtual void BulkMerge(DocumentMeta meta, DocumentInfo info, List<Document> docs, Format format = Format.None)
        {
            var formatting = format == Format.Indented ? Formatting.Indented : Formatting.None;
            var json = new JSONSerializer(Logger);
            var map = new DocumentMap(meta.Primary, meta.Primary);
            json.MergeRows(info, docs.Select(d => d.Data).ToList(), map, true, formatting);

            OnChangeDocuments(meta, info, docs, OperationType.Merge);
        }

        public virtual void BulkDelete(DocumentMeta meta, DocumentInfo info, List<Document> docs, Format format = Format.None)
        {
            var formatting = format == Format.Indented ? Formatting.Indented : Formatting.None;
            var json = new JSONSerializer(Logger);
            var map = new DocumentMap(meta.Primary, meta.Primary);
            json.DeleteRows(info, docs.Select(d => d.Data).ToList(), map, true, formatting);

            OnChangeDocuments(meta, info, docs, OperationType.Delete);
        }
    }
}
