using Objectiks.Caching.Serializer;
using Objectiks.Engine;
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

            Name = "NoDbEngineRedisOption";
            TypeOf = new DocumentTypes("Pages", "Tags");
            Schemes = new DocumentSchemes(pages, tags);
            
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
