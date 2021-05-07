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
using Objectiks.NoDb.Extentions;

namespace Objectiks.NoDb
{
    public partial class NoDbEngine : DocumentEngine
    {
        public override Document Read(string typeOf, object primaryOf)
        {
            var document = Cache.GetOrCreateDocument(typeOf, primaryOf, () =>
            {
                LoadDocumentType(typeOf);

                return Cache.Get(typeOf, primaryOf);
            });

            return document;
        }

        public override DocumentInfo GetTypeOfDocumentInfo(string typeOf, object primaryOf, Type primaryOfDataType)
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

        public override Document Read(DocumentQuery query, DocumentMeta meta = null)
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
            }

            return document;
        }

        public override T Read<T>(DocumentQuery query, DocumentMeta meta = null)
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

        public override List<T> ReadList<T>(DocumentQuery query)
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

                results.Add(((JObject)document.Data).ToObject<T>());
            }

            if (query.HasOrderBy && results.Count > 1)
            {
                return results.AsQueryable().OrderBy(queryResult.Query.OrderBy).ToList();
            }

            SetAnyCacheOfDocument(query, results);

            return results;
        }

        public override T GetCount<T>(DocumentQuery query, DocumentMeta meta = null)
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

            if (query.HasParameters)
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

        public override List<DocumentMeta> GetTypeMetaAll()
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

        public override DocumentMeta GetTypeMeta(string typeOf)
        {
            var meta = Cache.GetOrCreateMeta(typeOf, () =>
            {
                LoadDocumentType(typeOf);

                return Cache.Get(typeOf);
            });

            return meta;
        }

        public override T ReadAnyCacheOfFromQuery<T>(DocumentQuery query)
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

        public override void RemoveAnyCacheOfFromQuery(DocumentQuery query)
        {
            if (query.CacheOf.BeforeCallClear)
            {
                Cache.Remove(query);
            }
        }

        public override void SetAnyCacheOfDocument<T>(DocumentQuery query, T data)
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

        public override T GetCountFromQueryOf<T>(DocumentQuery query, DocumentMeta meta = null)
        {
            if (meta == null)
            {
                meta = GetTypeMeta(query.TypeOf);
            }

            var compiler = query.Compiler();
            return meta.Keys.AsQueryable().Count(compiler.WhereBy,
                     compiler.ValueBy).ChangeType<T>();
        }

        public override QueryResult GetDocumentKeysFromQueryOf(DocumentQuery query, DocumentMeta meta = null)
        {
            if (meta == null)
            {
                meta = GetTypeMeta(query.TypeOf);
            }

            List<DocumentKey> keys;

            var queryCompiler = query.Compiler();

            if (query.HasParameters)
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
