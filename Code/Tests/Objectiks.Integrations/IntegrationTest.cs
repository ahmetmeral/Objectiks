using Npgsql;
using NUnit.Framework;
using Objectiks.Engine.Query;
using Objectiks.Integrations.Models;
using Objectiks.Integrations.Option;
using Objectiks.Integrations.Options;
using Objectiks.Models;
using Objectiks.PostgreSql;
using System;
using System.Data.SqlClient;
using System.IO;

namespace Objectiks.Integrations
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            ObjectiksOf.Core.Map(typeof(DocumentProvider), new FileProviderOption());
            ObjectiksOf.Core.Map(typeof(NpgsqlConnection), new PostgreSqlProviderOption());
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
        public void DocumentTransactionTest()
        {
            var size = 5;
            var pages = TestSetup.GeneratePages(size);
            var repos = new ObjectiksOf();

            using (var trans = repos.BeginTransaction())
            {
                try
                {
                    using (var pageWriter = repos.WriterOf<Pages>())
                    {
                        pageWriter.Add(pages);

                        pageWriter.SubmitChanges();
                    }

                    using (var categoryWriter = repos.WriterOf<Categories>())
                    {
                        categoryWriter.Add(new Categories
                        {
                            Name = "Transactions",
                            Description = "test",
                            Title = "Test"
                        });

                        categoryWriter.SubmitChanges();
                    }

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
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
            var pages = TestSetup.GeneratePages(size);
            var repos = new ObjectiksOf();
            var countBefore = repos.Count("pages");

            using (var writer = repos.WriterOf<Pages>())
            {
                writer.Add(pages);
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
                writer.Add(pages);
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
                    writer.Add(page);
                }

                writer.Add(mergePage);

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
                writer2.Add(removePages);
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

                writer.Add(mergePage);

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