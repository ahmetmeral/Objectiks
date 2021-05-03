using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Redis
{
    public class RedisConnectionPool : IDisposable
    {
        public IConnectionMultiplexer Connection { get; set; }


        public RedisConnectionPool(IConnectionMultiplexer multiplexer)
        {
            Connection = multiplexer ?? throw new ArgumentNullException(nameof(multiplexer));
            Connection.ConnectionFailed += ConnectionFailed;
            Connection.ConnectionRestored += ConnectionRestored;
        }

        public bool IsConnected() => !Connection.IsConnecting;

        public long TotalOutstanding() => this.Connection.GetCounters().TotalOutstanding;

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
