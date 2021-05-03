using Objectiks.Engine;
using Objectiks.Models;
using Objectiks.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Objectiks.Redis
{
    public class RedisDocumentCache : DocumentCache
    {
        private readonly RedisClient Client;
        private readonly RedisConfiguration Configuration;

        private RedisDatabase Database
        {
            get
            {
                return Client.GetDatabase(Configuration.Database);
            }
        }

        public RedisDocumentCache(string bucket, RedisConfiguration configuration, IDocumentSerializer serializer, bool flush = false) : base(bucket, serializer)
        {
            Configuration = configuration;
            Client = new RedisClient(configuration, serializer);

            if (flush)
            {
                Flush();
            }
        }

        public override void Set(Document document, int expire)
        {
            Database.Set(CacheOf(document), document, expire);

            var info = new DocumentInfo(document);

            Database.Set(CacheOf(info), info);
        }

        public override void Set(DocumentMeta meta, int expire)
        {
            Database.Set(CacheOf(meta), meta, expire);
        }

        public override void Set(DocumentSequence sequence)
        {
            Database.Set(CacheOf(sequence), sequence);
        }

        public override void Set(DocumentInfo info)
        {
            Database.Set(CacheOf(info), info);
        }


        public override Document Get(string typeOf, object primaryOf)
        {
            var document = Database.Get<Document>(CacheOfDocument(typeOf, primaryOf));

            if (document != null)
            {
                document.Exists = true;

                return document;
            }

            return CreateNotExistEntity<Document>();
        }

        public override DocumentMeta Get(string typeOf)
        {
            var meta = Database.Get<DocumentMeta>(CacheOfMeta(typeOf));

            if (meta != null)
            {
                meta.Exists = true;

                return meta;
            }

            return CreateNotExistEntity<DocumentMeta>();
        }

        public override DocumentInfo GetDocumentInfo(string typeOf, object primaryOf)
        {
            var info = Database.Get<DocumentInfo>(CacheOfDocumentInfo(typeOf, primaryOf));

            if (info != null)
            {
                info.Exists = true;

                return info;
            }

            return CreateNotExistEntity<DocumentInfo>();
        }

        public override Document GetOrCreateDocument(string typeOf, object primaryOf, Func<Document> func)
        {
            Document document = Get(typeOf, primaryOf);

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

        public override DocumentSequence GetSequence(string typeOf)
        {
            return Database.Get<DocumentSequence>(CacheOfSequence(typeOf));
        }

        public override void Remove(string typeOf, object primaryOf)
        {
            Database.Remove(CacheOfDocument(typeOf, primaryOf));
            Database.Remove(CacheOfDocumentInfo(typeOf, primaryOf));
        }

        public override void Remove(string typeOf)
        {
            Database.Remove(typeOf);
        }

        public override void Remove(Document document)
        {
            Database.Remove(CacheOf(document));
            Database.Remove(CacheOfDocumentInfo(document.TypeOf, document.PrimaryOf));
        }

        public override void Remove(DocumentMeta meta)
        {
            Database.Remove(CacheOf(meta));
        }

        public override void Flush()
        {
            var endPoints = Database.Database.Multiplexer.GetEndPoints();

            var tasks = new List<Task>(endPoints.Length);

            for (var i = 0; i < endPoints.Length; i++)
            {
                var server = Database.Database.Multiplexer.GetServer(endPoints[i]);

                if (!server.IsReplica)
                    tasks.Add(server.FlushDatabaseAsync(Database.Number));
            }

            Task.WhenAll(tasks);
        }
    }
}
