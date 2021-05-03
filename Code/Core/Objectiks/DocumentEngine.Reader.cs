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
    }
}
