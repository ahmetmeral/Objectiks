using NUnit.Framework;
using Objectiks.Caching.Serializer;
using Objectiks.Engine;
using Objectiks.Integrations.Models;
using Objectiks.StackExchange.Redis;


using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Integrations
{
    public class RedisCacheCore
    {
        [SetUp]
        public void Setup() { }

        [Test]
        public void RedisClientTest()
        {
            var page = new Pages { Id = 2 };

            var client = new RedisCacheClient("localhost:6379", new DocumentJsonSerializer());
            var database = client.Db0;

            database.Set("pages", page);

            var pageRead = database.Get<Pages>("pages");

            //ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost:6379");

            //var db = redis.GetDatabase(0);
            //db.SetAdd("pages_2", DocumentSerializer.ToBson(page));

            //var configuration = new RedisConfiguration
            //{
            //    PoolSize = 5,
            //    Hosts = new RedisHost[]
            //    {
            //        new RedisHost { Host = "localhost", Port = 6379 }
            //    }
            //};

            //var client = new RedisClient(configuration);
            //client.Set("page_1", new Pages { Id = 1 });
        }
    }
}
