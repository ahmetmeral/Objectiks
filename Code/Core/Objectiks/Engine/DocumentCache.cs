using Objectiks.Models;
using Objectiks.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Engine
{
    public abstract class DocumentCache : IDocumentCache
    {
        public string Bucket { get; set; }
        public IDocumentSerializer Serializer { get; set; }

        public DocumentCache(string bucket, IDocumentSerializer serializer)
        {
            Bucket = bucket;
            Serializer = serializer;
        }



        public abstract Document Get(string typeOf, object primaryOf);

        public abstract DocumentMeta Get(string typeOf);
        public abstract Document GetOrCreateDocument(string typeOf, object primaryOf, Func<Document> func);
        public abstract DocumentMeta GetOrCreateMeta(string typeOf, Func<DocumentMeta> func);
        public abstract DocumentSequence GetOrCreateSequence(string typeOf, Func<DocumentSequence> func);
        public abstract void Remove(string typeOf, object primaryOf);
        public abstract void Remove(string typeOf);
        public abstract void Remove(Document document);
        public abstract void Remove(DocumentMeta meta);
        public abstract void Flush();
        public abstract void Set(Document document, int expire);
        public abstract void Set(DocumentMeta meta, int expire);
        public abstract void Set(DocumentSequence sequence);
        public abstract DocumentSequence GetSequence(string typeOf);
        public abstract void Set(DocumentInfo info);
        public abstract DocumentInfo GetDocumentInfo(string typeOf, object primaryOf);

        public abstract void SetCacheOf<T>(string typeOf, string key, T data, int expire);
        public abstract T GetCacheOf<T>(string typeOf, string key);

        public virtual string CacheOf(string typeOf, string key)
        {
            if (String.IsNullOrEmpty(typeOf))
            {
                typeOf = "Any";
            }

            return $"objectiks:{Bucket}:{DocumentDefaults.CacheOf}:{typeOf}:{key}".ToLowerInvariant();
        }

        public virtual string CacheOf(Document doc)
        {
            return CacheOfDocument(doc.TypeOf, doc.PrimaryOf);
        }

        public virtual string CacheOfDocument(string typeOf, object primaryOf)
        {
            return $"objectiks:{Bucket}:{DocumentDefaults.Documents}:{typeOf}:{primaryOf}".ToLowerInvariant();
        }

        public virtual string CacheOf(DocumentMeta meta)
        {
            return CacheOfMeta(meta.TypeOf);
        }

        public virtual string CacheOfMeta(string typeOf)
        {
            return $"objectiks:{Bucket}:{DocumentDefaults.Meta}:{typeOf}".ToLowerInvariant();
        }

        public virtual string CacheOf(DocumentSequence sequence)
        {
            return CacheOfSequence(sequence.TypeOf);
        }

        public virtual string CacheOfSequence(string typeOf)
        {
            return $"objectiks:{Bucket}:{DocumentDefaults.Sequence}:{typeOf}".ToLowerInvariant();
        }

        public virtual string CacheOf(DocumentInfo info)
        {
            return CacheOfDocumentInfo(info.TypeOf, info.PrimaryOf);
        }

        public virtual string CacheOfDocumentInfo(string typeOf, object primaryOf)
        {
            return $"objectiks:{Bucket}:{DocumentDefaults.Info}:{typeOf}:{primaryOf.ToString()}".ToLowerInvariant();
        }

        protected virtual T CreateNotExistEntity<T>()
        {
            return (T)Activator.CreateInstance(typeof(T));
        }


    }
}
