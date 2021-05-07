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
        public DocumentOption Option { get; private set; }
        public DocumentProvider Provider { get; internal set; }
        public IDocumentLogger Logger { get; private set; }
        public IDocumentCache Cache { get; private set; }
        public IDocumentWatcher Watcher { get; private set; }
        public List<IParser> ParseOf { get; private set; }

        private readonly DocumentMonitor Monitor;

        public DocumentEngine()
        {
            Monitor = new DocumentMonitor();
        }

        public DocumentEngine(DocumentProvider documentProvider, DocumentOption option)
        {
            Option = option;
            Provider = documentProvider;
            Cache = option.CacheInstance;
            Logger = option.DocumentLogger;
            Watcher = option.DocumentWatcher;
            ParseOf = option.ParserOfTypes;
            Monitor = new DocumentMonitor();

            if (Option.SupportDocumentWatcher)
            {
                Watcher?.WaitForChanged(this);
            }
        }


        protected virtual DocumentSchema GetDocumentSchema(string typeOf)
        {
            DocumentSchema schema = null;

            if (Option.Schemes != null)
            {
                schema = Option.Schemes.Where(s => s.TypeOf == typeOf).FirstOrDefault();
            }

            if (schema == null)
            {
                var schema_file = new FileInfo(Path.Combine(Provider.BaseDirectory, DocumentDefaults.Schemes, $"{typeOf}.json"));
                schema = new JSONSerializer().Get<DocumentSchema>(schema_file.FullName);
            }

            if (schema == null)
            {
                schema = DocumentSchema.Default();
                schema.Cache = Option.CacheInfo;

                Logger?.Debug(ScopeType.Engine, $"TypeOf: {typeOf} GetDocumentSchema Schema is null");
            }

            if (schema.Cache == null)
            {
                schema.Cache = Option.CacheInfo;
            }

            if (String.IsNullOrEmpty(schema.ParseOf))
            {
                schema.ParseOf = DocumentDefaults.DocumentParseOf;
            }

            if (String.IsNullOrWhiteSpace(schema.PrimaryOf))
            {
                schema.PrimaryOf = DocumentDefaults.DocumentPrimaryOf;
            }

            if (String.IsNullOrWhiteSpace(schema.WorkOf))
            {
                schema.WorkOf = Option.WorkOf;
            }

            if (String.IsNullOrWhiteSpace(schema.UserOf))
            {
                schema.UserOf = Option.User;
            }

            if (schema.KeyOf == null)
            {
                schema.KeyOf = Option.KeyOf != null ? Option.KeyOf : new DocumentKeyOfNames();
            }
            else if (schema.KeyOf.Count == 0)
            {
                schema.KeyOf = Option.KeyOf != null ? Option.KeyOf : new DocumentKeyOfNames();
            }

            return schema;
        }

        protected virtual Document GetDocumentFromSource(ref DocumentMeta meta, JObject data, int partition)
        {
            var document = new Document();
            document.TypeOf = meta.TypeOf;
            document.PrimaryOf = data[meta.Primary].AsString();
            document.CacheOf = Cache.CacheOfDoc(meta.TypeOf, document.PrimaryOf);
            document.KeyOf = data.ToKeyOfValues(meta.TypeOf, meta.KeyOfNames, meta.Primary);
            document.Partition = partition;
            document.CreatedAt = DateTime.UtcNow;

            if (!String.IsNullOrEmpty(meta.Workspace) && data.ContainsKey(meta.Workspace))
            {
                document.WorkOf = data[meta.Workspace].AsString();
            }
            else
            {
                document.WorkOf = "0";
            }

            if (!String.IsNullOrEmpty(meta.User) && data.ContainsKey(meta.User))
            {
                document.UserOf = data[meta.User].AsString();
            }
            else
            {
                document.UserOf = "0";
            }

            if (!data.ContainsKey(DocumentDefaults.DocumentTypeOf))
            {
                data["Type"] = meta.TypeOf;
            }

            document.Data = data;

            return document;
        }

        protected virtual void ParseDocumentData(ref DocumentMeta meta, ref Document document, DocumentStorage file)
        {
            var parser = GetDocumentParser(meta.TypeOf);

            if (parser != null)
            {
                parser.Parse(this, meta, document, file);
            }
        }

        protected virtual bool ParseDocumentRefs(List<DocumentRef> refs, ref Document document)
        {
            foreach (var docRef in refs)
            {
                var parser = GetReferenceParser(docRef);

                if (!parser.IsValidRef(docRef))
                {
                    throw new Exception($"Document ref schema incorrect {document.TypeOf} -> {docRef?.ParseOf} - {docRef?.TypeOf}");
                }

                parser.Parse(this, document, docRef);
            }

            return true;
        }

        protected virtual IDocumentParser GetDocumentParser(string typeOf)
        {
            var converter = ParseOf.Where(c => c.ParseOf == typeOf).FirstOrDefault();

            if (converter == null)
            {
                converter = ParseOf.Where(c => c.ParseOf == DocumentDefaults.DocumentParseOf).FirstOrDefault();
            }

            if (converter == null)
            {
                return null;
            }

            return (IDocumentParser)converter;
        }

        protected virtual IDocumentRefParser GetReferenceParser(DocumentRef docRef)
        {
            var converter = ParseOf.Where(c => c.ParseOf == docRef.ParseOf).FirstOrDefault();

            Ensure.NotNull(converter, $"Core ReferenceParser Type : {docRef.ParseOf} -> Parser undefined..");

            return (IDocumentRefParser)converter;
        }

        protected virtual DocumentManifest GetDocumentManifest(string baseDirectory)
        {
            var path = Path.Combine(baseDirectory, DocumentDefaults.Manifest);

            return DocumentManifest.Get(path);
        }

        internal virtual DocumentTransaction BeginTransaction()
        {
            return GetTransaction(true, false);
        }

        internal virtual DocumentTransaction BeginInternalTransaction()
        {
            return GetTransaction(true, true);
        }

        internal virtual DocumentTransaction GetThreadTransaction()
        {
            return GetTransaction(false, false);
        }

        internal virtual DocumentTransaction GetTransaction(bool create, bool isInternal)
        {
            return Monitor.GetTransaction(this, create, isInternal);
        }

        internal virtual void ReleaseTransaction(DocumentTransaction transaction)
        {
            Monitor.ReleaseTransaction(transaction);
        }
    }
}
