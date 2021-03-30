using NUnit.Framework;
using Objectiks.Engine.Query;
using Objectiks.Integrations.Models;
using System;
using System.IO;

namespace Objectiks.Integrations
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Order(1)]
        [Test]
        public void DocumentSequence()
        {
            var repos = new ObjectiksOf(TestSetup.Options);
            var meta = repos.GetTypeMeta("pages");

            var int_ = meta.GetNewSequenceId(typeof(int));
            var long_ = meta.GetNewSequenceId(typeof(long));
            var guid_ = meta.GetNewSequenceId(typeof(Guid));
            var str_ = meta.GetNewSequenceId(typeof(string));
        }

        [Test(Description = "Document Reader Test")]
        public void DocumentReaderTest()
        {
            var repos = new ObjectiksOf();
            var page = repos
             .TypeOf<Pages>()
             .PrimaryOf(1)
             .First();
        }

        [Order(2)]
        [Test(Description = "Document Writer Bulk Test")]
        public void DocumentWriterBulk()
        {
            var pages = TestSetup.GeneratePages(5000);
            var repos = new ObjectiksOf(TestSetup.Options);
            using (var writer = repos.WriterOf<Pages>())
            {
                writer.UsePartialStore(1000);
                writer.Add(pages);
                writer.UseFormatting();
                writer.SubmitChanges();
            }
        }

        [Order(3)]
        [Test(Description = "Document Writer Test")]
        public void DocumentWriter()
        {
            var repos = new ObjectiksOf(TestSetup.Options);

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

        [Order(4)]
        [Test(Description = "Document Writer Merge")]
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

        [Order(5)]
        [Test(Description = "Document Reader Test")]
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

        [Order(6)]
        [Test(Description = "Document Reader Dynamic Refs")]
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

        [Order(100)]
        [Test(Description = "Document Writer Delete")]
        public void DocumentWriteDelete()
        {
            var repos = new ObjectiksOf(TestSetup.Options);

            var meta_first = repos.GetTypeMeta<Pages>();

            var mergePage = repos
           .TypeOf<Pages>()
           .PrimaryOf(1)
           .First();

            mergePage.Title = "Delete ile değiştireceğiz.";

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