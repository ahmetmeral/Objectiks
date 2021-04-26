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


        public RedisConfiguration() { }

        public RedisConfiguration(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public ConfigurationOptions GetConfiguration()
        {
            Options = ConfigurationOptions.Parse(ConnectionString);

            return Options;
        }
    }
}
