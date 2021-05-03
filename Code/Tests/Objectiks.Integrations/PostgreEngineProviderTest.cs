using Npgsql;
using NUnit.Framework;
using Objectiks.Integrations.Models;
using Objectiks.Integrations.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Integrations
{
    //public class PostgreEngineProviderTest : DocumentEngineTestBase
    //{
    //    [SetUp]
    //    public override void Setup()
    //    {
    //        ObjectiksOf.Core.Map(typeof(NpgsqlConnection), new PostgreEngineRedisOption());
    //    }

    //    [Test]
    //    public void PostgreSqlProvider()
    //    {
    //        var connectionString = "Host=localhost;Port=5432;Database=objectiks;User id=postgres;Password=data1;SslMode=Disable;";
    //        var conn = new NpgsqlConnection(connectionString);
    //        var repos = new ObjectiksOf(conn);
    //        var meta = repos.GetTypeMeta<Pages>();
    //        var page = repos.TypeOf<Pages>().PrimaryOf(1).First();


    //        using (var writer = repos.WriterOf<Pages>())
    //        {

    //        }
    //    }
    //}
}
