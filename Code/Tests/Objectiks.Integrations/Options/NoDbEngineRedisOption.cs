using Objectiks.Caching.Serializer;
using Objectiks.Engine;
using Objectiks.Models;
using Objectiks.StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Integrations.Options
{
    public class NoDbEngineRedisOption : DocumentOption
    {
        public NoDbEngineRedisOption() : base()
        {
            var pages = new DocumentSchema
            {
                TypeOf = "Pages",
                ParseOf = "Document",
                PrimaryOf = "Id",
                WorkOf = "AccountRef",
                UserOf = "UserRef",
                KeyOf = new DocumentKeyOfNames("Tag")
            };

            var tags = new DocumentSchema
            {
                TypeOf = "Tags",
                ParseOf = "Document",
                PrimaryOf = "Id",
                KeyOf = new DocumentKeyOfNames()
            };

            Name = "RedisProject";
            BufferSize = 512;
            TypeOf = new DocumentTypes("Pages", "Tags");
            Schemes = new DocumentSchemes(pages, tags);
            SupportPartialStorage = true;
            

            //var cacheConfig = new RedisConfiguration("localhost:6379");
            var cacheConfig = new RedisConfiguration
            {
                Hosts = new RedisHost[] { new RedisHost() },
                ConnectionTimeout = 20000,
                SyncTimeout = 20000,
                AllowAdmin = true
            };

            var cacheProvider = new RedisDocumentCache(Name, cacheConfig, new DocumentJsonSerializer());
            cacheProvider.Flush();

            UseCacheProvider(cacheProvider);
            UseDocumentWatcher<DocumentWatcher>();
            UseEngineProvider<DocumentEngine>();

            RegisterDefaultTypeOrParser();
        }
    }
}
