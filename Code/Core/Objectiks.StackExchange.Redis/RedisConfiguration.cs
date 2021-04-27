using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.StackExchange.Redis
{
    public class RedisConfiguration
    {
        private ConfigurationOptions Options;
        public string ConnectionString { get; set; }
        public int Database { get; set; } = 0;
        public int PoolSize { get; set; } = 5;
        public int ConnectionTimeout { get; set; } = 5000;
        public int SyncTimeout { get; set; } = 5000;
        public bool AllowAdmin { get; set; } = false;
        public RedisHost[] Hosts { get; set; }


        public RedisConfiguration() { }

        public RedisConfiguration(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public ConfigurationOptions GetOptions()
        {
            if (!String.IsNullOrEmpty(ConnectionString))
            {
                Options = ConfigurationOptions.Parse(ConnectionString);
            }
            else
            {
                Options = new ConfigurationOptions
                {
                    ConnectTimeout = ConnectionTimeout,
                    SyncTimeout = SyncTimeout,
                    AllowAdmin = AllowAdmin
                };

                foreach (var redis in Hosts)
                {
                    Options.EndPoints.Add(redis.Host, redis.Port);
                }
            }

            return Options;
        }
    }
}
