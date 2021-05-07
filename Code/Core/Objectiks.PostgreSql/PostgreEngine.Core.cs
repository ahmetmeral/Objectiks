using Newtonsoft.Json.Linq;
using Npgsql;
using Npgsql.Json;
using Objectiks.Engine;
using Objectiks.Engine.Query;
using Objectiks.Extentions;
using Objectiks.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace Objectiks.PostgreSql
{
    public partial class PostgreEngine : DocumentEngine
    {
        public PostgreEngine()
            : base() { }

        public PostgreEngine(DocumentProvider documentProvider, DocumentOption option)
            : base(documentProvider, option) { }

        public PostgreEngine(DocumentProvider documentProvider, PostgreDocumentOption option)
            : base(documentProvider, option)
        {
        }


    }
}
