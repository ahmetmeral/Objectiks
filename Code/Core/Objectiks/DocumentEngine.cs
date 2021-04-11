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
    public class DocumentEngine : IDocumentEngine
    {
        public DocumentOption Option { get; private set; }
        public IDocumentLogger Logger { get; private set; }
        public DocumentProvider Provider { get; internal set; }
        public IDocumentCache Cache { get; private set; }
        public IDocumentWatcher Watcher { get; private set; }
        public List<IParser> ParseOf { get; private set; }
        public DocumentTypes TypeOf { get; set; }
        public bool FirstLoaded { get; set; }

        public DocumentEngine() { }

        internal DocumentEngine(DocumentProvider documentProvider, DocumentOption option)
        {
            Option = option;
            Provider = documentProvider;
            Cache = GetDocumentCache(option.CacheType, Provider.CacheBucket);
            Logger = GetDocumentLogger(option.LoggerType);
            Watcher = GetDocumentWatcher(option.WatcherType);
            ParseOf = GetDocumentParsers(option.ParserOfTypes);
            TypeOf = new DocumentTypes();

            if (Option.SupportDocumentWatcher)
            {
                Watcher?.WaitForChanged(this);
            }
        }

        internal virtual void FirstLoadAllDocumentType()
        {
            Watcher?.Lock();

            foreach (var typeOf in Option.TypeOf)
            {
                CheckDirectoryOrSchema(typeOf);

                if (LoadDocumentType(typeOf))
                {
                    TypeOf.Add(typeOf.ToLowerInvariant());

                }
            }

            Watcher?.UnLock();

            FirstLoaded = true;
        }

        //
        public virtual bool LoadDocumentType(string typeOf)
        {
            Logger?.Debug(DebugType.Engine, $"Load TypeOf: {typeOf}");

            var schema = GetDocumentSchema(typeOf);
            var meta = new DocumentMeta(typeOf, schema, Provider);
            var files = new List<DocumentInfo>();

            #region Files
            var directoryInfo = new DirectoryInfo(meta.Directory);
            if (directoryInfo.Exists)
            {
                var extentions = Option.Extention;
                var directoryFiles = directoryInfo.GetFiles(extentions, SearchOption.TopDirectoryOnly);

                meta.HasData = directoryFiles.Length > 0;

                var parts = new List<int>();
                foreach (var file in directoryFiles)
                {
                    var info = new DocumentInfo(meta.TypeOf, Provider?.BaseDirectory, file);

                    if (info.Partition > meta.Partitions.Current)
                    {
                        meta.Partitions.Current = info.Partition;
                    }

                    if (!parts.Contains(info.Partition))
                    {
                        parts.Add(info.Partition);
                    }

                    meta.DiskSize += file.Length;

                    files.Add(info);
                }

                parts = parts.OrderBy(p => p).ToList();

                foreach (var part in parts)
                {
                    meta.Partitions.Add(part, 0);
                }

                meta.Partitions.Next = meta.Partitions.Current + 1;
            }
            #endregion

            if (files.Count == 0)
            {
                Logger?.Debug(DebugType.Engine, "LoadDocumentType files.count = 0");

                return false;
            }

            Logger?.Debug(DebugType.Engine, $"TypeOf:{typeOf} number of files : {files.Count}");

            int bufferSize = Option.BufferSize;
            var serializer = new JsonSerializer();

            foreach (DocumentInfo file in files)
            {
                Logger?.Debug(DebugType.Engine, $"Read TypeOf: {file.TypeOfName} - File : {file.FullName}");

                try
                {
                    using (FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize))
                    using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, true, bufferSize))
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        reader.SupportMultipleContent = false;

                        while (reader.Read())
                        {
                            if (reader.TokenType == JsonToken.StartObject)
                            {
                                var document = new Document
                                {
                                    TypeOf = typeOf,
                                    Data = serializer.Deserialize<JObject>(reader),
                                    Partition = file.Partition,
                                    HasLazy = meta.HasLazy,
                                    CreatedAt = DateTime.UtcNow
                                };

                                UpdateDocumentMeta(ref meta, ref document, file, OperationType.None);
                                ParseDocumentData(ref meta, ref document, file);
                                ParseDocumentRefs(meta.GetRefs(false), ref document);

                                Cache.Set(document, meta.Cache.Expire);

                                document.Dispose();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger?.Fatal($"Exception Read TypeOf: {file.TypeOfName} File : {file.FullName}", ex);
                }
            }

            Cache.Set(meta, meta.Cache.Expire);

            return true;
        }




        protected virtual void CheckDirectoryOrSchema(string typeOf)
        {
            Logger?.Debug(DebugType.Engine, "Check Document Directory and Schema");

            var documents = Path.Combine(Provider.BaseDirectory, DocumentDefaults.Documents, typeOf);
            if (!Directory.Exists(documents))
            {
                Directory.CreateDirectory(documents);

                Logger?.Debug(DebugType.Engine, $"TypeOf:{typeOf} directory created.. Directory : {documents}");
            }

            var docFile = Path.Combine(documents, $"{typeOf}.json");
            if (!File.Exists(docFile))
            {
                File.WriteAllText(docFile, "[]", Encoding.UTF8);

                Logger?.Debug(DebugType.Engine, $"TypeOf:{typeOf} document created.. File: {docFile}");
            }

            var docSchema = Path.Combine(Provider.BaseDirectory, DocumentDefaults.Schemes, $"{typeOf}.json");

            if (!File.Exists(docSchema))
            {
                var temporySchema = DocumentSchema.Default();
                temporySchema.TypeOf = typeOf;
                temporySchema.ParseOf = "Document";
                temporySchema.KeyOf = Option.KeyOf;
                temporySchema.Primary = Option.Primary;

                var schema = new DocumentSerializer().Serialize(temporySchema);

                File.WriteAllText(docSchema, schema, Encoding.UTF8);

                Logger?.Debug(DebugType.Engine, $"TypeOf:{typeOf} schema created.. File: {docSchema}");
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
                schema = new DocumentSerializer().Get<DocumentSchema>(schema_file.FullName);
            }

            if (schema == null)
            {
                schema = DocumentSchema.Default();
                schema.Cache = Option.Cache;

                Logger?.Debug(DebugType.Engine, $"TypeOf: {typeOf} GetDocumentSchema Schema is null");
            }

            if (schema.Cache == null)
            {
                schema.Cache = Option.Cache;
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
                schema.KeyOf = Option.KeyOf != null ? Option.KeyOf : new DocumentKeyOfNames();
            }
            else if (schema.KeyOf.Count == 0)
            {
                schema.KeyOf = Option.KeyOf != null ? Option.KeyOf : new DocumentKeyOfNames();
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
            var keyOfProperties = meta.KeyOfNames;
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
                Cache.Remove(typeOf, document.Primary);
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

        protected virtual DocumentCache GetDocumentCache(Type type, string bucket = "DefaultBucket")
        {
            return (DocumentCache)Activator.CreateInstance(type, bucket);
        }

        protected virtual DocumentWatcher GetDocumentWatcher(Type type)
        {
            if (type == null)
            {
                return null;
            }

            return (DocumentWatcher)Activator.CreateInstance(type);
        }

        protected virtual IDocumentLogger GetDocumentLogger(Type type)
        {
            if (type == null)
            {
                return null;
            }

            return (IDocumentLogger)Activator.CreateInstance(type);
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

        public virtual Document Read(string typeOf, object primaryOf)
        {
            var document = Cache.GetOrCreate(typeOf, primaryOf, () =>
            {
                LoadDocumentType(typeOf);

                return Cache.Get(typeOf, primaryOf);
            });

            return document;
        }

        public virtual Document Read(QueryOf query, DocumentMeta meta = null)
        {
            if (meta == null)
            {
                meta = GetTypeMeta(query.TypeOf);
            }

            List<DocumentKey> documentKeys = meta.GetDocumentKeysFromQueryOf(query);

            if (documentKeys.Count > 1 || documentKeys.Count == 0)
            {
                return null;
            }

            return Read(query.TypeOf, documentKeys[0].PrimaryOf);
        }

        public virtual T Read<T>(QueryOf query, DocumentMeta meta = null)
        {
            if (meta == null)
            {
                meta = GetTypeMeta(query.TypeOf);
            }

            Document document;

            if (query.HasPrimaryOf)
            {
                document = Read(query.TypeOf, query.PrimaryOfList[0]);
            }
            else
            {
                document = Read(query, meta);
            }

            if (document == null)
            {
                return default(T);
            }

            if (!document.Exists)
            {
                return default(T);
            }

            if (meta.HasLazy && query.Lazy)
            {
                //default refs
                ParseDocumentRefs(meta.GetRefs(true), ref document);
            }

            if (query.HasRefs && query.Lazy)
            {
                //custom refs
                ParseDocumentRefs(query.RefList, ref document);
            }

            return ((JObject)document.Data).ToObject<T>();
        }

        public virtual List<T> ReadList<T>(QueryOf query)
        {
            var results = new List<T>();
            //read document meta data..
            var meta = GetTypeMeta(query.TypeOf);

            if (query.HasPrimaryOf && !query.HasKeyOf)
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
                        ParseDocumentRefs(meta.GetRefs(true), ref document);
                    }

                    //check dynamic refs..
                    if (query.HasRefs && query.Lazy)
                    {
                        ParseDocumentRefs(query.RefList, ref document);
                    }

                    results.Add(((JObject)document.Data).ToObject<T>());
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

                    results.Add(((JObject)document.Data).ToObject<T>());
                }
            }

            if (query.HasOrderBy)
            {
                return results.AsQueryable().OrderBy(query.AsOrderBy()).ToList();
            }

            return results;
        }

        public virtual T GetCount<T>(QueryOf query, DocumentMeta meta = null)
        {
            if (meta == null)
            {
                meta = GetTypeMeta(query.TypeOf);
            }

            if (query.HasPrimaryOf || query.HasKeyOf)
            {
                return meta.GetCountFromQueryOf<T>(query);
            }
            else
            {
                return meta.TotalRecords.ChangeType<T>();
            }
        }

        public virtual List<DocumentMeta> GetTypeMetaAll()
        {
            var list = new List<DocumentMeta>();

            foreach (var typeOf in TypeOf)
            {
                var meta = GetTypeMeta(typeOf);

                if (meta != null)
                {
                    list.Add(meta);
                }
            }

            return list;
        }

        public virtual DocumentMeta GetTypeMeta(string typeOf)
        {
            var meta = Cache.GetOrCreate(typeOf, () =>
            {
                LoadDocumentType(typeOf);

                return Cache.Get(typeOf);
            });

            return meta;
        }

        public virtual void Write(DocumentMeta meta, DocumentInfo info, List<Document> docs, OperationType operation, Format format)
        {
            int count = docs.Count;

            if (count > 0)
            {
                Logger?.Debug(DebugType.Engine, $"Document write : number of documents : {count}");

                try
                {
                    for (int i = 0; i < count; i++)
                    {
                        Document document = docs[i];
                        UpdateDocumentMeta(ref meta, ref document, info, operation);
                    }

                    var json = new JSONSerializer(Logger);
                    var map = new DocumentMap(meta.Primary, meta.Primary);
                    var formatting = format == Format.Indented ? Formatting.Indented : Formatting.None;

                    if (operation == OperationType.Append)
                    {
                        json.AppendRows(info, docs.Select(d => d.Data).ToList(), true, formatting);
                    }
                    else if (operation == OperationType.Merge)
                    {
                        json.MergeRows(info, docs.Select(d => d.Data).ToList(), map, true, formatting);
                    }
                    else if (operation == OperationType.Create)
                    {
                        json.CreateRows(info, docs.Select(d => d.Data).ToList(), formatting);
                    }
                    else if (operation == OperationType.Delete)
                    {
                        json.DeleteRows(info, docs.Select(d => d.Data).ToList(), map, true, formatting);
                    }

                    if (operation != OperationType.Delete)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            Document document = docs[i];

                            if (meta.Refs != null && meta.Refs.Count > 0)
                            {
                                ParseDocumentRefs(meta.GetRefs(false), ref document);
                            }

                            Cache.Set(document, meta.Cache.Expire);
                        }
                    }

                    Cache.Set(meta, meta.Cache.Expire);
                }
                catch (Exception ex)
                {
                    Logger?.Fatal(ex);

                    throw ex;
                }
            }
        }



    }
}
