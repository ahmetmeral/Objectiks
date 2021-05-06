using NUnit.Framework;
using Objectiks.Engine;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Integrations
{
    public class NoDbEngineTest
    {
        [SetUp]
        public void Setup() { }

        [Test]
        public void QueryBuilder()
        {
            // result = Keys.AsQueryable().Where(query.AsWhere(), query.AsWhereParameters())?.ToList();

            var query = new DocumentQuery();
            

        }
    }
}
