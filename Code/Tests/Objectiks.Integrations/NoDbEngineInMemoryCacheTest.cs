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
    public class NoDbEngineInMemoryCacheTest : DocumentEngineTestBase
    {
        [SetUp]
        public override void Setup()
        {
            ObjectiksOf.Core.Map(typeof(DocumentProvider), new NoDbEngineInMemoryOption());
        }
    }
}