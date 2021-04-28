using Objectiks.Caching.Serializer;
using Objectiks.Models;
using Objectiks.StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Integrations.Options
{
    public class RedisCacheProviderOption : DocumentOption
    {
        public RedisCacheProviderOption() : base()
        {
            var page = new DocumentSchema
            {
                TypeOf = "Pages",
                ParseOf = "Document",
                KeyOf = new DocumentKeyOfNames("Name"),
                Primary = "Id"
            };

            var category = new DocumentSchema
            {
                TypeOf = "Categories",
                ParseOf = "Document",
                KeyOf = new DocumentKeyOfNames(),
                Primary = "Id"
            };

            Name = "RedisProject";
            BufferSize = 512;
            TypeOf = new DocumentTypes("Pages", "Categories");
            Schemes = new DocumentSchemes(page, category);
            SupportLoaderInRefs = false;
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

            UseCacheProvider(cacheProvider);

            RegisterDefaultTypeOrParser();
        }
    }
}
