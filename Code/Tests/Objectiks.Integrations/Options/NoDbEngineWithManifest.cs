using Objectiks.Caching;
using Objectiks.Caching.Serializer;
using Objectiks.Engine;
using Objectiks.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Integrations.Options
{
    public class NoDbEngineWithManifest : DocumentOption
    {
        public NoDbEngineWithManifest() : base()
        {
            Name = "NoDbEngineWithManifest";

            var cacheConfig = new RedisConfiguration
            {
                Hosts = new RedisHost[] { new RedisHost() },
                ConnectionTimeout = 20000,
                SyncTimeout = 20000,
                AllowAdmin = true
            };

            UseManifestFile();
            UseCacheProvider(new RedisDocumentCache(Name, cacheConfig, new DocumentJsonSerializer(), false));
            UseDocumentWatcher<DocumentWatcher>();

            RegisterDefaultTypeOrParser();
        }
    }
}
