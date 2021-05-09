using Objectiks.Caching;
using Objectiks.Caching.Serializer;
using Objectiks.Integrations.Models;
using Objectiks.Models;
using Objectiks.PostgreSql;
using Objectiks.Redis;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace Objectiks.Integrations.Options
{
    public class PostgreEngineRedisOption : PostgreDocumentOption
    {
        public PostgreEngineRedisOption() : base()
        {
            Name = "PostgreSql";

            RegisterTypeOf<Pages>();
            RegisterTypeOf<Tags>();

            UseEngineProvider<PostgreEngine>();

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
        }
    }
}
