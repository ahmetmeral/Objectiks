using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Objectiks.Redis
{
    public class RedisConnection
    {
        private readonly RedisConfiguration Configuration;
        private readonly ConcurrentBag<Lazy<RedisConnectionPool>> Pool;

        public RedisConnection(RedisConfiguration redisConfiguration)
        {
            Configuration = redisConfiguration;
            Pool = new ConcurrentBag<Lazy<RedisConnectionPool>>();

            EmitConnections();
        }

        public IConnectionMultiplexer GetConnection()
        {
            var isValueCreatedCount = Pool.Count(lazy => lazy.IsValueCreated);

            if (isValueCreatedCount == this.Pool.Count)
            {
                return Pool.OrderBy(x => x.Value.TotalOutstanding()).First().Value.Connection;
            }

            foreach (var item in Pool)
            {
                if (!item.IsValueCreated)
                {
                    return item.Value.Connection;
                }

                if (item.Value.TotalOutstanding() == 0)
                {
                    return item.Value.Connection;
                }
            }

            return this.Pool.First().Value.Connection;
        }

        private void EmitConnection()
        {
            Pool.Add(new Lazy<RedisConnectionPool>(() =>
            {
                var multiplexer = ConnectionMultiplexer.Connect(Configuration.GetOptions());

                return new RedisConnectionPool(multiplexer);
            }));
        }

        private void EmitConnections()
        {
            if (Pool.Count >= Configuration.PoolSize)
            {
                return;
            }

            for (int i = 0; i < Configuration.PoolSize; i++)
            {
                EmitConnection();
            }
        }
    }
}
