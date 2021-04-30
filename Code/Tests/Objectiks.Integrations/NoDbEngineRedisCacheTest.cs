using NUnit.Framework;
using Objectiks.Caching.Serializer;
using Objectiks.Engine;
using Objectiks.Integrations.Models;
using Objectiks.Integrations.Options;
using Objectiks.StackExchange.Redis;


using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Objectiks.Integrations
{
    public class NoDbEngineRedisCacheTest : DocumentEngineTestBase
    {
        [SetUp]
        public override void Setup()
        {
            ObjectiksOf.Core.Map(typeof(DocumentProvider), new NoDbEngineRedisOption());
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
    }
}
