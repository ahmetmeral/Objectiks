using Objectiks.Caching;
using Objectiks.Caching.Serializer;
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
            #region TypeOf
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
            #endregion

            TypeOf = new DocumentTypes("Pages", "Tags");
            Schemes = new DocumentSchemes(pages, tags);
       
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
