using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Objectiks.Engine;
using Objectiks.Extentions;
using Objectiks.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Objectiks.Caching
{
    public class DocumentInMemory : DocumentCache
    {
        public override DocumentManifest Manifest { get; set; }
        public override IDocumentConnection Connection { get; set; }

        private IMemoryCache Cache;
        private static CancellationTokenSource _resetCacheToken = new CancellationTokenSource();

        public DocumentInMemory(DocumentManifest manifest, IDocumentConnection connection)
            : base(manifest, connection)
        {
            Cache = new MemoryCache(new MemoryCacheOptions { });
        }

        public override void Set(Document document, int expire)
        {
            var expiration = TimeSpan.FromMinutes(expire);
            var options = new MemoryCacheEntryOptions()
                .SetPriority(CacheItemPriority.Normal)
                .SetAbsoluteExpiration(expiration);
            options.AddExpirationToken(new CancellationChangeToken(_resetCacheToken.Token));

            Cache.Set(CacheOf(document), document, options);
        }

        public override void Set(DocumentMeta meta, int expire)
        {
            var expiration = TimeSpan.FromMinutes(expire);
            var options = new MemoryCacheEntryOptions()
                .SetPriority(CacheItemPriority.Normal)
                .SetAbsoluteExpiration(expiration);
            options.AddExpirationToken(new CancellationChangeToken(_resetCacheToken.Token));

            Cache.Set(CacheOf(meta), meta, options);
        }

        public override Document Get(string typeOf, object primary)
        {
            if (Cache.TryGetValue(CacheOfDocument(typeOf, primary), out Document document))
            {
                document.Exists = true;

                return document;
            }

            return CreateNotExistEntity<Document>();
        }

        public override DocumentMeta Get(string typeOf)
        {
            if (Cache.TryGetValue(CacheOfMeta(typeOf), out DocumentMeta meta))
            {
                meta.Exists = true;

                return meta;
            }

            return CreateNotExistEntity<DocumentMeta>();
        }

        public override Document GetOrCreate(string typeOf, object primary, Func<Document> func)
        {
            Document document = Get(typeOf, primary);

            if (!document.Exists)
            {
                document = func();
            }

            return document;
        }

        public override DocumentMeta GetOrCreate(string typeOf, Func<DocumentMeta> func)
        {
            DocumentMeta meta = Get(typeOf);

            if (!meta.Exists)
            {
                meta = func();
            }

            return meta;
        }

        public override void Remove(string typeOf, object primary)
        {
            Cache.Remove(CacheOfDocument(typeOf, primary));
        }

        public override void Remove(string typeOf)
        {
            Cache.Remove(CacheOfMeta(typeOf));
        }

        public override void Remove(Document document)
        {
            Cache.Remove(CacheOf(document));
        }

        public override void Remove(DocumentMeta meta)
        {
            Cache.Remove(CacheOf(meta));
        }

        public override void Reset()
        {
            if (_resetCacheToken != null && !_resetCacheToken.IsCancellationRequested && _resetCacheToken.Token.CanBeCanceled)
            {
                _resetCacheToken.Cancel();
                _resetCacheToken.Dispose();
            }

            _resetCacheToken = new CancellationTokenSource();
        }
    }
}
