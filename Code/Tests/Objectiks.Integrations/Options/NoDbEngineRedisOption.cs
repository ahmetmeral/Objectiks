using Objectiks.Caching.Serializer;
using Objectiks.Engine;
using Objectiks.Integrations.Models;
using Objectiks.Models;
using Objectiks.NoDb;
using Objectiks.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Integrations.Options
{
    public class NoDbEngineRedisOption : NoDbDocumentOption
    {
        public NoDbEngineRedisOption() : base()
        {
            Name = "NoDbEngineRedisOption";

            RegisterTypeOf<Pages>();
            RegisterTypeOf<Tags>();

            var cacheConfig = new RedisConfiguration
            {
                Hosts = new RedisHost[] { new RedisHost() },
                ConnectionTimeout = 20000,
                SyncTimeout = 20000,
                AllowAdmin = true
            };

            UseCacheProvider(
                new RedisDocumentCache(
                    Name,
                    cacheConfig, 
                    new DocumentJsonSerializer(), 
                    true
                  )
                );

            UseDocumentWatcher<DocumentWatcher>();
        }
    }
}
