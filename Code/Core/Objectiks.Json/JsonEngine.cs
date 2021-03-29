using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Objectiks.Engine.Query;
using Objectiks.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Linq.Dynamic.Core;
using Objectiks.Extentions;
using Objectiks.Engine;
using Objectiks.Services;

namespace Objectiks.Json
{
    public class JsonEngine : DocumentEngine
    {
        public JsonEngine(DocumentManifest manifest, IDocumentConnection connection, IDocumentCache cache) :
            base(manifest, connection, cache)
        {
        }

        public override bool LoadDocumentType(string typeOf)
        {
            var schema = GetDocumentSchema(typeOf);
            var meta = new DocumentMeta(typeOf, schema, Connection);
            var files = new List<DocumentInfo>();

            #region Files
            var directoryInfo = new DirectoryInfo(meta.Directory);
            if (directoryInfo.Exists)
            {
                var extentions = Manifest.Extention.Documents;
                var directoryFiles = directoryInfo.GetFiles(extentions, SearchOption.TopDirectoryOnly);

                meta.HasData = directoryFiles.Length > 0;

                var parts = new List<int>();
                foreach (var file in directoryFiles)
                {
                    var info = new DocumentInfo(meta.TypeOf, file);

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
                return false;
            }

            int bufferSize = Manifest.BufferSize;
            var serializer = new JsonSerializer();

            foreach (DocumentInfo file in files)
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

            Cache.Set(meta, meta.Cache.Expire);

            return true;
        }


        public override Document Read(string typeOf, object primaryOf)
        {
            var document = Cache.GetOrCreate(typeOf, primaryOf, () =>
             {
                 LoadDocumentType(typeOf);

                 return Cache.Get(typeOf, primaryOf);
             });

            return document;
        }

        public override Document Read(QueryOf query, DocumentMeta meta = null)
        {
            if (meta == null)
            {
                meta = GetTypeMeta(query.TypeOf);
            }

            List<DocumentKey> documentKeys = meta.GetDocumentKeysFromQueryOf(query);

            if (documentKeys.Count > 1)
            {
                return null;
            }

            return Read(query.TypeOf, documentKeys[0].PrimaryOf);
        }

      


        public override T Read<T>(QueryOf query, DocumentMeta meta = null)
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

        public override List<T> ReadList<T>(QueryOf query)
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

        public override T GetCount<T>(QueryOf query, DocumentMeta meta = null)
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

        public override List<DocumentMeta> GetTypeMetaAll()
        {
            var list = new List<DocumentMeta>();

            foreach (var typeOf in ObjectiksOf.Core.TypeOf)
            {
                var meta = GetTypeMeta(typeOf);

                if (meta != null)
                {
                    list.Add(meta);
                }
            }

            return list;
        }

        public override DocumentMeta GetTypeMeta(string typeOf)
        {
            var meta = Cache.GetOrCreate(typeOf, () =>
            {
                LoadDocumentType(typeOf);

                return Cache.Get(typeOf);
            });

            return meta;
        }

        public override void Write(DocumentMeta meta, DocumentInfo info, List<Document> docs, OperationType operation, Format format)
        {
            int count = docs.Count;

            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    Document document = docs[i];
                    document.Data = JObject.FromObject(document.Data);
                    UpdateDocumentMeta(ref meta, ref document, info, operation);

                    if (operation != OperationType.Delete)
                    {
                        if (meta.Refs != null && meta.Refs.Count > 0)
                        {
                            ParseDocumentRefs(meta.GetRefs(false), ref document);
                        }

                        Cache.Set(document, meta.Cache.Expire);
                    }
                }

                var json = new JSONSerializer();
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

                Cache.Set(meta, meta.Cache.Expire);
            }
        }
    }
}
