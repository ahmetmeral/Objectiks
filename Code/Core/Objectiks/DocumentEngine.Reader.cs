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
                    if (meta.HasRefs)
                    {
                        //default refs
                        ParseDocumentRefs(meta.GetRefs(true), ref document);
                    }

                    //check dynamic refs..
                    if (query.HasRefs)
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
                    if (meta.HasRefs)
                    {
                        ParseDocumentRefs(meta.GetRefs(true), ref document);
                    }

                    //check dynamic refs..
                    if (query.HasRefs)
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

        public virtual Document Read(QueryOf query, DocumentMeta meta = null)
        {
            Document document = null;

            if (meta == null)
            {
                meta = GetTypeMeta(query.TypeOf);
            }

            if (query.HasPrimaryOf)
            {
                document = Read(query.TypeOf, query.PrimaryOfList[0]);

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
                        ParseDocumentRefs(query.RefList, ref document);
                    }

                    return document;
                }
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

        public virtual List<T> ReadList<T>(QueryOf query)
        {
            var cacheOfData = ReadAnyCacheOfFromQuery<List<T>>(query);

            if (cacheOfData != null)
            {
                return cacheOfData;
            }

            var results = new List<T>();
            //read document meta data..
            var meta = GetTypeMeta(query.TypeOf);

            if (query.HasPrimaryOf && !query.HasKeyOf)
            {
                foreach (var primaryOf in query.PrimaryOfList)
                {
                    var document = Read(query.TypeOf, primaryOf);

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
                        ParseDocumentRefs(query.RefList, ref document);
                    }

                    results.Add(((JObject)document.Data).ToObject<T>());
                }
            }

            if (query.HasOrderBy)
            {
                return results.AsQueryable().OrderBy(query.AsOrderBy()).ToList();
            }

            SetAnyCacheOfDocument(query, results);

            return results;
        }

        public virtual T GetCount<T>(QueryOf query, DocumentMeta meta = null)
        {
            T result = ReadAnyCacheOfFromQuery<T>(query);

            if (result != null)
            {
                return result;
            }

            if (meta == null)
            {
                meta = GetTypeMeta(query.TypeOf);
            }

            if (query.HasPrimaryOf || query.HasKeyOf)
            {
                result = meta.GetCountFromQueryOf<T>(query);
            }
            else
            {
                result = meta.TotalRecords.ChangeType<T>();
            }

            SetAnyCacheOfDocument<T>(query, result);

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

        public virtual T ReadAnyCacheOfFromQuery<T>(string typeOf, string cacheOfKey)
        {
            return Cache.GetCacheOf<T>(typeOf, cacheOfKey);
        }

        public virtual T ReadAnyCacheOfFromQuery<T>(QueryOf query)
        {
            if (query == null)
            {
                return default;
            }

            if (query.IsCacheOf)
            {
                return Cache.GetCacheOf<T>(query.TypeOf, query.GetCacheOfKey());
            }
            return default;
        }

        public virtual void SetAnyCacheOfDocument<T>(QueryOf query, T data)
        {
            if (query == null)
            {
                return;
            }

            if (query.IsCacheOf)
            {
                Cache.SetCacheOf<T>(query.TypeOf, query.GetCacheOfKey(), data, query.CacheOfExpire);
            }
        }
    }
}
