using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.PostgreSql
{
    public class PostgreDocumentOption : DocumentOption
    {
        public PostgreDocumentOption() : base()
        {
            Name = "PostgreDocuments";
            SqlProviderSchema = "public";
            SqlProviderSchemaSeperator = ".";

            UseEngineProvider<PostgreEngine>();
            RegisterDefaults();
        }
    }
}
