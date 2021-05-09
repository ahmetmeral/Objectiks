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
    public abstract partial class DocumentEngine : IDocumentEngine
    {

        public abstract DocumentInfo GetTypeOfDocumentInfo(string typeOf, object primaryOf);
        public abstract DocumentInfo GetTypeOfDocumentInfo(string typeOf, object primaryOf, Type primaryOfDataType);


        public abstract T Read<T>(DocumentQuery query, DocumentMeta meta = null);
        public abstract List<T> ReadList<T>(DocumentQuery query);
        public abstract T GetCount<T>(DocumentQuery query, DocumentMeta meta = null);


        public virtual List<DocumentMeta> GetTypeMetaAll()
        {
            var list = new List<DocumentMeta>();

            foreach (var type in Option.TypeOf)
            {
                var meta = GetTypeMeta(type.TypeOf);

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


    }
}
