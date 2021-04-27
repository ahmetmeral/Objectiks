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
        public DocumentTypes TypeOf { get; set; }
        public bool FirstLoaded { get; set; }

        public DocumentEngine() { }

        public DocumentEngine(DocumentProvider documentProvider, DocumentOption option)
        {
            Option = option;
            Provider = documentProvider;
            Cache = option.CacheInstance;
            Logger = option.DocumentLogger;
            Watcher = option.DocumentWatcher;
            ParseOf = GetDocumentParsers(option.ParserOfTypes);
            TypeOf = new DocumentTypes();

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

                Logger?.Debug(DebugType.Engine, $"TypeOf: {typeOf} GetDocumentSchema Schema is null");
            }

            if (schema.Cache == null)
            {
                schema.Cache = Option.CacheInfo;
            }

            if (String.IsNullOrEmpty(schema.ParseOf))
            {
                schema.ParseOf = DocumentDefaults.DocumentParseOf;
            }

            if (String.IsNullOrWhiteSpace(schema.Primary))
            {
                schema.Primary = DocumentDefaults.DocumentPrimaryOf;
            }

            if (String.IsNullOrWhiteSpace(schema.Account))
            {
                schema.Account = Option.Account;
            }

            if (String.IsNullOrWhiteSpace(schema.User))
            {
                schema.User = Option.User;
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

        protected virtual void UpdateDocumentMeta(ref DocumentMeta meta, ref Document document, int partition, OperationType operation)
        {
            JObject target = document.Data;

            document.PrimaryOf = target[meta.Primary].AsString();
            document.CacheOf = Cache.CacheOfDocument(meta.TypeOf, document.PrimaryOf);
            document.KeyOf = target.ToKeyOfValues(meta.TypeOf, meta.KeyOfNames, meta.Primary);
            document.Partition = partition;

            if (!String.IsNullOrEmpty(meta.Account) && target.ContainsKey(meta.Account))
            {
                document.WorkOf = target[meta.Account].AsString();
            }

            if (!String.IsNullOrEmpty(meta.User) && target.ContainsKey(meta.User))
            {
                document.UserOf = target[meta.User].AsString();
            }

            if (!target.ContainsKey(DocumentDefaults.DocumentTypeOf))
            {
                target["Type"] = meta.TypeOf;
            }

            meta.SubmitChanges(document, operation);

            document.Data = target;
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

        protected virtual List<IParser> GetDocumentParsers(List<Type> parseOfList)
        {
            var list = new List<IParser>();

            foreach (var parseOf in parseOfList)
            {
                list.Add((IParser)Activator.CreateInstance(parseOf));
            }

            return list;
        }

        internal virtual DocumentTransaction BeginTransaction()
        {
            return new DocumentTransaction(this, false);
        }
    }
}
