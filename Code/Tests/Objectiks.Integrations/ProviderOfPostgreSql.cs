using Objectiks.Engine;
using Objectiks.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Integrations
{
    public class ProviderOfPostgreSql : DocumentProvider
    {
        public ProviderOfPostgreSql(DocumentManifest manifest, DocumentOptions options,
            IDocumentCache cache, IDocumentLogger logger, IDocumentWatcher watcher)
            : base(manifest, options, cache, logger, watcher)
        {

        }

        public override bool LoadDocumentType(string typeOf, bool isCheckLoadedSkip)
        {
            return base.LoadDocumentType(typeOf);
        }
    }
}
