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
        public virtual void SubmitChanges(DocumentContext context)
        {
            if (context.HasDocuments)
            {
                Logger?.Debug(DebugType.Engine, $"Document write");

                try
                {
                    if (context.Operation == OperationType.Append)
                    {
                        BulkAppend(context);
                    }
                    else if (context.Operation == OperationType.Merge)
                    {
                        BulkMerge(context);
                    }
                    else if (context.Operation == OperationType.Create)
                    {
                        BulkCreate(context);
                    }
                    else if (context.Operation == OperationType.Delete)
                    {
                        BulkDelete(context);
                    }

                    Transaction.AddOperation(context);
                }
                catch (Exception ex)
                {
                    Logger?.Fatal(ex);

                    throw ex;
                }
            }
        }

        internal virtual void OnChangeDocuments(DocumentMeta meta, DocumentContext context)
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
                var refs = meta.GetRefs(false);

                for (int i = 0; i < count; i++)
                {
                    Document document = context.Documents[i];

                    if (Option.SupportDocumentParser)
                    {
                        ParseDocumentData(ref meta, ref document, context.Storage);
                    }

                    meta.SubmitChanges(document, context.Operation);

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

        public virtual void BulkCreate(DocumentContext context)
        {
            var formatting = context.Formatting == Format.Indented ? Formatting.Indented : Formatting.None;
            var json = new JSONSerializer(Logger);
            json.CreateRows(context.Storage, context.Documents.Select(d => d.Data).ToList(), formatting);
        }

        public virtual void BulkAppend(DocumentContext context)
        {
            var formatting = context.Formatting == Format.Indented ? Formatting.Indented : Formatting.None;
            var json = new JSONSerializer(Logger);
            json.AppendRows(context.Storage, context.Documents.Select(d => d.Data).ToList(), formatting);
        }

        public virtual void BulkMerge(DocumentContext context)
        {
            var formatting = context.Formatting == Format.Indented ? Formatting.Indented : Formatting.None;
            var json = new JSONSerializer(Logger);
            var map = new DocumentMap(context.Primary, context.Primary);
            json.MergeRows(context.Storage, context.Documents.Select(d => d.Data).ToList(), map, formatting);
        }

        public virtual void BulkDelete(DocumentContext context)
        {
            var formatting = context.Formatting == Format.Indented ? Formatting.Indented : Formatting.None;
            var json = new JSONSerializer(Logger);
            var map = new DocumentMap(context.Primary, context.Primary);
            json.DeleteRows(context.Storage, context.Documents.Select(d => d.Data).ToList(), map, formatting);
        }
    }
}
