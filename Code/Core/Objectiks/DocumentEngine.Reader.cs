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
using System.Threading;

namespace Objectiks
{
    public partial class DocumentEngine : IDocumentEngine
    {
        public virtual JArray ReadList(DocumentQuery query, DocumentMeta meta = null)
        {
            var results = new JArray();

            if (meta == null)
            {
                //read document meta data..
                meta = GetTypeMeta(query.TypeOf);
            }

            //get KeyOf match items..
            QueryResult queryResult = GetDocumentKeysFromQueryOf(query, meta);

            foreach (var key in queryResult.Keys)
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
                if (meta.HasRefs)
                {
                    ParseDocumentRefs(meta.GetRefs(true), ref document);
                }

                //check dynamic refs..
                if (query.HasRefs)
                {
                    ParseDocumentRefs(query.Refs, ref document);
                }

                results.Add(((JObject)document.Data));
            }

            if (query.HasOrderBy)
            {
                throw new Exception("JArray OrderBy not supported..");
            }

            return results;
        }

        public virtual Document Read(string typeOf, object primaryOf)
        {
            var document = Cache.GetOrCreateDocument(typeOf, primaryOf, () =>
            {
                LoadDocumentType(typeOf);

                return Cache.Get(typeOf, primaryOf);
            });

            return document;
        }

        public virtual DocumentInfo GetTypeOfDocumentInfo(string typeOf, object primaryOf, Type primaryOfDataType)
        {
            try
            {
                Ensure.NotNullOrEmpty(typeOf, "Document info typeOf is empty");

                var info = new DocumentInfo(typeOf);

                var sequence = Cache.GetOrCreateSequence(typeOf, () =>
                {
                    LoadDocumentType(typeOf);

                    return Cache.GetSequence(typeOf);

                }).GetTypeOfSequence(primaryOfDataType, primaryOf);

                if (sequence.IsNew)
                {
                    info.PrimaryOf = sequence.Value;
                    info.Partition = 0;
                    info.Exists = false;

                    Cache.Set(new DocumentSequence(typeOf, sequence.Value));
                    Cache.Set(info);
                }
                else
                {
                    var readInfo = Cache.GetDocumentInfo(typeOf, sequence.Value);

                    if (readInfo.Exists)
                    {
                        info.PrimaryOf = readInfo.PrimaryOf;
                        info.Partition = readInfo.Partition;
                        info.Exists = true;
                    }
                    else
                    {
                        info.PrimaryOf = sequence.Value;
                        info.Partition = 0;
                        info.Exists = false;

                        Cache.Set(new DocumentSequence(typeOf, sequence.Value));
                        Cache.Set(info);
                    }
                }

                return info;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual Document Read(DocumentQuery query, DocumentMeta meta = null)
        {
            Document document = null;

            if (meta == null)
            {
                meta = GetTypeMeta(query.TypeOf);
            }

            var queryResult = GetDocumentKeysFromQueryOf(query, meta);

            if (queryResult.Keys.Count == 1)
            {
                document = Read(query.TypeOf, queryResult.Keys[0].PrimaryOf);

                if (document != null && document.Exists)
                {
                    if (meta.HasRefs)
                    {
                        //default refs
                        ParseDocumentRefs(meta.GetRefs(true), ref document);
                    }

                    if (query.HasRefs)
                    {
                        //custom refs
                        ParseDocumentRefs(query.Refs, ref document);
                    }
                }
            }

            return document;
        }

        public virtual T Read<T>(DocumentQuery query, DocumentMeta meta = null)
        {
            var cacheOfData = ReadAnyCacheOfFromQuery<T>(query);

            if (cacheOfData != null)
            {
                return cacheOfData;
            }

            Document document = Read(query, meta);

            if (document == null)
            {
                return default;
            }

            var data = ((JObject)document.Data).ToObject<T>();

            SetAnyCacheOfDocument(query, data);

            return data;
        }

        public virtual List<T> ReadList<T>(DocumentQuery query)
        {
            var cacheOfData = ReadAnyCacheOfFromQuery<List<T>>(query);

            if (cacheOfData != null)
            {
                return cacheOfData;
            }

            var results = new List<T>();
            //read document meta data..
            var meta = GetTypeMeta(query.TypeOf);

            //get KeyOf match items..
            var queryResult = GetDocumentKeysFromQueryOf(query, meta);

            foreach (var key in queryResult.Keys)
            {
                //direct read from cache key..
                var document = Read(query.TypeOf, key.PrimaryOf);

                if (document == null || !document.Exists)
                {
                    continue;
                }

                //check schema refs
                if (meta.HasRefs)
                {
                    ParseDocumentRefs(meta.GetRefs(true), ref document);
                }

                //check dynamic refs..
                if (query.HasRefs)
                {
                    ParseDocumentRefs(query.Refs, ref document);
                }

                results.Add(((JObject)document.Data).ToObject<T>());
            }

            if (query.HasOrderBy && results.Count > 1)
            {
                return results.AsQueryable().OrderBy(queryResult.Query.OrderBy).ToList();
            }

            SetAnyCacheOfDocument(query, results);

            return results;
        }

        public virtual T GetCount<T>(DocumentQuery query, DocumentMeta meta = null)
        {
            long? readFromCache = ReadAnyCacheOfFromQuery<long?>(query);

            if (readFromCache.HasValue)
            {
                return readFromCache.Value.ChangeType<T>();
            }

            T result;

            if (meta == null)
            {
                meta = GetTypeMeta(query.TypeOf);
            }

            if (query.HasFilter)
            {
                result = GetCountFromQueryOf<T>(query, meta);
            }
            else
            {
                result = meta.TotalRecords.ChangeType<T>();
            }

            SetAnyCacheOfDocument(query, result);

            return result;
        }

        public virtual List<DocumentMeta> GetTypeMetaAll()
        {
            var list = new List<DocumentMeta>();

            foreach (var typeOf in Option.TypeOf)
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
            var meta = Cache.GetOrCreateMeta(typeOf, () =>
            {
                LoadDocumentType(typeOf);

                return Cache.Get(typeOf);
            });

            return meta;
        }

        public virtual T ReadAnyCacheOfFromQuery<T>(DocumentQuery query)
        {
            if (!query.HasCacheOf)
            {
                return default;
            }

            if (query.CacheOf.BeforeCallClear)
            {
                RemoveAnyCacheOfFromQuery(query);

                return default;
            }

            return Cache.Get<T>(query);
        }

        public virtual void RemoveAnyCacheOfFromQuery(DocumentQuery query)
        {
            if (query.CacheOf.BeforeCallClear)
            {
                Cache.Remove(query);
            }
        }

        public virtual void SetAnyCacheOfDocument<T>(DocumentQuery query, T data)
        {
            if (query == null)
            {
                return;
            }

            if (query.HasCacheOf)
            {
                Cache.Set(query, data);
            }
        }

        public virtual T GetCountFromQueryOf<T>(DocumentQuery query, DocumentMeta meta = null)
        {
            if (meta == null)
            {
                meta = GetTypeMeta(query.TypeOf);
            }

            var compiler = query.Compiler();
            return meta.Keys.AsQueryable().Count(compiler.WhereBy,
                     compiler.ValueBy).ChangeType<T>();
        }

        public virtual QueryResult GetDocumentKeysFromQueryOf(DocumentQuery query, DocumentMeta meta = null)
        {
            if (meta == null)
            {
                meta = GetTypeMeta(query.TypeOf);
            }

            List<DocumentKey> keys;

            var queryCompiler = query.Compiler();

            if (query.HasFilter)
            {
                keys = meta.Keys.AsQueryable().Where(queryCompiler.WhereBy, queryCompiler.ValueBy.ToArray())?.ToList();

                if (keys != null && query.Take > 0)
                {
                    keys = keys.Skip(query.Skip).Take(query.Take).ToList();
                }
            }
            else
            {
                if (query.Take > 0)
                {
                    keys = meta.Keys.Skip(query.Skip).Take(query.Take).ToList();
                }
                else
                {
                    keys = meta.Keys.ToList();
                }
            }

            return new QueryResult(queryCompiler, keys);
        }
    }
}
