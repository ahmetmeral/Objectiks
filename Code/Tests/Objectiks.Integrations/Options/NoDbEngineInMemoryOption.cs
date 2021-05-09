using Objectiks.Caching;
using Objectiks.Caching.Serializer;
using Objectiks.Engine;
using Objectiks.Integrations.Models;
using Objectiks.Models;
using Objectiks.NoDb;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Integrations.Option
{
    public class NoDbEngineInMemoryOption : NoDbDocumentOption
    {
        public NoDbEngineInMemoryOption() : base()
        {
            Name = "NoDbEngineProvider";

            RegisterTypeOf<Pages>();
            RegisterTypeOf<Tags>();

            UseDocumentWatcher<DocumentWatcher>();
        }
    }
}
