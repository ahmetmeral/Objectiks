using Objectiks.Helper;
using Objectiks.Models;
using Objectiks.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Engine
{
    public abstract class DocumentCache : IDocumentCache
    {
        public abstract DocumentManifest Manifest { get; set; }
        public string Hash { get; set; }

        public DocumentCache(DocumentManifest manifest)
        {
            Manifest = manifest;
            Hash = HashHelper.CreateMD5(manifest.Name);
        }

        public abstract Document Get(string typeOf, object primaryOf);
        public abstract DocumentMeta Get(string typeOf);
        public abstract Document GetOrCreate(string typeOf, object primaryOf, Func<Document> func);
        public abstract DocumentMeta GetOrCreate(string typeOf, Func<DocumentMeta> func);
        public abstract DocumentTypeStatus GetStatus(string typeOf);
        public abstract void Remove(string typeOf, object primaryOf);
        public abstract void Remove(string typeOf);
        public abstract void Remove(Document document);
        public abstract void Remove(DocumentMeta meta);
        public abstract void Reset();
        public abstract void Set(Document document, int expire);
        public abstract void Set(DocumentMeta meta, int expire);
        public abstract void SetStatus(DocumentTypeStatus status);


        public virtual string CacheOf(DocumentTypeStatus status)
        {
            return CacheOfStatus(status.TypeOf);
        }

        public virtual string CacheOfStatus(string typeOf)
        {
            return $"objectiks:{Hash}:{DocumentDefaults.Documents}:status:{typeOf}".ToLowerInvariant();
        }

        public virtual string CacheOf(Document doc)
        {
            return CacheOfDocument(doc.TypeOf, doc.Primary);
        }

        public virtual string CacheOfDocument(string typeOf, object primaryOf)
        {
            return $"objectiks:{Hash}:{DocumentDefaults.Documents}:{typeOf}:{primaryOf}".ToLowerInvariant();
        }

        public virtual string CacheOf(DocumentMeta meta)
        {
            return CacheOfMeta(meta.TypeOf);
        }

        public virtual string CacheOfMeta(string typeOf)
        {
            return $"objectiks:{Hash}:{DocumentDefaults.Meta}:{typeOf}".ToLowerInvariant();
        }

        protected virtual T CreateNotExistEntity<T>() where T : class
        {
            return (T)Activator.CreateInstance(typeof(T));
        }
    }
}
