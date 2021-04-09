using NUnit.Framework;
using Objectiks.Integrations.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Integrations
{
    public class MultipleProviderTest
    {
        [SetUp]
        public void Setup() { }

        [Test]
        public void ProviderTest()
        {
            var options = new ObjectiksOptions();
            options.ClearProviderOf();
            options.AddProviderOf<DocumentProvider>("Default");
            options.AddProviderOf<DocumentProvider>("Default2");

            var repo = new ObjectiksOf(options);
            var meta = repo.GetTypeMeta<Pages>();
            var all = repo.GetTypeMetaAll();


            var all_status = ObjectiksOf.Core.GetTypeOfStatusAll();

        }
    }
}
