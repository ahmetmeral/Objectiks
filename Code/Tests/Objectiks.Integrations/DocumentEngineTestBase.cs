using NUnit.Framework;
using Objectiks.Integrations.Models;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;

namespace Objectiks.Integrations
{
    public abstract class DocumentEngineTestBase
    {
        public abstract void Setup();

        [Test]
        public void DocumentWorkOfUserOfKeyOfPrimaryOf()
        {
            var repos = new ObjectiksOf();

            repos.TruncateOf<Pages>();

            var pages = TestSetup.GeneratePages(10, false, true);

            using (var writer = repos.WriterOf<Pages>())
            {
                writer.UseFormatting();
                writer.AddDocuments(pages);
                writer.SubmitChanges();
            }

            Assert.IsTrue(pages.Count == repos.Count<Pages>());

            var select_first = pages[0];
            var select_second = pages[1];

            var workOfResults = repos
                .TypeOf<Pages>()
                .WorkOf(select_first.WorkSpaceRef)
                .ToList();

            int workOfResultsCheckCount = pages.Count(p => p.WorkSpaceRef == select_first.WorkSpaceRef);
            Assert.IsTrue(workOfResults.Count == workOfResultsCheckCount);

            var keyOfResults = repos
                .TypeOf<Pages>()
                .KeyOf(select_first.Tag)
                .ToList();

            int keyOfResultsCheckCount = pages.Count(p => p.Tag == select_first.Tag);
            Assert.IsTrue(keyOfResults.Count == keyOfResultsCheckCount);

            var primaryOfResults = repos
                .TypeOf<Pages>()
                .PrimaryOf(select_first.Id)
                .PrimaryOf(select_second.Id)
                .Any()
                .ToList();

            int primaryOfResultsCheckCount = pages.Count(p => p.Id == select_first.Id || p.Id == select_second.Id);
            Assert.IsTrue(primaryOfResults.Count == primaryOfResultsCheckCount);

            var primaryOfFirstResult = repos
                .TypeOf<Pages>()
                .PrimaryOf(select_first.Id)
                .First();

            Assert.NotNull(primaryOfFirstResult);

            var multiple = repos
                .TypeOf<Pages>()
                .WorkOf(select_first.WorkSpaceRef)
                .KeyOf(select_first.Tag)
                .PrimaryOf(select_first.Id)
                .PrimaryOf(select_second.Id)
                .Any()
                .ToList();

            int multipleCheckCount = pages.Count(
                    p => p.WorkSpaceRef == select_first.WorkSpaceRef
                    && p.Tag.Contains(select_first.Tag)
                    && (p.Id == select_first.Id || p.Id == select_second.Id)
                    );

            Assert.IsTrue(multiple.Count == multipleCheckCount);
        }

        [Test]
        public void RepositoryCacheOf()
        {
            bool callBeforeCacheOf = true;

            var repos = new ObjectiksOf();

            var page = repos.TypeOf<Pages>()
                .PrimaryOf(1)
                .CacheOf("test", callBeforeCacheOf)
                .First();

            var page_list = repos.TypeOf<Pages>()
              .CacheOf(callBeforeCacheOf)
              .ToList();

            var tags = repos.TypeOf<Tags>().CacheOf().ToList();

            var tags_first = repos
                .TypeOf<Tags>()
                .PrimaryOf(1)
                .PrimaryOf(2)
                .CacheOf(callBeforeCacheOf)
                .ToList();
        }

        [Test]
        public void DocumentWriter()
        {
            var size = 500;
            var pages = TestSetup.GeneratePages(size, false);
            var repos = new ObjectiksOf();

            repos.TruncateOf<Pages>();

            var count_before = repos.Count<Pages>();

            using (var writer = repos.WriterOf<Pages>())
            {
                writer.UseFormatting();

                foreach (var page in pages)
                {
                    writer.AddDocument(page);
                }

                writer.SubmitChanges();
            }

            var count_after = repos.Count<Pages>();

            Assert.IsTrue((count_after - count_before) == size);
        }

        [Test]
        public void DocumentUpdate()
        {
            var pages = TestSetup.GeneratePages(1, false);
            var repos = new ObjectiksOf();

            //removes
            repos.TruncateOf<Pages>();

            using (var writer = repos.WriterOf<Pages>())
            {
                writer.UseFormatting();
                writer.AddDocuments(pages);
                writer.SubmitChanges();
            }

            int pageID = pages[0].Id;
            var page_update_before = repos.TypeOf<Pages>().PrimaryOf(pageID).First();
            var meta_before = repos.GetTypeMeta<Pages>();

            Assert.NotNull(page_update_before);

            page_update_before.Title = "Updated Title";

            using (var writer = repos.WriterOf<Pages>())
            {
                writer.UseFormatting();
                writer.UpdateDocument(page_update_before);
                writer.SubmitChanges();
            }

            var meta_after = repos.GetTypeMeta<Pages>();

            var page_update_after = repos.First<Pages>(pageID);

            Assert.IsTrue(page_update_before.Title == page_update_after.Title);
        }

        [Test]
        public void DocumentReader()
        {
            var pages = TestSetup.GeneratePages(10, false);

            var repos = new ObjectiksOf();

            var get_pages = repos.TypeOf<Pages>().ToList();
            var meta = repos.GetTypeMeta<Pages>();

            //get page count..
            var numberOfRecordBefore = repos.Count<Pages>();

            //all pages remove..
            var numberOfRecordDelete = repos.TypeOf<Pages>().Delete();

            Assert.IsTrue(numberOfRecordBefore == numberOfRecordDelete);

            var numberOfRecordAfter = repos.Count<Pages>();

            //check delete success..
            Assert.IsTrue(numberOfRecordAfter == 0);

            //add pages..
            using (var writer = repos.WriterOf<Pages>())
            {
                writer.UseFormatting();
                writer.AddDocuments(pages);
                writer.SubmitChanges();
            }

            var list = repos.TypeOf<Pages>().ToList();

            Assert.IsTrue(list.Count == pages.Count);

            var userPages = repos
                .TypeOf<Pages>()
                .KeyOf("FakeKeyOf")
                .ToList();

            Assert.IsTrue(userPages.Count == 0);

            var keyOfTag = list[0].Tag;
            var keyOfTagNumberOfCount = list.Count(l => l.Tag == keyOfTag);

            Debug.WriteLine($"KeyOf:{keyOfTag}");

            var keyOfPages = repos.TypeOf<Pages>().KeyOf(keyOfTag).ToList();

            Assert.IsTrue(keyOfPages.Count == keyOfTagNumberOfCount);
        }

        [Test]
        public void DocumentKeyOfAfterWriterQueryExceptionTest()
        {
            var repos = new ObjectiksOf();

            repos.TruncateOf<Pages>();

            var adminPages_before_writer = repos
             .TypeOf<Pages>()
             .KeyOf("Admin")
             .OrderBy("Title")
             .Desc()
             .ToList();

            var pages = TestSetup.GeneratePages(10, false);

            using (var writer = repos.WriterOf<Pages>())
            {
                writer.UseFormatting();
                writer.AddDocuments(pages);
                writer.SubmitChanges();
            }

            try
            {
                var adminPages_after_writer = repos
                     .TypeOf<Pages>()
                     .KeyOf("Admin")
                     .OrderBy("Title")
                     .Desc()
                     .ToList();
            }
            catch
            {
                Assert.IsTrue(false, "KeyOf value in document is empty");
            }
        }

        [Test]
        public void DocumentDelete()
        {
            var pages = TestSetup.GeneratePages(1, false);

            var repos = new ObjectiksOf();

            //removes
            repos.TruncateOf<Pages>();

            using (var create = repos.WriterOf<Pages>())
            {
                create.UseFormatting();
                create.AddDocument(pages[0]);
                create.SubmitChanges();
            }

            var page_after_create = repos.First<Pages>(pages[0].Id);

            Assert.IsNotNull(page_after_create);

            using (var writer = repos.WriterOf<Pages>())
            {
                writer.UseFormatting();
                writer.DeleteDocument(pages[0]);
                writer.SubmitChanges();
            }

            var page_after_delete = repos.First<Pages>(pages[0].Id);

            Assert.IsNull(page_after_delete);
        }

        [Test]
        public void DocumentBulkWrite()
        {
            var size = 500;
            var pages = TestSetup.GeneratePages(size, false);
            var repos = new ObjectiksOf();

            //removes
            repos.TruncateOf<Pages>();

            using (var writer = repos.WriterOf<Pages>())
            {
                writer.UseFormatting();
                writer.AddDocuments(pages);
                writer.SubmitChanges();
            }

            var numberOfRecord = repos.Count<Pages>();

            Assert.True(numberOfRecord == size);
        }

        [Test]
        public void DocumentTransactionInternalTest()
        {
            var size = 50;
            var pages = TestSetup.GeneratePages(size, false);
            var repos = new ObjectiksOf();

            repos.TruncateOf<Pages>();

            using (var writer = repos.WriterOf<Pages>())
            {
                writer.AddDocuments(pages);
                writer.SubmitChanges();
            }

            Assert.IsTrue(repos.Count<Pages>() == size);
        }

        [Test]
        public void DocumentTransactionTest()
        {
            var size = 5;
            var pages = TestSetup.GeneratePages(size, false);
            var repos = new ObjectiksOf();

            //truncate types..
            using (var trans = repos.BeginTransaction())
            {
                try
                {
                    using (var pageWriter = repos.WriterOf<Pages>())
                    {
                        pageWriter.Truncate();
                    }

                    using (var tagWriter = repos.WriterOf<Tags>())
                    {
                        tagWriter.Truncate();
                    }

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback(ex);
                }
            }

            using (var trans = repos.BeginTransaction())
            {
                try
                {
                    var tag = new Tags
                    {
                        Name = "Tag1"
                    };

                    using (var tagWriter = repos.WriterOf<Tags>())
                    {
                        tagWriter.UseFormatting();
                        tagWriter.AddDocument(tag);
                        tagWriter.SubmitChanges();
                    }

                    using (var pageWriter = repos.WriterOf<Pages>())
                    {
                        pageWriter.UseFormatting();
                        pageWriter.AddDocuments(pages);
                        pageWriter.SubmitChanges();
                    }

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback(ex);
                }
            }

            var count = repos.Count<Pages>();

            Assert.IsTrue(count == size);
            Assert.IsTrue(repos.Count<Tags>() == 1);
        }

        [Test]
        public void DocumentTransactionInternalParalelTest()
        {
            var size = 500;
            var pages = TestSetup.GeneratePages(size, false);
            var repos = new ObjectiksOf();

            repos.TruncateOf<Pages>();
            repos.TruncateOf<Tags>();

            var result = Parallel.ForEach(pages, page =>
            {
                using (var pageWriter = repos.WriterOf<Pages>())
                {
                    pageWriter.UseFormatting();
                    pageWriter.AddDocument(page);
                    pageWriter.SubmitChanges();
                }
            });

            Assert.IsTrue(repos.Count<Pages>() == size);
        }

        [Test]
        public void DocumentTransactionGlobalParalelTest()
        {
            var size = 500;
            var pages = TestSetup.GeneratePages(size, false);
            var repos = new ObjectiksOf();

            repos.TruncateOf<Pages>();
            repos.TruncateOf<Tags>();

            var result = Parallel.ForEach(pages, page =>
            {
                using (var trans = repos.BeginTransaction())
                {
                    try
                    {
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

            Assert.IsTrue(repos.Count<Pages>() == size);
        }



        [Test]
        public void DocumentBulkWritePartial()
        {
            var size = 50;
            var pages = TestSetup.GeneratePages(size, false);
            var repos = new ObjectiksOf();

            repos.TruncateOf<Pages>();

            using (var writer = repos.WriterOf<Pages>())
            {
                writer.UsePartialStore(25);
                writer.UseFormatting();
                writer.AddDocuments(pages);
                writer.SubmitChanges();
            }

            Assert.IsTrue(repos.Count<Pages>() == size);
        }

        public void DocumenRefs()
        {
            var repos = new ObjectiksOf();

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

            var page = repos
               .TypeOf("pages")
               .PrimaryOf(2)
               .Refs(refs_1)
               .Refs(refs_2)
               .First();
        }
    }
}
