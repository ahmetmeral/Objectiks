using Npgsql;
using NUnit.Framework;
using Objectiks.Engine;
using Objectiks.Engine.Query;
using Objectiks.Integrations.Models;
using Objectiks.Integrations.Option;
using Objectiks.Integrations.Options;
using Objectiks.Models;
using Objectiks.PostgreSql;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace Objectiks.Integrations
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            ObjectiksOf.Core.Map(typeof(DocumentProvider), new FileEngineProviderOption());
            ObjectiksOf.Core.Map(typeof(NpgsqlConnection), new PostgreEngineProviderOption());
        }


        [Test]
        public void MssqlProvider()
        {
            var connectionString = "Server=.\\SqlExpress;Database=INBOX;User Id=sa;Password=data1;";
            var conn = new SqlConnection(connectionString);
        }

        [Test]
        public void PostgreSqlProvider()
        {
            var connectionString = "Host=localhost;Port=5432;Database=objectiks;User id=postgres;Password=data1;SslMode=Disable;";
            var conn = new NpgsqlConnection(connectionString);
            var repos = new ObjectiksOf(conn);
            var meta = repos.GetTypeMeta<Pages>();
            var page = repos.TypeOf<Pages>().PrimaryOf(1).First();


            using (var writer = repos.WriterOf<Pages>())
            {

            }
        }

        [Test]
        public void DocumentTransactionParalelTest()
        {
            var size = 5;
            var pages = TestSetup.GeneratePages(size, false);
            var repos = new ObjectiksOf();
            var meta_before = repos.GetTypeMeta<Pages>();

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

        [Test]
        public void DocumentTransactionTest()
        {
            var size = 5;
            var pages = TestSetup.GeneratePages(size, false);
            var repos = new ObjectiksOf();

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
                        pageWriter.UsePartialStore(1);
                        pageWriter.AddDocuments(pages);
                        pageWriter.SubmitChanges();
                    }


                    //throw new Exception("opss");

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback(ex);
                }
            }
        }

        [Test]
        public void DocumentSequence()
        {
            var repos = new ObjectiksOf();
            var meta = repos.GetTypeMeta("pages");

            var int_ = meta.GetNewSequenceId(typeof(int));
            var long_ = meta.GetNewSequenceId(typeof(long));
            var guid_ = meta.GetNewSequenceId(typeof(Guid));
        }

        [Test]
        public void DocumentWriterBulk()
        {
            var size = 5;
            var pages = TestSetup.GeneratePages(size, false);
            var repos = new ObjectiksOf();
            var countBefore = repos.Count("pages");

            using (var writer = repos.WriterOf<Pages>())
            {
                writer.AddDocuments(pages);
                writer.UseFormatting();
                writer.SubmitChanges();
            }

            var countAfter = repos.Count<Pages>();
            var diff = countAfter - countBefore;

            Assert.True(diff == size);
        }

        [Test]
        public void DocumentWriterBulkPartial()
        {
            var pages = TestSetup.GeneratePages(5000);
            var repos = new ObjectiksOf();
            using (var writer = repos.WriterOf<Pages>())
            {
                writer.UsePartialStore(1000);
                writer.AddDocuments(pages);
                writer.UseFormatting();
                writer.SubmitChanges();
            }
        }

        [Test]
        public void DocumentWriter()
        {
            var repos = new ObjectiksOf();

            var meta_first = repos.GetTypeMeta<Pages>();
            var count_first = meta_first.TotalRecords;
            var keys_first = meta_first.Keys.Count;

            var mergePage = repos
             .TypeOf<Pages>()
             .PrimaryOf(1)
             .First();

            mergePage.Title = "Merge";

            var pages = TestSetup.GeneratePages(3);

            using (var writer = repos.WriterOf<Pages>())
            {
                writer.UsePartialStore(1);
                writer.UseFormatting();

                foreach (var page in pages)
                {
                    writer.AddDocument(page);
                }

                writer.AddDocument(mergePage);

                writer.SubmitChanges();
            }

            var getPage = repos.TypeOf<Pages>()
                .PrimaryOf(pages[0].Id)
                .First();

            var meta_end = repos.GetTypeMeta<Pages>();
            var count_end = meta_end.TotalRecords;
            var keys_end = meta_end.Keys.Count;

            var removePages = TestSetup.GeneratePages(12);

            using (var writer2 = repos.WriterOf<Pages>())
            {
                writer2.UsePartialStore(2);
                writer2.AddDocuments(removePages);
                writer2.SubmitChanges();
            }

            using (var writerDel = repos.WriterOf<Pages>())
            {
                writerDel.Delete(removePages);
                writerDel.SubmitChanges();
            }
        }

        [Test]
        public void DocumentWriterMerge()
        {
            var repos = new ObjectiksOf(TestSetup.Options);

            var meta_first = repos.GetTypeMeta<Pages>();

            var mergePage = repos
           .TypeOf<Pages>()
           .PrimaryOf(1)
           .First();

            mergePage.Title = "Merge ile değiştireceğiz.";

            using (var writer = repos.WriterOf<Pages>())
            {
                writer.UseFormatting();

                writer.AddDocument(mergePage);

                writer.SubmitChanges();
            }

            var meta_end = repos.GetTypeMeta<Pages>();
        }

        [Test]
        public void DocumentReader()
        {
            var repos = new ObjectiksOf(TestSetup.Options);

            var first = repos
             .TypeOf("Pages")
             .PrimaryOf(1)
             .First();

            var list_01 = repos
              .TypeOf("Pages")
              .OrderBy("Title")
              .Desc()
              .ToList();

            var list_02 = repos
             .TypeOf("Pages")
             .PrimaryOf(1)
             .PrimaryOf(3)
             .KeyOf("en")
             .OrderBy("Title")
             .Desc()
             .Any()
             .ToList();

            var tr = repos
            .TypeOf("Pages")
            .KeyOf("tr")
            .ToList();

            var ne = repos
            .TypeOf("Pages")
            .KeyOf("en")
            .ToList();

            var home = repos
            .TypeOf("Pages")
            .KeyOf("home")
            .ToList();

            Assert.Pass();
        }

        [Test]
        public void DocumentReaderDynamicRef()
        {
            var repos = new ObjectiksOf(TestSetup.Options);

            var refs_1 = new
            {
                ParseOf = "M:M",
                TypeOf = "Sections",
                MapOf = new
                {
                    Target = "DynamicRefs"
                }
            };

            var refs_2 = new
            {
                ParseOf = "1:M",
                TypeOf = "Sections",
                KeyOf = new
                {
                    Source = new string[] { "Language" },
                    Target = new string[] { "Language" }
                },
                MapOf = new
                {
                    Source = "Name",
                    Target = "Sections"
                }
            };

            var second = repos
               .TypeOf("pages")
               .PrimaryOf(2)
               .Refs(refs_1)
               .Refs(refs_2)
               .First();

        }

        [Test]
        public void DocumentWriteDelete()
        {
            var repos = new ObjectiksOf(TestSetup.Options);

            var meta_first = repos.GetTypeMeta<Pages>();

            var mergePage = repos
           .TypeOf<Pages>()
           .PrimaryOf(1)
           .First();

            mergePage.Title = "Delete";

            using (var writer = repos.WriterOf<Pages>())
            {

                writer.UseFormatting();
                writer.Delete(mergePage);

                writer.SubmitChanges();
            }

            var meta_end = repos.GetTypeMeta<Pages>();
        }
    }
}