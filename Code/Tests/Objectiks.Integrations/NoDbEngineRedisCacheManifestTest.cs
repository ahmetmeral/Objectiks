using NUnit.Framework;
using Objectiks.Caching.Serializer;
using Objectiks.Engine;
using Objectiks.Integrations.Models;
using Objectiks.Integrations.Options;
using Objectiks.Redis;


using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Objectiks.Integrations
{
    public class NoDbEngineRedisCacheManifestTest : DocumentEngineTestBase
    {
        [SetUp]
        public override void Setup()
        {
            ObjectiksOf.Core.Initialize(new DocumentProvider(), new NoDbEngineWithManifest());
        }
    }
}
