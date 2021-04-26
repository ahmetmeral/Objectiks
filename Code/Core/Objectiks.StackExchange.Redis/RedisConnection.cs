using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.StackExchange.Redis
{
    public class RedisConnection
    {
        private readonly RedisConfiguration Configuration;
        private readonly ConcurrentBag<Lazy<RedisConnectionState>> States;

        public RedisConnection(RedisConfiguration redisConfiguration)
        {
            Configuration = redisConfiguration;
            States = new ConcurrentBag<Lazy<RedisConnectionState>>();
        }

        public IConnectionMultiplexer GetConnection()
        {
            return ConnectionMultiplexer.Connect(Configuration.GetConfiguration());
        }
    }

    public class RedisConnectionState : IDisposable
    {
        public IConnectionMultiplexer Connection { get; set; }


        public RedisConnectionState(IConnectionMultiplexer multiplexer)
        {
            Connection = multiplexer ?? throw new ArgumentNullException(nameof(multiplexer));
            Connection.ConnectionFailed += ConnectionFailed;
            Connection.ConnectionRestored += ConnectionRestored;
        }

        public bool IsConnected() => !Connection.IsConnecting;

        private void ConnectionFailed(object sender, ConnectionFailedEventArgs e)
        {

        }

        private void ConnectionRestored(object sender, ConnectionFailedEventArgs e)
        {

        }

        public void Dispose()
        {
            Connection.ConnectionFailed -= ConnectionFailed;
            Connection.ConnectionRestored -= ConnectionRestored;
            Connection.Dispose();
        }
    }
}
