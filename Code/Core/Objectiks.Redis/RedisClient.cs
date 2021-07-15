using Objectiks.Caching.Serializer;
using Objectiks.Services;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;


namespace Objectiks.Redis
{
    //references: https://github.com/imperugo/StackExchange.Redis.Extensions
    public class RedisClient : IDisposable
    {
        private readonly IDocumentSerializer Serializer;
        private RedisConfiguration Configuration;
        private RedisConnection Connection;

        public bool IsConnected
        {
            get
            {
                if (!Connection.IsConnected)
                {
                    Reset();
                }

                return Connection.IsConnected;
            }
        }

        public RedisClient(string connectionString, IDocumentSerializer serializer = null)
            : this(new RedisConfiguration(connectionString), serializer)
        {
        }

        public RedisClient(RedisConfiguration configuration, IDocumentSerializer serializer = null)
        {
            if (configuration.PoolSize == 0)
            {
                configuration.PoolSize = 5;
            }

            Configuration = configuration;
            Serializer = serializer ?? new DocumentBsonSerializer();
            Connection = new RedisConnection(configuration);
        }

        public RedisConnectionInformation GetInformation()
        {
            return Connection.GetInformations();
        }

        public void Reset()
        {
            Connection.Reset();
            Connection = new RedisConnection(Configuration);
        }

        public RedisDatabase GetDatabase(int databaseNumber)
        {
            try
            {
                if (!IsConnected)
                {
                    throw new Exception("Redis connection failed..");
                }

                return new RedisDatabase(Connection, Serializer, databaseNumber);
            }
            catch
            {
                return null;
            }
        }

        public void Dispose()
        {
            Connection.Reset();
            Connection?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
