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

namespace Objectiks
{
    public abstract class DocumentEngine : IDocumentEngine
    {
        public DocumentManifest Manifest { get; private set; }
        public IDocumentConnection Connection { get; private set; }
        public IDocumentCache Cache { get; private set; }


        public DocumentEngine(DocumentManifest manifest, IDocumentConnection connections, IDocumentCache cache)
        {
            Manifest = manifest;
            Connection = connections;
            Cache = cache;
        }

        internal virtual List<string> LoadAllDocumentType(DocumentTypes typeOfs)
        {
            var typeOfList = new List<string>();
            foreach (var typeOf in typeOfs)
            {
                if (LoadDocumentType(typeOf))
                {
                    typeOfList.Add(typeOf);
                }
            }

            return typeOfList;
        }

        protected virtual DocumentSchema GetDocumentSchema(string typeOf)
        {
            var schema_file = new FileInfo(Path.Combine(Connection.BaseDirectory, DocumentDefaults.Schemes, $"{typeOf}.json"));
            var schema = new DocumentSerializer().Get<DocumentSchema>(schema_file.FullName);

            if (schema == null)
            {
                schema = ObjectiksOf.Core.DefaultSchema;
                schema.Cache = Manifest.Cache;
            }

            if (schema.Cache == null)
            {
                schema.Cache = Manifest.Cache;
            }

            if (String.IsNullOrEmpty(schema.ParseOf))
            {
                schema.ParseOf = DocumentDefaults.DocumentParseOf;
            }

            if (String.IsNullOrWhiteSpace(schema.Primary))
            {
                schema.Primary = DocumentDefaults.DocumentPrimaryOf;
            }

            if (schema.KeyOf == null)
            {
                schema.KeyOf = Manifest.KeyOf;
            }
            else if (schema.KeyOf.Count == 0)
            {
                schema.KeyOf = Manifest.KeyOf;
            }

            return schema;
        }

        protected virtual void UpdateDocumentMeta(ref DocumentMeta meta, ref Document document, DocumentInfo file, OperationType operation)
        {
            JObject target = document.Data;

            var typeOf = meta.TypeOf;
            var primary = meta.Primary;

            document.Primary = target[primary];
            document.CacheOf = Cache.CacheOfDocument(typeOf, document.Primary);

            if (!target.ContainsKey(DocumentDefaults.DocumentTypeOf))
            {
                target["Type"] = meta.TypeOf;
            }

            #region KeyOf Generate
            var keyOfProperties = meta.KeyOf;
            var keyOfValues = new List<string>();

            if (!keyOfProperties.Contains(primary))
            {
                keyOfProperties.Add(primary);
            }

            foreach (var key in keyOfProperties)
            {
                if (target.ContainsKey(key))
                {
                    var keyValue = target[key].AsString().ToLowerInvariant();

                    if (!String.IsNullOrEmpty(keyValue))
                    {
                        keyOfValues.Add(keyValue);
                    }
                }
                else
                {
                    throw new ArgumentNullException($"{typeOf} Schema Key {key} undefined..");
                }
            }
            #endregion

            meta.UpdateSequence(document.Primary);

            /*
            Create = 1,
            Append = 2,
            Merge = 3,
            Delete = 4
             */

            if (operation == OperationType.None)
            {
                meta.TotalRecords++;
                meta.Partitions[file.Partition]++;
                meta.AddKeys(new DocumentKey(document.Primary.ToString(), document.CacheOf, keyOfValues, file.Partition));
            }
            else if (operation == OperationType.Delete)
            {
                meta.TotalRecords--;
                meta.Partitions[file.Partition]--;
                meta.RemoveKeys(document.Primary);
            }
            else if (operation == OperationType.Create || operation == OperationType.Append)
            {
                meta.TotalRecords++;
                meta.Partitions[file.Partition]++;
                meta.AddKeys(new DocumentKey(document.Primary.ToString(), document.CacheOf, keyOfValues, file.Partition));
            }
            else if (operation == OperationType.Merge)
            {
                meta.UpdateKeys(new DocumentKey(document.Primary.ToString(), document.CacheOf, keyOfValues, file.Partition));
            }

            document.Data = target;
        }

        protected virtual void ParseDocumentData(ref DocumentMeta meta, ref Document document, DocumentInfo file)
        {
            var parser = ObjectiksOf.Core.GetDocumentParser(meta.TypeOf);

            if (parser != null)
            {
                parser.Parse(this, meta, document, file);
            }
        }

        protected virtual bool ParseDocumentRefs(List<DocumentRef> refs, ref Document document)
        {
            foreach (var docRef in refs)
            {
                var parser = ObjectiksOf.Core.GetReferenceParser(docRef);

                if (!parser.IsValidRef(docRef))
                {
                    throw new Exception($"Document ref schema incorrect {document.TypeOf} -> {docRef?.ParseOf} - {docRef?.TypeOf}");
                }

                parser.Parse(this, document, docRef);
            }

            return true;
        }

        public abstract bool LoadDocumentType(string typeOf);

        public virtual JArray ReadList(QueryOf query, DocumentMeta meta = null)
        {
            var results = new JArray();

            if (meta == null)
            {
                //read document meta data..
                meta = GetTypeMeta(query.TypeOf);
            }

            if (query.HasPrimaryOf)
            {
                foreach (var primaryOf in query.PrimaryOfList)
                {
                    var document = Read(query.TypeOf, primaryOf);

                    if (document == null)
                    {
                        continue;
                    }

                    if (!document.Exists)
                    {
                        continue;
                    }

                    //check schema refs
                    if (meta.HasLazy && query.Lazy)
                    {
                        //default refs
                        ParseDocumentRefs(meta.GetRefs(true), ref document);
                    }

                    //check dynamic refs..
                    if (query.HasRefs && query.Lazy)
                    {
                        //custom refs
                        ParseDocumentRefs(query.RefList, ref document);
                    }

                    results.Add(((JObject)document.Data));
                }
            }
            else
            {
                //get KeyOf match items..
                List<DocumentKey> documentKeys = meta.GetDocumentKeysFromQueryOf(query);

                foreach (var key in documentKeys)
                {
                    //direct read from cache key..
                    var document = Read(query.TypeOf, key.PrimaryOf);

                    if (document == null)
                    {
                        continue;
                    }

                    if (!document.Exists)
                    {
                        continue;
                    }

                    //check schema refs
                    if (meta.HasLazy && query.Lazy)
                    {
                        ParseDocumentRefs(meta.GetRefs(true), ref document);
                    }

                    //check dynamic refs..
                    if (query.HasRefs && query.Lazy)
                    {
                        ParseDocumentRefs(query.RefList, ref document);
                    }

                    results.Add(((JObject)document.Data));
                }
            }

            if (query.HasOrderBy)
            {
                throw new Exception("JArray OrderBy not supported..");
            }

            return results;
        }

        

        public abstract Document Read(string typeOf, object primaryOf);
        public abstract Document Read(QueryOf query, DocumentMeta meta = null);


        public abstract List<T> ReadList<T>(QueryOf query);
        public abstract T Read<T>(QueryOf query, DocumentMeta meta = null);
        public abstract T GetCount<T>(QueryOf query, DocumentMeta meta = null);

        public abstract DocumentMeta GetTypeMeta(string typeOf);
        public abstract List<DocumentMeta> GetTypeMetaAll();

        public abstract void Write(DocumentMeta meta, DocumentInfo info, List<Document> docs, OperationType operation, Format format);
    }
}
