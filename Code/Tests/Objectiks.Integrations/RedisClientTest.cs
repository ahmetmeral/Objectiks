using NUnit.Framework;
using Objectiks.Caching.Serializer;
using Objectiks.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Objectiks.Integrations
{
    public class RedisClientTest
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void RedisClientParallelTest()
        {
            var size = 500;
            var pages = TestSetup.GeneratePages(size, true);
            var client = new RedisClient("localhost:6379", new DocumentJsonSerializer());

            var result = Parallel.ForEach(pages, page =>
            {
                var database = client.GetDatabase(1);
                database.Set($"RestTest:{page.Id}", page);
            });
        }

        [Test]
        public void RedisConnectionReset()
        {
            var size = 100;
            var pages = TestSetup.GeneratePages(size, true);
            var client = new RedisClient("localhost:6379", new DocumentJsonSerializer());

            //while (!client.IsConnected)
            //{
            //    Thread.Sleep(1000);
            //}

            //var is_connected = client.IsConnected;

            foreach (var item in pages)
            {
                try
                {
                    var database = client.GetDatabase(0);
                    database.Set(item.Id.ToString(), item);
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}
