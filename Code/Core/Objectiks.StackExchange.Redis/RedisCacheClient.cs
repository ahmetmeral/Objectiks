using Objectiks.Caching.Serializer;
using Objectiks.Services;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;


namespace Objectiks.StackExchange.Redis
{
    public class RedisCacheClient
    {
        private readonly RedisConnection Connection;
        private readonly IDocumentSerializer Serializer;

        private RedisConfiguration Configuration;

        public RedisDatabase Db0 => GetDatabase(0);

        public RedisCacheClient(string connectionString, IDocumentSerializer serializer = null)
            : this(new RedisConfiguration(connectionString), serializer)
        {

        }

        public RedisCacheClient(RedisConfiguration configuration, IDocumentSerializer serializer = null)
        {
            Configuration = configuration;
            Serializer = serializer ?? new DocumentBsonSerializer();
            Connection = new RedisConnection(configuration);
        }

        public RedisDatabase GetDatabase(int databaseNumber)
        {
            return new RedisDatabase(Connection, Serializer, databaseNumber);
        }
    }
}
