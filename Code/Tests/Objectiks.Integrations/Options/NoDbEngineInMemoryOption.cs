using Objectiks.Caching;
using Objectiks.Caching.Serializer;
using Objectiks.Engine;
using Objectiks.Integrations.Models;
using Objectiks.Integrations.Parsers;
using Objectiks.Models;
using Objectiks.NoDb;
using Objectiks.Services;
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

            RegisterParseOf<PageDocumentParser>();

            SupportDocumentParser = true;

            //UseDocumentLogger<DocumentLogger>();
            //UseDocumentWatcher<DocumentWatcher>();
        }
    }

   
}
