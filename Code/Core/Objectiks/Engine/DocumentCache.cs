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
        public abstract void Remove(DocumentQuery query);
        public abstract void Flush();
        public abstract void Set(Document document, int expire);
        public abstract void Set(DocumentMeta meta, int expire);
        public abstract void Set(DocumentSequence sequence);
        public abstract DocumentSequence GetSequence(string typeOf);
        public abstract void Set(DocumentInfo info);
        public abstract DocumentInfo GetDocumentInfo(string typeOf, object primaryOf);

        public abstract void Set<T>(DocumentQuery query, T data);
        public abstract T Get<T>(DocumentQuery query);

        public virtual string CacheOf(DocumentQuery query)
        {
            return $"objectiks:{Bucket}:{DocumentDefaults.CacheOf}:{query.TypeOf}:{query.GetCacheOfKey()}".ToLowerInvariant();
        }

        public virtual string CacheOf(Document doc)
        {
            return CacheOfDoc(doc.TypeOf, doc.PrimaryOf);
        }

        public virtual string CacheOfDoc(string typeOf, object primaryOf)
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
            return CacheOfDocInfo(info.TypeOf, info.PrimaryOf);
        }

        public virtual string CacheOfDocInfo(string typeOf, object primaryOf)
        {
            return $"objectiks:{Bucket}:{DocumentDefaults.Info}:{typeOf}:{primaryOf.ToString()}".ToLowerInvariant();
        }

        protected virtual T CreateNotExistEntity<T>()
        {
            return (T)Activator.CreateInstance(typeof(T));
        }


    }
}
