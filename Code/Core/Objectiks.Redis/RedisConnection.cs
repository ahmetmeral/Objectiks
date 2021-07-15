using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Objectiks.Redis
{
    public class RedisConnection : IDisposable
    {
        private readonly RedisConfiguration Configuration;
        private readonly ConcurrentBag<Lazy<RedisConnectionPool>> Pool;

        public bool IsConnected { get; private set; }

        public RedisConnection(RedisConfiguration redisConfiguration)
        {
            Configuration = redisConfiguration;
            Pool = new ConcurrentBag<Lazy<RedisConnectionPool>>();
            EmitConnections();
        }

        public IConnectionMultiplexer GetConnection()
        {
            try
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
            catch
            {
                return null;
            }
        }

        public RedisConnectionInformation GetInformations()
        {
            var poolSize = 0;
            var active = 0;
            var invalid = 0;

            foreach (var pool in Pool)
            {
                poolSize++;

                if (!pool.Value.IsConnected())
                {
                    invalid++;
                    continue;
                }

                active++;
            }

            return new RedisConnectionInformation()
            {
                PoolSize = poolSize,
                Active = active,
                Invalid = invalid
            };
        }

        private void EmitConnection()
        {
            try
            {
                Pool.Add(new Lazy<RedisConnectionPool>(() =>
                {
                    try
                    {
                        var multiplexer = ConnectionMultiplexer.Connect(Configuration.GetOptions());

                        return new RedisConnectionPool(multiplexer);
                    }
                    catch
                    {
                        return null;
                    }
                }));
            }
            catch
            {

            }
        }

        private void EmitConnections()
        {
            try
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
            finally
            {
                TestConnections();
            }
        }

        internal bool TestConnections()
        {
            var poolCount = this.Pool.Count;
            var isValueCreatedCount = Pool.Count(lazy => lazy.IsValueCreated);

            while (poolCount != isValueCreatedCount)
            {
                var connection = GetConnection();

                if (connection == null)
                {
                    break;
                }

                if (connection.IsConnected)
                {
                    IsConnected = true;
                    break;
                }
            }

            return IsConnected;
        }

        public void Reset()
        {
            Pool.Clear();
        }

        public void Dispose()
        {
            Reset();
            GC.SuppressFinalize(Pool);
        }
    }
}
