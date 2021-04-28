﻿using NUnit.Framework;
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
    public class RedisTest
    {
        [SetUp]
        public void Setup()
        {
            ObjectiksOf.Core.Map(typeof(DocumentProvider), new RedisCacheProviderOption());
        }

        [Test]
        public void RedisClientTest()
        {
            var size = 500;
            var pages = TestSetup.GeneratePages(size, true);
            var client = new RedisClient("localhost:6379", new DocumentJsonSerializer());

            var result = Parallel.ForEach(pages, page =>
            {
                var database = client.GetDatabase(0);
                database.Set($"Page:{page.Id}", page);
            });
        }

        [Test]
        public void WriterTest()
        {
            var size = 2;
            var pages = TestSetup.GeneratePages(size, false);
            var repos = new ObjectiksOf();

            var meta_before = repos.GetTypeMeta<Pages>();

            using (var writer = repos.WriterOf<Pages>())
            {
                //writer.UsePartialStore(1);
                writer.UseFormatting();

                foreach (var page in pages)
                {
                    writer.AddDocument(page);
                }

                writer.SubmitChanges();
            }

            var meta_after = repos.GetTypeMeta<Pages>();

            //repos.Flush();
        }

        [Test]
        public void DocumentTransactionParalelTest()
        {
            var size = 20;
            var pages = TestSetup.GeneratePages(size, false);
            var repos = new ObjectiksOf();
            var meta_before = repos.GetTypeMeta<Pages>();
            var count = meta_before.TotalRecords;

            var result = Parallel.ForEach(pages, page =>
            {
                using (var trans = repos.BeginTransaction())
                {
                    try
                    {
                        //var category = new Categories
                        //{
                        //    Name = "Transactions",
                        //    Description = "test",
                        //    Title = "Test"
                        //};

                        //using (var categoryWriter = repos.WriterOf<Categories>(trans))
                        //{
                        //    categoryWriter.UseFormatting();
                        //    categoryWriter.AddDocument(category);
                        //    categoryWriter.SubmitChanges();
                        //}

                        using (var pageWriter = repos.WriterOf<Pages>())
                        {
                            pageWriter.UseFormatting();
                            pageWriter.AddDocument(page);
                            pageWriter.SubmitChanges();
                        }

                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback(ex);
                    }
                }
            });


            while (!result.IsCompleted)
            {

            }

            var meta_after = repos.GetTypeMeta<Pages>();

        }
    }
}