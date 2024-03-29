﻿using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Objectiks.Engine;
using Objectiks.Extentions;
using Objectiks.Models;
using Objectiks.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Objectiks.Caching
{
    //todo: if expire = -1 - NeverRemove 
    public class DocumentInMemory : DocumentCache
    {
        private IMemoryCache Cache;
        private static CancellationTokenSource _resetCacheToken = new CancellationTokenSource();

        public DocumentInMemory(string bucket, IDocumentSerializer serializer, long? memorySize = null)
            : base(bucket, serializer)
        {
            var options = new MemoryCacheOptions();

            if (memorySize.HasValue)
            {
                options.SizeLimit = memorySize;
            }

            Cache = new MemoryCache(options);
        }

        public override void Set(Document document, int expire)
        {
            var expiration = TimeSpan.FromMinutes(expire);
            var options = new MemoryCacheEntryOptions()
                .SetPriority(CacheItemPriority.Normal)
                .SetAbsoluteExpiration(expiration);
            options.AddExpirationToken(new CancellationChangeToken(_resetCacheToken.Token));

            Cache.Set(CacheOf(document), Serializer.Serialize(document), options);

            Set(new DocumentInfo(document));
        }

        public override void Set(DocumentMeta meta, int expire)
        {
            var expiration = TimeSpan.FromMinutes(expire);
            var options = new MemoryCacheEntryOptions()
                .SetPriority(CacheItemPriority.Normal)
                .SetAbsoluteExpiration(expiration);
            options.AddExpirationToken(new CancellationChangeToken(_resetCacheToken.Token));

            Cache.Set(CacheOf(meta), Serializer.Serialize(meta), options);
        }

        public override void Set(DocumentSequence sequence)
        {
            var options = new MemoryCacheEntryOptions()
                .SetPriority(CacheItemPriority.NeverRemove);
            options.AddExpirationToken(new CancellationChangeToken(_resetCacheToken.Token));

            Cache.Set(CacheOf(sequence), Serializer.Serialize(sequence), options);
        }

        public override void Set(DocumentInfo info)
        {
            var options = new MemoryCacheEntryOptions()
                .SetPriority(CacheItemPriority.NeverRemove);
            options.AddExpirationToken(new CancellationChangeToken(_resetCacheToken.Token));

            Cache.Set(CacheOf(info), Serializer.Serialize(info), options);
        }

        public override void Set<T>(DocumentQuery query, T data)
        {
            var expiration = TimeSpan.FromMinutes(query.CacheOf.Expire);
            var options = new MemoryCacheEntryOptions()
                .SetPriority(CacheItemPriority.Normal)
                .SetAbsoluteExpiration(expiration);
            options.AddExpirationToken(new CancellationChangeToken(_resetCacheToken.Token));

            Cache.Set(CacheOf(query), data, options);
        }

        public override T Get<T>(DocumentQuery query)
        {
            if (Cache.TryGetValue(CacheOf(query), out byte[] data))
            {
                return Serializer.Deserialize<T>(data);
            }

            return default;
        }

        public override DocumentInfo GetDocumentInfo(string typeOf, object primary)
        {
            if (Cache.TryGetValue(CacheOfDocInfo(typeOf, primary), out byte[] data))
            {
                var info = Serializer.Deserialize<DocumentInfo>(data);

                info.Exists = true;

                return info;
            }

            return CreateNotExistEntity<DocumentInfo>();
        }

        public override Document Get(string typeOf, object primary)
        {
            if (Cache.TryGetValue(CacheOfDoc(typeOf, primary), out byte[] data))
            {
                var document = Serializer.Deserialize<Document>(data);

                document.Exists = true;

                return document;
            }

            return CreateNotExistEntity<Document>();
        }

        public override DocumentMeta Get(string typeOf)
        {
            if (Cache.TryGetValue(CacheOfMeta(typeOf), out byte[] data))
            {
                DocumentMeta meta = Serializer.Deserialize<DocumentMeta>(data);

                meta.Exists = true;

                return meta;
            }

            return CreateNotExistEntity<DocumentMeta>();
        }

        public override DocumentSequence GetSequence(string typeOf)
        {
            if (Cache.TryGetValue(CacheOfSequence(typeOf), out byte[] data))
            {
                DocumentSequence sequence = Serializer.Deserialize<DocumentSequence>(data);

                sequence.Exists = true;

                return sequence;
            }

            return CreateNotExistEntity<DocumentSequence>();
        }

        public override Document GetOrCreateDocument(string typeOf, object primary, Func<Document> func)
        {
            Document document = Get(typeOf, primary);

            if (!document.Exists)
            {
                document = func();
            }

            return document;
        }

        public override DocumentMeta GetOrCreateMeta(string typeOf, Func<DocumentMeta> func)
        {
            DocumentMeta meta = Get(typeOf);

            if (!meta.Exists)
            {
                meta = func();
            }

            return meta;
        }

        public override DocumentSequence GetOrCreateSequence(string typeOf, Func<DocumentSequence> func)
        {
            DocumentSequence sequence = GetSequence(typeOf);

            if (!sequence.Exists)
            {
                sequence = func();
            }

            return sequence;
        }

        public override void Remove(string typeOf, object primary)
        {
            Cache.Remove(CacheOfDoc(typeOf, primary));
            Cache.Remove(CacheOfDocInfo(typeOf, primary));
        }

        public override void Remove(string typeOf)
        {
            Cache.Remove(CacheOfMeta(typeOf));
        }

        public override void Remove(DocumentQuery query)
        {
            Cache.Remove(CacheOf(query));
        }

        public override void Remove(Document document)
        {
            Cache.Remove(CacheOf(document));
            Cache.Remove(CacheOfDocInfo(document.TypeOf, document.PrimaryOf));
        }

        public override void Remove(DocumentMeta meta)
        {
            Cache.Remove(CacheOf(meta));
        }

        public override void Flush()
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
