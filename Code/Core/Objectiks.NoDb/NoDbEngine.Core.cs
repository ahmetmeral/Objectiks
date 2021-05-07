using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.NoDb
{
    public partial class NoDbEngine : DocumentEngine
    {
        public NoDbEngine()
            : base() { }

        public NoDbEngine(DocumentProvider documentProvider, DocumentOption option) 
            : base(documentProvider, option) { }

        public NoDbEngine(DocumentProvider documentProvider, NoDbDocumentOption option)
            : base(documentProvider, option)
        {
        }
    }
}
